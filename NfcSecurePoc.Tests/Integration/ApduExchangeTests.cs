using System.Text;
using NfcSecurePoc.Apdu;
using NfcSecurePoc.Crypto;

namespace NfcSecurePoc.Tests.Integration;

/// <summary>
/// Simulates a full APDU exchange (Reader ↔ HCE) without NFC hardware.
/// Validates the protocol logic: SELECT → GET_DATA → PUT_DATA.
/// </summary>
public class ApduExchangeTests
{
    private readonly AesCryptoService _crypto = new();

    [Fact]
    public void FullExchange_SelectGetDataPutData_Succeeds()
    {
        var hcePayload = "Secret NFC Data";

        // === 1. SELECT AID ===
        var selectCmd = ApduCommand.Select(ApduConstants.Aid);
        var selectResponse = SimulateHceSelect(selectCmd);
        var selectResp = new ApduResponse(selectResponse);

        Assert.True(selectResp.IsSuccess);
        Assert.Equal(ApduConstants.VersionString, Encoding.UTF8.GetString(selectResp.Data));

        // === 2. GET DATA ===
        var getDataCmd = ApduCommand.GetData();
        var getDataResponse = SimulateHceGetData(getDataCmd, hcePayload);
        var getDataResp = new ApduResponse(getDataResponse);

        Assert.True(getDataResp.IsSuccess);

        // Reader decrypts the payload
        var decrypted = _crypto.Decrypt(getDataResp.Data);
        Assert.Equal(hcePayload, Encoding.UTF8.GetString(decrypted));

        // === 3. PUT DATA ===
        var ackPayload = $"ACK: {hcePayload}";
        var encrypted = _crypto.Encrypt(Encoding.UTF8.GetBytes(ackPayload));
        var putDataCmd = ApduCommand.PutData(encrypted);
        var putDataResponse = SimulateHcePutData(putDataCmd);
        var putDataResp = new ApduResponse(putDataResponse);

        Assert.True(putDataResp.IsSuccess);
    }

    [Fact]
    public void Select_WrongAid_ReturnsFileNotFound()
    {
        var wrongAid = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD };
        var selectCmd = ApduCommand.Select(wrongAid);
        var response = SimulateHceSelect(selectCmd);
        var resp = new ApduResponse(response);

        Assert.False(resp.IsSuccess);
        Assert.Equal(ApduConstants.SwFileNotFound, resp.StatusWord);
    }

    [Fact]
    public void GetData_DifferentPayloads_AllDecryptCorrectly()
    {
        var payloads = new[]
        {
            "Short",
            "A medium length payload for testing",
            "Un payload avec des caractères spéciaux: éàü 日本語",
            new string('X', 200) // long payload
        };

        foreach (var payload in payloads)
        {
            var cmd = ApduCommand.GetData();
            var response = SimulateHceGetData(cmd, payload);
            var resp = new ApduResponse(response);

            Assert.True(resp.IsSuccess);
            var decrypted = _crypto.Decrypt(resp.Data);
            Assert.Equal(payload, Encoding.UTF8.GetString(decrypted));
        }
    }

    [Fact]
    public void PutData_CorruptedEncryption_ReturnsError()
    {
        var encrypted = _crypto.Encrypt(Encoding.UTF8.GetBytes("Test"));
        // Corrupt the ciphertext
        encrypted[20] ^= 0xFF;

        var cmd = ApduCommand.PutData(encrypted);
        var response = SimulateHcePutData(cmd);
        var resp = new ApduResponse(response);

        Assert.False(resp.IsSuccess);
        Assert.Equal(ApduConstants.SwConditionsNotSatisfied, resp.StatusWord);
    }

    // --- HCE simulation (mirrors AndroidHceService logic) ---

    private byte[] SimulateHceSelect(byte[] commandApdu)
    {
        if (commandApdu.Length < 5)
            return ApduResponse.Build(ApduConstants.SwWrongLength);

        var lc = commandApdu[4];
        if (commandApdu.Length < 5 + lc)
            return ApduResponse.Build(ApduConstants.SwWrongLength);

        var receivedAid = commandApdu[5..(5 + lc)];

        if (!receivedAid.AsSpan().SequenceEqual(ApduConstants.Aid))
            return ApduResponse.Build(ApduConstants.SwFileNotFound);

        var versionBytes = Encoding.UTF8.GetBytes(ApduConstants.VersionString);
        return ApduResponse.Build(versionBytes, ApduConstants.SwSuccess);
    }

    private byte[] SimulateHceGetData(byte[] commandApdu, string payload)
    {
        var plaintext = Encoding.UTF8.GetBytes(payload);
        var encrypted = _crypto.Encrypt(plaintext);
        return ApduResponse.Build(encrypted, ApduConstants.SwSuccess);
    }

    private byte[] SimulateHcePutData(byte[] commandApdu)
    {
        if (commandApdu.Length < 5)
            return ApduResponse.Build(ApduConstants.SwWrongLength);

        var lc = commandApdu[4];
        if (commandApdu.Length < 5 + lc)
            return ApduResponse.Build(ApduConstants.SwWrongLength);

        var encryptedData = commandApdu[5..(5 + lc)];

        try
        {
            _crypto.Decrypt(encryptedData);
            return ApduResponse.Build(ApduConstants.SwSuccess);
        }
        catch
        {
            return ApduResponse.Build(ApduConstants.SwConditionsNotSatisfied);
        }
    }
}

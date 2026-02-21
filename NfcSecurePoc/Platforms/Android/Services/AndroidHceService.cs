using Android.App;
using Android.Content;
using Android.Nfc.CardEmulators;
using Android.OS;
using NfcSecurePoc.Apdu;
using NfcSecurePoc.Crypto;
using System.Text;

namespace NfcSecurePoc.Platforms.Android.Services;

/// <summary>
/// Android HostApduService implementation.
/// The OS instantiates this service directly, so we use static events
/// to bridge communication to the DI-managed wrapper.
/// </summary>
[Service(
    Exported = true,
    Permission = "android.permission.BIND_NFC_SERVICE"),
 IntentFilter(new[] { "android.nfc.cardemulation.action.HOST_APDU_SERVICE" }),
 MetaData("android.nfc.cardemulation.host_apdu_service", Resource = "@xml/apduservice")]
public class AndroidHceService : HostApduService
{
    private static string _payload = "Hello from NFC-POC!";
    private static bool _isActive;
    private static readonly AesCryptoService Crypto = new();

    // Static events bridged to the wrapper
    internal static event EventHandler<(byte[] Apdu, string Description)>? StaticApduReceived;
    internal static event EventHandler<(byte[] Apdu, string Description)>? StaticApduResponseSent;

    public static bool IsActive => _isActive;

    public static void SetPayload(string payload) => _payload = payload;
    public static void SetActive(bool active) => _isActive = active;

    public override byte[] ProcessCommandApdu(byte[]? commandApdu, Bundle? extras)
    {
        if (commandApdu == null || commandApdu.Length < 4)
            return ApduResponse.Build(ApduConstants.SwConditionsNotSatisfied);

        var ins = commandApdu[1];

        switch (ins)
        {
            case ApduConstants.InsSelect:
                return HandleSelect(commandApdu);

            case ApduConstants.InsGetData:
                return HandleGetData(commandApdu);

            case ApduConstants.InsPutData:
                return HandlePutData(commandApdu);

            default:
                RaiseReceived(commandApdu, $"Unknown INS: 0x{ins:X2}");
                return ApduResponse.Build(ApduConstants.SwInsNotSupported);
        }
    }

    private byte[] HandleSelect(byte[] commandApdu)
    {
        RaiseReceived(commandApdu, "SELECT AID");

        // Extract AID from command
        if (commandApdu.Length < 5)
            return BuildAndNotify(ApduConstants.SwWrongLength, "SELECT: wrong length");

        var lc = commandApdu[4];
        if (commandApdu.Length < 5 + lc)
            return BuildAndNotify(ApduConstants.SwWrongLength, "SELECT: data truncated");

        var receivedAid = commandApdu[5..(5 + lc)];

        if (!receivedAid.AsSpan().SequenceEqual(ApduConstants.Aid))
            return BuildAndNotify(ApduConstants.SwFileNotFound, "SELECT: AID mismatch");

        var versionBytes = Encoding.UTF8.GetBytes(ApduConstants.VersionString);
        var response = ApduResponse.Build(versionBytes, ApduConstants.SwSuccess);
        RaiseSent(response, $"SELECT OK → {ApduConstants.VersionString}");
        return response;
    }

    private byte[] HandleGetData(byte[] commandApdu)
    {
        RaiseReceived(commandApdu, "GET DATA");

        var plaintext = Encoding.UTF8.GetBytes(_payload);
        var encrypted = Crypto.Encrypt(plaintext);

        var response = ApduResponse.Build(encrypted, ApduConstants.SwSuccess);
        RaiseSent(response, $"GET DATA → encrypted {encrypted.Length} bytes");
        return response;
    }

    private byte[] HandlePutData(byte[] commandApdu)
    {
        RaiseReceived(commandApdu, "PUT DATA");

        if (commandApdu.Length < 5)
            return BuildAndNotify(ApduConstants.SwWrongLength, "PUT DATA: wrong length");

        var lc = commandApdu[4];
        if (commandApdu.Length < 5 + lc)
            return BuildAndNotify(ApduConstants.SwWrongLength, "PUT DATA: data truncated");

        var encryptedData = commandApdu[5..(5 + lc)];

        try
        {
            var decrypted = Crypto.Decrypt(encryptedData);
            var text = Encoding.UTF8.GetString(decrypted);
            var response = ApduResponse.Build(ApduConstants.SwSuccess);
            RaiseSent(response, $"PUT DATA OK → decrypted: \"{text}\"");
            return response;
        }
        catch (Exception ex)
        {
            return BuildAndNotify(ApduConstants.SwConditionsNotSatisfied, $"PUT DATA decrypt error: {ex.Message}");
        }
    }

    private byte[] BuildAndNotify(ushort sw, string description)
    {
        var response = ApduResponse.Build(sw);
        RaiseSent(response, description);
        return response;
    }

    private static void RaiseReceived(byte[] apdu, string desc) =>
        StaticApduReceived?.Invoke(null, (apdu, desc));

    private static void RaiseSent(byte[] apdu, string desc) =>
        StaticApduResponseSent?.Invoke(null, (apdu, desc));

    public override void OnDeactivated(DeactivationReason reason)
    {
        // Card deactivated by reader or user switching
    }
}

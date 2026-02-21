using NfcSecurePoc.Apdu;

namespace NfcSecurePoc.Tests.Apdu;

public class ApduCommandTests
{
    [Fact]
    public void Select_BuildsCorrectApdu()
    {
        var cmd = ApduCommand.Select(ApduConstants.Aid);

        // [CLA, INS, P1, P2, Lc, AID...]
        Assert.Equal(5 + ApduConstants.Aid.Length, cmd.Length);
        Assert.Equal(ApduConstants.ClaIso, cmd[0]);
        Assert.Equal(ApduConstants.InsSelect, cmd[1]);
        Assert.Equal(ApduConstants.P1SelectByName, cmd[2]);
        Assert.Equal(ApduConstants.P2SelectFirst, cmd[3]);
        Assert.Equal((byte)ApduConstants.Aid.Length, cmd[4]);
        Assert.Equal(ApduConstants.Aid, cmd[5..]);
    }

    [Fact]
    public void Select_WithCustomAid_EmbeddsAidCorrectly()
    {
        var customAid = new byte[] { 0xA0, 0x00, 0x00, 0x01 };
        var cmd = ApduCommand.Select(customAid);

        Assert.Equal(9, cmd.Length); // 5 header + 4 AID
        Assert.Equal(4, cmd[4]);    // Lc
        Assert.Equal(customAid, cmd[5..]);
    }

    [Fact]
    public void GetData_BuildsCorrectApdu()
    {
        var cmd = ApduCommand.GetData();

        Assert.Equal(5, cmd.Length);
        Assert.Equal(ApduConstants.ClaIso, cmd[0]);
        Assert.Equal(ApduConstants.InsGetData, cmd[1]);
        Assert.Equal(0x00, cmd[2]); // P1
        Assert.Equal(0x00, cmd[3]); // P2
        Assert.Equal(0x00, cmd[4]); // Le = 0 (max)
    }

    [Fact]
    public void PutData_BuildsCorrectApdu()
    {
        var payload = new byte[] { 0x11, 0x22, 0x33, 0x44, 0x55 };
        var cmd = ApduCommand.PutData(payload);

        Assert.Equal(5 + payload.Length, cmd.Length);
        Assert.Equal(ApduConstants.ClaIso, cmd[0]);
        Assert.Equal(ApduConstants.InsPutData, cmd[1]);
        Assert.Equal(0x00, cmd[2]); // P1
        Assert.Equal(0x00, cmd[3]); // P2
        Assert.Equal((byte)payload.Length, cmd[4]); // Lc
        Assert.Equal(payload, cmd[5..]);
    }

    [Fact]
    public void PutData_EmptyPayload_HasLcZero()
    {
        var cmd = ApduCommand.PutData(Array.Empty<byte>());

        Assert.Equal(5, cmd.Length);
        Assert.Equal(0x00, cmd[4]); // Lc = 0
    }
}

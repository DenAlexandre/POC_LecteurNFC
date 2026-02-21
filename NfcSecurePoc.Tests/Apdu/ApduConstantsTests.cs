using NfcSecurePoc.Apdu;

namespace NfcSecurePoc.Tests.Apdu;

public class ApduConstantsTests
{
    [Fact]
    public void Aid_HasCorrectLength()
    {
        Assert.Equal(8, ApduConstants.Aid.Length);
    }

    [Fact]
    public void Aid_MatchesExpectedHex_F0504F432E4E4643()
    {
        var expected = new byte[] { 0xF0, 0x50, 0x4F, 0x43, 0x2E, 0x4E, 0x46, 0x43 };
        Assert.Equal(expected, ApduConstants.Aid);
    }

    [Fact]
    public void VersionString_IsNfcPocV1()
    {
        Assert.Equal("NFC-POC-v1", ApduConstants.VersionString);
    }

    [Theory]
    [InlineData(0x9000, 0x90, 0x00)]
    [InlineData(0x6A82, 0x6A, 0x82)]
    [InlineData(0x6700, 0x67, 0x00)]
    [InlineData(0x6985, 0x69, 0x85)]
    [InlineData(0x6D00, 0x6D, 0x00)]
    public void StatusWordToBytes_SplitsCorrectly(ushort sw, byte expectedSw1, byte expectedSw2)
    {
        var bytes = ApduConstants.StatusWordToBytes(sw);

        Assert.Equal(2, bytes.Length);
        Assert.Equal(expectedSw1, bytes[0]);
        Assert.Equal(expectedSw2, bytes[1]);
    }

    [Fact]
    public void InsSelect_Is0xA4()
    {
        Assert.Equal(0xA4, ApduConstants.InsSelect);
    }

    [Fact]
    public void InsGetData_Is0xCA()
    {
        Assert.Equal(0xCA, ApduConstants.InsGetData);
    }

    [Fact]
    public void InsPutData_Is0xDA()
    {
        Assert.Equal(0xDA, ApduConstants.InsPutData);
    }
}

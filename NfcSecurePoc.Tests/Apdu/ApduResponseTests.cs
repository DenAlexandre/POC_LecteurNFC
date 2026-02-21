using NfcSecurePoc.Apdu;

namespace NfcSecurePoc.Tests.Apdu;

public class ApduResponseTests
{
    [Fact]
    public void Constructor_ParsesStatusWordOnly()
    {
        var raw = new byte[] { 0x90, 0x00 };
        var resp = new ApduResponse(raw);

        Assert.Equal(0x90, resp.Sw1);
        Assert.Equal(0x00, resp.Sw2);
        Assert.Equal(ApduConstants.SwSuccess, resp.StatusWord);
        Assert.True(resp.IsSuccess);
        Assert.Empty(resp.Data);
    }

    [Fact]
    public void Constructor_ParsesDataAndStatusWord()
    {
        var raw = new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x90, 0x00 };
        var resp = new ApduResponse(raw);

        Assert.Equal(new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F }, resp.Data); // "Hello"
        Assert.True(resp.IsSuccess);
    }

    [Fact]
    public void Constructor_ParsesErrorStatusWord()
    {
        var raw = new byte[] { 0x6A, 0x82 };
        var resp = new ApduResponse(raw);

        Assert.Equal(ApduConstants.SwFileNotFound, resp.StatusWord);
        Assert.False(resp.IsSuccess);
        Assert.Empty(resp.Data);
    }

    [Fact]
    public void Constructor_ThrowsOnNullResponse()
    {
        Assert.Throws<ArgumentException>(() => new ApduResponse(null!));
    }

    [Fact]
    public void Constructor_ThrowsOnTooShortResponse()
    {
        Assert.Throws<ArgumentException>(() => new ApduResponse(new byte[] { 0x90 }));
        Assert.Throws<ArgumentException>(() => new ApduResponse(Array.Empty<byte>()));
    }

    [Fact]
    public void Build_WithData_CreatesCorrectResponse()
    {
        var data = new byte[] { 0x01, 0x02, 0x03 };
        var raw = ApduResponse.Build(data, ApduConstants.SwSuccess);

        Assert.Equal(5, raw.Length);
        Assert.Equal(data, raw[..3]);
        Assert.Equal(0x90, raw[3]);
        Assert.Equal(0x00, raw[4]);
    }

    [Fact]
    public void Build_StatusWordOnly_CreatesTwoBytes()
    {
        var raw = ApduResponse.Build(ApduConstants.SwFileNotFound);

        Assert.Equal(2, raw.Length);
        Assert.Equal(0x6A, raw[0]);
        Assert.Equal(0x82, raw[1]);
    }

    [Fact]
    public void Build_ThenParse_Roundtrips()
    {
        var data = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };
        var raw = ApduResponse.Build(data, ApduConstants.SwSuccess);
        var parsed = new ApduResponse(raw);

        Assert.Equal(data, parsed.Data);
        Assert.True(parsed.IsSuccess);
    }

    [Fact]
    public void Build_ErrorStatus_ThenParse_Roundtrips()
    {
        var raw = ApduResponse.Build(ApduConstants.SwInsNotSupported);
        var parsed = new ApduResponse(raw);

        Assert.Equal(ApduConstants.SwInsNotSupported, parsed.StatusWord);
        Assert.False(parsed.IsSuccess);
        Assert.Empty(parsed.Data);
    }
}

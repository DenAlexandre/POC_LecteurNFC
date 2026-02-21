namespace NfcSecurePoc.Services;

/// <summary>
/// Raised when an APDU command is received by the HCE service.
/// </summary>
public class ApduReceivedEventArgs : EventArgs
{
    public byte[] CommandApdu { get; }
    public string Description { get; }

    public ApduReceivedEventArgs(byte[] commandApdu, string description)
    {
        CommandApdu = commandApdu;
        Description = description;
    }
}

/// <summary>
/// Raised when the HCE service sends a response.
/// </summary>
public class ApduResponseSentEventArgs : EventArgs
{
    public byte[] ResponseApdu { get; }
    public string Description { get; }

    public ApduResponseSentEventArgs(byte[] responseApdu, string description)
    {
        ResponseApdu = responseApdu;
        Description = description;
    }
}

/// <summary>
/// Raised when a NFC tag is discovered by the reader.
/// </summary>
public class TagDiscoveredEventArgs : EventArgs
{
    public string TagId { get; }
    public string TagType { get; }

    public TagDiscoveredEventArgs(string tagId, string tagType)
    {
        TagId = tagId;
        TagType = tagType;
    }
}

/// <summary>
/// Raised when data is received from a tag/card via the reader.
/// </summary>
public class DataReceivedEventArgs : EventArgs
{
    public byte[] RawData { get; }
    public string? DecryptedText { get; }

    public DataReceivedEventArgs(byte[] rawData, string? decryptedText = null)
    {
        RawData = rawData;
        DecryptedText = decryptedText;
    }
}

namespace NfcSecurePoc.Apdu;

/// <summary>
/// Parses an ISO 7816-4 APDU response: [Data..., SW1, SW2].
/// </summary>
public class ApduResponse
{
    public byte[] Data { get; }
    public byte Sw1 { get; }
    public byte Sw2 { get; }
    public ushort StatusWord => (ushort)((Sw1 << 8) | Sw2);
    public bool IsSuccess => StatusWord == ApduConstants.SwSuccess;

    public ApduResponse(byte[] rawResponse)
    {
        if (rawResponse == null || rawResponse.Length < 2)
            throw new ArgumentException("APDU response must be at least 2 bytes (SW1+SW2).");

        Sw1 = rawResponse[^2];
        Sw2 = rawResponse[^1];
        Data = rawResponse.Length > 2
            ? rawResponse[..^2]
            : Array.Empty<byte>();
    }

    /// <summary>
    /// Builds a raw response byte array from data + status word.
    /// </summary>
    public static byte[] Build(byte[] data, ushort statusWord)
    {
        var sw = ApduConstants.StatusWordToBytes(statusWord);
        var response = new byte[data.Length + 2];
        Array.Copy(data, 0, response, 0, data.Length);
        response[^2] = sw[0];
        response[^1] = sw[1];
        return response;
    }

    /// <summary>
    /// Builds a raw response with status word only (no data).
    /// </summary>
    public static byte[] Build(ushort statusWord) =>
        ApduConstants.StatusWordToBytes(statusWord);
}

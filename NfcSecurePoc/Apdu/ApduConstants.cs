namespace NfcSecurePoc.Apdu;

public static class ApduConstants
{
    /// <summary>
    /// Application Identifier: "POC.NFC" encoded as hex (F0504F432E4E4643).
    /// </summary>
    public static readonly byte[] Aid = { 0xF0, 0x50, 0x4F, 0x43, 0x2E, 0x4E, 0x46, 0x43 };

    /// <summary>
    /// Version string returned on successful SELECT.
    /// </summary>
    public const string VersionString = "NFC-POC-v1";

    // CLA
    public const byte ClaIso = 0x00;

    // INS codes
    public const byte InsSelect = 0xA4;
    public const byte InsGetData = 0xCA;
    public const byte InsPutData = 0xDA;

    // SELECT parameters
    public const byte P1SelectByName = 0x04;
    public const byte P2SelectFirst = 0x00;

    // Status Words
    public const ushort SwSuccess = 0x9000;
    public const ushort SwFileNotFound = 0x6A82;
    public const ushort SwWrongLength = 0x6700;
    public const ushort SwConditionsNotSatisfied = 0x6985;
    public const ushort SwInsNotSupported = 0x6D00;

    public static byte[] StatusWordToBytes(ushort sw) =>
        new[] { (byte)(sw >> 8), (byte)(sw & 0xFF) };
}

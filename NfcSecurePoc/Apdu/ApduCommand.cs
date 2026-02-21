namespace NfcSecurePoc.Apdu;

/// <summary>
/// Builds ISO 7816-4 APDU command byte arrays.
/// Format: [CLA, INS, P1, P2, Lc, Data..., Le?]
/// </summary>
public static class ApduCommand
{
    /// <summary>
    /// SELECT command to select an application by AID.
    /// </summary>
    public static byte[] Select(byte[] aid)
    {
        var cmd = new byte[5 + aid.Length];
        cmd[0] = ApduConstants.ClaIso;
        cmd[1] = ApduConstants.InsSelect;
        cmd[2] = ApduConstants.P1SelectByName;
        cmd[3] = ApduConstants.P2SelectFirst;
        cmd[4] = (byte)aid.Length;
        Array.Copy(aid, 0, cmd, 5, aid.Length);
        return cmd;
    }

    /// <summary>
    /// GET DATA command to read encrypted payload from the card.
    /// </summary>
    public static byte[] GetData()
    {
        return new byte[]
        {
            ApduConstants.ClaIso,
            ApduConstants.InsGetData,
            0x00, 0x00,
            0x00  // Le = 0 → max length expected
        };
    }

    /// <summary>
    /// PUT DATA command to send encrypted payload to the card.
    /// </summary>
    public static byte[] PutData(byte[] encryptedPayload)
    {
        var cmd = new byte[5 + encryptedPayload.Length];
        cmd[0] = ApduConstants.ClaIso;
        cmd[1] = ApduConstants.InsPutData;
        cmd[2] = 0x00;
        cmd[3] = 0x00;
        cmd[4] = (byte)encryptedPayload.Length;
        Array.Copy(encryptedPayload, 0, cmd, 5, encryptedPayload.Length);
        return cmd;
    }
}

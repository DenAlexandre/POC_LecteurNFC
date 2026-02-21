using System.Security.Cryptography;

namespace NfcSecurePoc.Crypto;

public class AesCryptoService : IAesCryptoService
{
    private const int IvLength = 16;

    /// <summary>
    /// Hardcoded AES-256 key for POC purposes only.
    /// In production, use secure key exchange / key storage.
    /// </summary>
    private static readonly byte[] SharedKey =
    {
        0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
        0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10,
        0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18,
        0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F, 0x20
    };

    public byte[] Encrypt(byte[] plaintext)
    {
        using var aes = Aes.Create();
        aes.Key = SharedKey;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var ciphertext = encryptor.TransformFinalBlock(plaintext, 0, plaintext.Length);

        // Prepend IV to ciphertext
        var result = new byte[IvLength + ciphertext.Length];
        Array.Copy(aes.IV, 0, result, 0, IvLength);
        Array.Copy(ciphertext, 0, result, IvLength, ciphertext.Length);
        return result;
    }

    public byte[] Decrypt(byte[] ivAndCiphertext)
    {
        if (ivAndCiphertext.Length < IvLength + 16)
            throw new CryptographicException("Data too short: must contain IV + at least one AES block.");

        var iv = ivAndCiphertext[..IvLength];
        var ciphertext = ivAndCiphertext[IvLength..];

        using var aes = Aes.Create();
        aes.Key = SharedKey;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(ciphertext, 0, ciphertext.Length);
    }
}

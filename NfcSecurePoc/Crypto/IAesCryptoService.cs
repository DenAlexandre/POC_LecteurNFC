namespace NfcSecurePoc.Crypto;

public interface IAesCryptoService
{
    /// <summary>
    /// Encrypts plaintext with AES-256-CBC. Returns IV (16 bytes) prepended to ciphertext.
    /// </summary>
    byte[] Encrypt(byte[] plaintext);

    /// <summary>
    /// Decrypts data where the first 16 bytes are the IV, followed by AES-256-CBC ciphertext.
    /// </summary>
    byte[] Decrypt(byte[] ivAndCiphertext);
}

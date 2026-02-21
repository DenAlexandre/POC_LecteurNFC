using System.Security.Cryptography;
using System.Text;
using NfcSecurePoc.Crypto;

namespace NfcSecurePoc.Tests.Crypto;

public class AesCryptoServiceTests
{
    private readonly AesCryptoService _crypto = new();

    [Fact]
    public void Encrypt_ReturnsIvPrependedToCiphertext()
    {
        var plaintext = Encoding.UTF8.GetBytes("Hello NFC");
        var result = _crypto.Encrypt(plaintext);

        // IV (16 bytes) + at least 1 AES block (16 bytes)
        Assert.True(result.Length >= 32);
        // Length = 16 (IV) + ceil(plaintext / 16) * 16
        Assert.Equal(0, (result.Length - 16) % 16);
    }

    [Fact]
    public void Decrypt_RecoverOriginalPlaintext()
    {
        var original = "Hello NFC Secure POC!";
        var plaintext = Encoding.UTF8.GetBytes(original);

        var encrypted = _crypto.Encrypt(plaintext);
        var decrypted = _crypto.Decrypt(encrypted);

        Assert.Equal(original, Encoding.UTF8.GetString(decrypted));
    }

    [Fact]
    public void EncryptDecrypt_EmptyString_Roundtrips()
    {
        var plaintext = Array.Empty<byte>();

        var encrypted = _crypto.Encrypt(plaintext);
        var decrypted = _crypto.Decrypt(encrypted);

        Assert.Empty(decrypted);
    }

    [Fact]
    public void EncryptDecrypt_ExactBlockSize_Roundtrips()
    {
        // 16 bytes = exact AES block size, PKCS7 adds a full padding block
        var plaintext = new byte[16];
        new Random(42).NextBytes(plaintext);

        var encrypted = _crypto.Encrypt(plaintext);
        var decrypted = _crypto.Decrypt(encrypted);

        Assert.Equal(plaintext, decrypted);
    }

    [Fact]
    public void EncryptDecrypt_LargePayload_Roundtrips()
    {
        var plaintext = new byte[1024];
        new Random(42).NextBytes(plaintext);

        var encrypted = _crypto.Encrypt(plaintext);
        var decrypted = _crypto.Decrypt(encrypted);

        Assert.Equal(plaintext, decrypted);
    }

    [Fact]
    public void Encrypt_ProducesDifferentCiphertextEachTime()
    {
        var plaintext = Encoding.UTF8.GetBytes("Same input");

        var encrypted1 = _crypto.Encrypt(plaintext);
        var encrypted2 = _crypto.Encrypt(plaintext);

        // Different random IV → different ciphertext
        Assert.NotEqual(encrypted1, encrypted2);
    }

    [Fact]
    public void Encrypt_IvIsDifferentEachTime()
    {
        var plaintext = Encoding.UTF8.GetBytes("Test");

        var encrypted1 = _crypto.Encrypt(plaintext);
        var encrypted2 = _crypto.Encrypt(plaintext);

        var iv1 = encrypted1[..16];
        var iv2 = encrypted2[..16];

        Assert.NotEqual(iv1, iv2);
    }

    [Fact]
    public void Decrypt_ThrowsOnTooShortData()
    {
        // Less than IV (16) + one block (16) = 32 bytes minimum
        var tooShort = new byte[20];

        Assert.Throws<CryptographicException>(() => _crypto.Decrypt(tooShort));
    }

    [Fact]
    public void Decrypt_ThrowsOnCorruptedCiphertext()
    {
        var plaintext = Encoding.UTF8.GetBytes("Hello");
        var encrypted = _crypto.Encrypt(plaintext);

        // Corrupt the ciphertext (not the IV)
        encrypted[20] ^= 0xFF;
        encrypted[21] ^= 0xFF;

        Assert.ThrowsAny<CryptographicException>(() => _crypto.Decrypt(encrypted));
    }

    [Fact]
    public void Decrypt_WithTamperedIv_ProducesWrongPlaintext()
    {
        var original = "Sensitive data";
        var plaintext = Encoding.UTF8.GetBytes(original);

        var encrypted = _crypto.Encrypt(plaintext);

        // Tamper with IV
        encrypted[0] ^= 0xFF;

        // CBC with tampered IV decrypts but produces wrong first block
        var decrypted = _crypto.Decrypt(encrypted);
        Assert.NotEqual(original, Encoding.UTF8.GetString(decrypted));
    }

    [Fact]
    public void EncryptDecrypt_Utf8SpecialChars_Roundtrips()
    {
        var original = "Données chiffrées éàü 日本語 🔐";
        var plaintext = Encoding.UTF8.GetBytes(original);

        var encrypted = _crypto.Encrypt(plaintext);
        var decrypted = _crypto.Decrypt(encrypted);

        Assert.Equal(original, Encoding.UTF8.GetString(decrypted));
    }
}

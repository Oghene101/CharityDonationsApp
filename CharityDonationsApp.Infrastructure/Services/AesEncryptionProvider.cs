using System.Security.Cryptography;
using CharityDonationsApp.Application.Common.Contracts.Abstractions;
using CharityDonationsApp.Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace CharityDonationsApp.Infrastructure.Services;

public class AesEncryptionProvider : IEncryptionProvider
{
    private readonly byte[] _encryptionKey;

    public AesEncryptionProvider(IOptions<EncryptionSettings> encryption)
    {
        var keyBase64 = Convert.FromBase64String(encryption.Value.Key);

        _encryptionKey = keyBase64.Take(32).ToArray();

        if (_encryptionKey.Length != 32) // 256 bits
            throw new ArgumentException("Key must be 32 bytes for AES-256");
    }

    public string Encrypt(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
            return plaintext;

        using var aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.GenerateIV(); // Generate unique IV for each operation

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var memoryStream = new MemoryStream();

        // Write IV first, then encrypted data
        memoryStream.Write(aes.IV, 0, aes.IV.Length);

        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
        using (var streamWriter = new StreamWriter(cryptoStream))
        {
            streamWriter.Write(plaintext);
        }

        var encryptedData = memoryStream.ToArray();
        return Convert.ToBase64String(encryptedData);
    }

    public string Decrypt(string ciphertext)
    {
        if (string.IsNullOrEmpty(ciphertext))
            return ciphertext;

        try
        {
            var fullCipher = Convert.FromBase64String(ciphertext);

            using var aes = Aes.Create();
            aes.Key = _encryptionKey;

            // Extract IV from the beginning of the ciphertext
            var iv = new byte[16];
            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            aes.IV = iv;

            // Extract actual encrypted data (after IV)
            var cipherData = new byte[fullCipher.Length - iv.Length];
            Buffer.BlockCopy(fullCipher, iv.Length, cipherData, 0, cipherData.Length);

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var memoryStream = new MemoryStream(cipherData);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cryptoStream);

            return streamReader.ReadToEnd();
        }
        catch (CryptographicException ex)
        {
            throw new InvalidOperationException("Decryption failed", ex);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException("Invalid base64 string", ex);
        }
    }
}
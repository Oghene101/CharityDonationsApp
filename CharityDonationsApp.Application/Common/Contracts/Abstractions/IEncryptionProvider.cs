namespace CharityDonationsApp.Application.Common.Contracts.Abstractions;

public interface IEncryptionProvider
{
    string Encrypt(string plaintext);
    string Decrypt(string ciphertext);
}
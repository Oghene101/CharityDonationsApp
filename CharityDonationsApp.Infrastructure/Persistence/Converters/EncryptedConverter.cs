using CharityDonationsApp.Application.Common.Contracts.Abstractions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CharityDonationsApp.Infrastructure.Persistence.Converters;

public class EncryptedConverter(
    IEncryptionProvider encryptionProvider) : ValueConverter<string, string>(
    v => encryptionProvider.Encrypt(v),
    v => encryptionProvider.Decrypt(v))
{
}
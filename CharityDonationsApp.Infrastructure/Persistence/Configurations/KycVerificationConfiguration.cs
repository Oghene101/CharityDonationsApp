using CharityDonationsApp.Application.Common.Contracts.Abstractions;
using CharityDonationsApp.Domain.Entities;
using CharityDonationsApp.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CharityDonationsApp.Infrastructure.Persistence.Configurations;

public class KycVerificationConfiguration(
    IEncryptionProvider encryptionProvider) : IEntityTypeConfiguration<KycVerification>
{
    public void Configure(EntityTypeBuilder<KycVerification> builder)
    {
        builder.HasIndex(k => k.UserId);
        builder.HasIndex(k => k.BvnHash).IsUnique();
        builder.HasIndex(k => k.NinHash).IsUnique();
        builder.Property(k => k.BvnCipher).HasConversion(new EncryptedConverter(encryptionProvider));
        builder.Property(k => k.NinCipher).HasConversion(new EncryptedConverter(encryptionProvider));
    }
}
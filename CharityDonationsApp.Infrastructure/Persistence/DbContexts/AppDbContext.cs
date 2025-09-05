using System.Reflection;
using CharityDonationsApp.Application.Common.Contracts.Abstractions;
using CharityDonationsApp.Domain.Attributes;
using CharityDonationsApp.Domain.Entities;
using CharityDonationsApp.Infrastructure.Persistence.Comparators;
using CharityDonationsApp.Infrastructure.Persistence.Configurations;
using CharityDonationsApp.Infrastructure.Persistence.Converters;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace CharityDonationsApp.Infrastructure.Persistence.DbContexts;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Address> Addresses { get; set; }
    public DbSet<AuditTrail> AuditTrails { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<KycVerification> KycVerifications { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Wallet> Wallets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        var encryptionProvider = this.GetService<IEncryptionProvider>();
        modelBuilder.ApplyConfiguration(new KycVerificationConfiguration(encryptionProvider));

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType != typeof(string)) continue;

                var memberInfo = property.PropertyInfo ?? (MemberInfo)property.FieldInfo;
                if (memberInfo == null || !Attribute.IsDefined(memberInfo, typeof(EncryptedAttribute))) continue;

                property.SetValueConverter(new EncryptedConverter(encryptionProvider));
                property.SetValueComparer(new EncryptedConverterComparer());
            }
        }
    }
}
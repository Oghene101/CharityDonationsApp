using System.ComponentModel.DataAnnotations;
using CharityDonationsApp.Domain.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace CharityDonationsApp.Domain.Entities;

public class User : IdentityUser<Guid>, IAuditable
{
    public override Guid Id { get; set; } = Guid.CreateVersion7();
    [Required, MaxLength(50)] public string FirstName { get; set; } = string.Empty;
    [Required, MaxLength(50)] public string LastName { get; set; } = string.Empty;
    public int LockoutCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    [Required, MaxLength(150)] public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    [Required, MaxLength(150)] public string UpdatedBy { get; set; } = string.Empty;

    //Navigation props
    public KycVerification KycVerification { get; set; } = null!;
    public ICollection<Event> Events { get; set; } = [];
    public ICollection<Wallet> Wallets { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
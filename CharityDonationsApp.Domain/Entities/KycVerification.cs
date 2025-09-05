using System.ComponentModel.DataAnnotations;

namespace CharityDonationsApp.Domain.Entities;

public class KycVerification : BaseEntity
{
    [MaxLength(50)] public string? BvnCipher { get; set; }
    [MaxLength(50)] public string? BvnHash { get; set; }
    public bool? IsBvnSuccessfullyVerified { get; set; }
    public DateTimeOffset? BvnVerifiedAt { get; set; }
    [MaxLength(100)] public string? BvnVerificationReference { get; set; }
    [MaxLength(50)] public string? NinCipher { get; set; }
    [MaxLength(50)] public string? NinHash { get; set; }
    public bool? IsNinSuccessfullyVerified { get; set; }
    public DateTimeOffset? NinVerifiedAt { get; set; }
    [MaxLength(100)] public string? NinVerificationReference { get; set; }
    [Required] public Guid UserId { get; set; }

    // Navigation props
    public User User { get; set; } = null!;
    public ICollection<Address> Addresses { get; set; } = [];
}
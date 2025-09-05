using System.ComponentModel.DataAnnotations;

namespace CharityDonationsApp.Domain.Entities;

public class Address : BaseEntity
{
    [Required, MaxLength(10)] public string HouseNumber { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string Landmark { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string Street { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string Lga { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string City { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string State { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string Country { get; set; } = string.Empty;
    [Required] public bool IsSuccessfullyVerified { get; set; }
    [Required] public Guid KycVerificationId { get; set; }

    // Navigation props
    public KycVerification KycVerification { get; set; } = null!;
}
using System.ComponentModel.DataAnnotations;

namespace CharityDonationsApp.Domain.Entities;

public class RefreshToken : BaseEntity
{
    [Required, MaxLength(50)] public string Token { get; set; } = string.Empty;
    [Required] public DateTimeOffset ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public bool IsUsed { get; set; }
    [Required] public Guid UserId { get; set; }

    //Navigation props
    public User User { get; set; } = null!;
}
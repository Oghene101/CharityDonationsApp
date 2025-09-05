using System.ComponentModel.DataAnnotations;

namespace CharityDonationsApp.Domain.Entities;

public class Event : BaseEntity
{
    [Required, MaxLength(150)] public string Name { get; set; } = string.Empty;
    [Required, MaxLength(500)] public string Description { get; set; } = string.Empty;
    [Required] public Guid UserId { get; set; }

    //Navigation props
    public User User { get; set; } = null!;
    public Wallet Wallet { get; set; } = null!;
}
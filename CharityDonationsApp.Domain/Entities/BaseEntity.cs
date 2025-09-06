using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CharityDonationsApp.Domain.Abstractions;

namespace CharityDonationsApp.Domain.Entities;

public abstract class BaseEntity : IAuditable
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [Required] public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    [Required, MaxLength(150)] public string CreatedBy { get; set; } = string.Empty;
    [Required] public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    [Required, MaxLength(150)] public string UpdatedBy { get; set; } = string.Empty;
}
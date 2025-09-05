using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace CharityDonationsApp.Domain.Entities;

public class Wallet : BaseEntity
{
    [Required, MaxLength(100)] public string WalletType { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string VendorWalletId { get; set; } = string.Empty;
    [Required, MaxLength(50)] public string BankName { get; set; } = string.Empty;
    [Required, MaxLength(10)] public string AccountNumber { get; set; } = string.Empty;
    [Required, MaxLength(150)] public string AccountName { get; set; } = string.Empty;
    [Required, Precision(18, 2)]public decimal AccountBalance { get; set; }
    [Required] public Guid UserId { get; set; }
    [Required] public Guid EventId { get; set; }

    //Navigation props
    public User User { get; set; } = null!;
    public Event Event { get; set; } = null!;
    public ICollection<Transaction> Transactions { get; set; } = [];
}
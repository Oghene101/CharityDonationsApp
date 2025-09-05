using System.ComponentModel.DataAnnotations;
using CharityDonationsApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CharityDonationsApp.Domain.Entities;

public class Transaction : BaseEntity
{
    [Required] public TransactionType Type { get; set; }
    [Required, MaxLength(300)] public string Narration { get; set; } = string.Empty;
    [Required, Precision(18, 2)] public decimal Amount { get; set; }
    [Required] public Guid WalletId { get; set; }

    //Navigation props
    public Wallet Wallet { get; set; } = null!;
}
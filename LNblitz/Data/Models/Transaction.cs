using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace LNblitz.Data.Models
{
    public class Transaction
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string TransactionId { get; set; }
        public string WalletId { get; set; }
        public int Amount { get; set; }
        public string Description { get; set; }
        public string PaymentRequest { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset? PaidAt { get; set; }

        public Wallet Wallet { get; set; }
    }
}

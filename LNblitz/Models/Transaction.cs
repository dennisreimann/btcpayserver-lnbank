using System;

namespace LNblitz.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public int WalletId { get; set; }
        public int Amount { get; set; }
        public string Description { get; set; }
        public string PaymentRequest { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset? PaidAt { get; set; }

        public Wallet Wallet { get; set; }
    }
}

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace LNblitz.Data.Models
{
    public class Transaction
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string TransactionId { get; set; }
        public string InvoiceId { get; set; }
        public string WalletId { get; set; }
        public long Amount { get; set; }
        [DisplayName("Received")]
        public long AmountReceived { get; set; }
        public string Description { get; set; }
        [DisplayName("Payment Request")]
        public string PaymentRequest { get; set; }
        [DisplayName("Expiry")]
        public DateTimeOffset ExpiresAt { get; set; }
        [DisplayName("Payment date")]
        public DateTimeOffset? PaidAt { get; set; }

        public Wallet Wallet { get; set; }

        public string Status
        {
            get {
                if (AmountReceived > Amount) return "overpaid";
                if (AmountReceived == Amount) return "paid";
                if (AmountReceived > 0 && AmountReceived < Amount) return "partially paid";
                if (ExpiresAt <= DateTimeOffset.UtcNow) return "expired";
                return "unpaid";
            }
        }
    }
}

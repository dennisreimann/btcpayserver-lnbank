using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BTCPayServer.Lightning;

namespace LNblitz.Data.Models
{
    public class Transaction
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string TransactionId { get; set; }
        public string InvoiceId { get; set; }
        public string WalletId { get; set; }

        [Column(TypeName = "long")]
        [Required]
        public LightMoney Amount { get; set; }
        [DisplayName("Settled amount")]
        [Column(TypeName = "long")]
        public LightMoney AmountSettled { get; set; }
        public string Description { get; set; }
        [DisplayName("Payment Request")]
        [Required]
        public string PaymentRequest { get; set; }
        [DisplayName("Creation date")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        [DisplayName("Expiry")]
        public DateTimeOffset ExpiresAt { get; set; }
        [DisplayName("Payment date")]
        public DateTimeOffset? PaidAt { get; set; }

        public Wallet Wallet { get; set; }

        private const string StatusPaid = "paid";
        private const string StatusUnpaid = "unpaid";
        private const string StatusExpired = "expired";

        public string Status
        {
            get
            {
                if (AmountSettled != null && AmountSettled > 0)
                {
                    if (AmountSettled > Amount) return "overpaid";
                    if (AmountSettled < Amount) return "partially paid";
                    return StatusPaid;
                }

                if (ExpiresAt <= DateTimeOffset.UtcNow)
                {
                    return StatusExpired;
                }

                return StatusUnpaid;
            }
        }

        public bool IsPaid => Status == StatusPaid;
        public bool IsUnpaid => Status == StatusUnpaid;
        public bool IsExpired => Status == StatusExpired;

        public DateTimeOffset Date => PaidAt ?? CreatedAt;
    }
}

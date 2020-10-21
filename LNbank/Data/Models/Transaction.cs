using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BTCPayServer.Lightning;

namespace LNbank.Data.Models
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
        private const string StatusOverpaid = "overpaid";
        private const string StatusPaidPartially = "partially paid";

        public string Status
        {
            get
            {
                if (AmountSettled != null && AmountSettled > 0)
                {
                    if (AmountSettled > Amount) return StatusOverpaid;
                    if (AmountSettled < Amount) return StatusPaidPartially;
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
        public bool IsOverpaid => Status == StatusOverpaid;
        public bool IsPaidPartially => Status == StatusPaidPartially;

        public DateTimeOffset Date => PaidAt ?? CreatedAt;
    }
}

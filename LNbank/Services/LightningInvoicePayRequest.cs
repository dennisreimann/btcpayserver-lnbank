using System.ComponentModel.DataAnnotations;

namespace LNbank.Services
{
    public class LightningInvoicePayRequest
    {
        [Required]
        public string WalletId { get; set; }
        [Required]
        public string PaymentRequest { get; set; }
    }
}

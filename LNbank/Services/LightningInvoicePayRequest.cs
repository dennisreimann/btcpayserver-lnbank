namespace LNbank.Services
{
    public class LightningInvoicePayRequest
    {
        public string WalletId { get; set; }
        public string PaymentRequest { get; set; }
    }
}

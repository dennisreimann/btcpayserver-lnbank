using System;
using BTCPayServer.Lightning;

namespace LNblitz.Services
{
    public class LightningInvoiceResponse
    {
        public string InvoiceId { get; set; }
        public string PaymentRequest { get; set; }
        public LightMoney Amount { get; set; }
        public LightMoney AmountReceived { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset? PaidAt { get; set; }
    }
}

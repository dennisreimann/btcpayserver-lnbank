using System;
using BTCPayServer.Lightning;

namespace LNbank.Services
{
    public class LightningInvoiceCreateRequest
    {
        public static readonly TimeSpan ExpiryDefault = TimeSpan.FromDays(1);
        public string WalletId { get; set; }
        public string Description { get; set; }
        public LightMoney Amount { get; set; }
        public TimeSpan Expiry { get; set; } = ExpiryDefault;
    }
}

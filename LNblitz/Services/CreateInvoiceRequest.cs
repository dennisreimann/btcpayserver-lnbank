using System;
using BTCPayServer.Lightning;

namespace LNblitz.Services
{
    public class CreateInvoiceRequest
    {
        public string Description { get; set; }
        public LightMoney Amount { get; set; }
        public TimeSpan Expiry { get; } = new TimeSpan(24, 0, 0);
    }
}

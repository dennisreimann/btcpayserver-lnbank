using System;
using System.ComponentModel.DataAnnotations;
using BTCPayServer.Client.JsonConverters;
using BTCPayServer.Lightning;
using Newtonsoft.Json;

namespace LNbank.Services
{
    public class LightningInvoiceCreateRequest
    {
        public static readonly TimeSpan ExpiryDefault = TimeSpan.FromDays(1);
        [Required]
        public string WalletId { get; set; }
        public string Description { get; set; }
        [Required]
        [JsonConverter(typeof(LightMoneyJsonConverter))]
        public LightMoney Amount { get; set; }
        public TimeSpan Expiry { get; set; } = ExpiryDefault;
    }
}

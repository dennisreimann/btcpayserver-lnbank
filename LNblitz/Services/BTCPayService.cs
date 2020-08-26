using System;
using System.Threading;
using System.Threading.Tasks;
using BTCPayServer.Client;
using BTCPayServer.Client.Models;
using Microsoft.Extensions.Configuration;

namespace LNblitz.Services
{
    public class BTCPayService
    {
        private const string CryptoCode = "BTC";
        private readonly BTCPayServerClient _client;
        private readonly string _storeId;

        public BTCPayService(IConfiguration configuration)
        {
            var endpoint = configuration["BTCPay:Endpoint"];
            var apiKey = configuration["BTCPay:ApiKey"];
            var storeId = configuration["BTCPay:StoreId"];

            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));
            if (apiKey == null) throw new ArgumentNullException(nameof(apiKey));
            if (storeId == null) throw new ArgumentNullException(nameof(storeId));

            Uri baseUri = new Uri(endpoint);
            _client = new BTCPayServerClient(baseUri, apiKey);
            _storeId = storeId;
        }

        // TODO: Use custom response data classes

        public async Task<LightningInvoiceData> CreateInvoice(CreateInvoiceRequest req)
        {
            return await _client.CreateLightningInvoice(_storeId, CryptoCode, new CreateLightningInvoiceRequest
            {
                Amount = req.Amount,
                Description = req.Description,
                Expiry = req.Expiry
            });
        }

        public async Task<LightningInvoiceData> GetInvoice(string invoiceId, CancellationToken cancellationToken)
        {
            return await _client.GetLightningInvoice(_storeId, CryptoCode, invoiceId, cancellationToken);
        }
    }
}

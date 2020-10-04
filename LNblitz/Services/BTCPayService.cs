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
        private static readonly string CryptoCode = "BTC";

        private readonly BTCPayServerClient _client;
        private readonly Uri _baseUri;
        private readonly string _storeId;

        public BTCPayService(IConfiguration configuration)
        {
            var apiKey = configuration["BTCPay:ApiKey"];
            var storeId = configuration["BTCPay:StoreId"];
            var endpoint = configuration["BTCPay:Endpoint"];

            if (apiKey == null) throw new ArgumentNullException(nameof(apiKey));
            if (storeId == null) throw new ArgumentNullException(nameof(storeId));
            if (endpoint == null) throw new ArgumentNullException(nameof(endpoint));

            _storeId = storeId;
            _baseUri = new Uri(endpoint);
            _client = new BTCPayServerClient(_baseUri, apiKey);
        }

        public async Task<ApplicationUserData> GetUserDataForApiKey(string userApiKey)
        {
            var client = new BTCPayServerClient(_baseUri, userApiKey);
            return await client.GetCurrentUser();
        }

        public async Task<LightningInvoiceData> CreateInvoice(CreateInvoiceRequest req)
        {
            return await _client.CreateLightningInvoice(_storeId, CryptoCode, new CreateLightningInvoiceRequest
            {
                Amount = req.Amount,
                Description = req.Description,
                Expiry = req.Expiry
            });
        }
        public async Task PayInvoice(PayInvoiceRequest req)
        {
            await _client.PayLightningInvoice(_storeId, CryptoCode, new PayLightningInvoiceRequest
            {
                BOLT11 = req.PaymentRequest
            });
        }

        public async Task<LightningInvoiceData> GetInvoice(string invoiceId, CancellationToken cancellationToken)
        {
            return await _client.GetLightningInvoice(_storeId, CryptoCode, invoiceId, cancellationToken);
        }
    }
}

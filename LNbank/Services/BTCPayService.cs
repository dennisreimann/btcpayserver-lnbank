using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BTCPayServer.Client;
using BTCPayServer.Client.Models;
using LNbank.Services.Settings;
using Microsoft.Extensions.Logging;

namespace LNbank.Services
{
    public class BTCPayService
    {
        private static readonly string CryptoCode = "BTC";

        private readonly BTCPayServerClient _client;
        private readonly Uri _baseUri;
        private readonly string _storeId;

        public BTCPayService(
            SettingsService settingsService,
            ILogger<BTCPayService> logger) : this(
            settingsService.BtcPay.Endpoint,
            settingsService.BtcPay.StoreId,
            settingsService.BtcPay.ApiKey,
            logger) {}

        public BTCPayService(string endpoint, string storeId, string apiKey, ILogger<BTCPayService> logger)
        {
            if (apiKey == null || storeId == null || endpoint == null)
            {
                logger.LogWarning("BTCPay Server settings missing, cannot instantiate BTCPayService.");
            }
            else
            {
                _storeId = storeId;
                _baseUri = new Uri(endpoint);
                _client = new BTCPayServerClient(_baseUri, apiKey);
            }
        }

        public async Task<ApplicationUserData> GetUserDataForApiKey(string userApiKey)
        {
            var client = new BTCPayServerClient(_baseUri, userApiKey);
            return await client.GetCurrentUser();
        }

        public async Task<LightningInvoiceData> CreateLightningInvoice(LightningInvoiceCreateRequest req)
        {
            return await _client.CreateLightningInvoice(_storeId, CryptoCode, new CreateLightningInvoiceRequest
            {
                Amount = req.Amount,
                Description = req.Description,
                Expiry = req.Expiry
            });
        }
        public async Task PayLightningInvoice(LightningInvoicePayRequest req)
        {
            await _client.PayLightningInvoice(_storeId, CryptoCode, new PayLightningInvoiceRequest
            {
                BOLT11 = req.PaymentRequest
            });
        }

        public async Task<LightningInvoiceData> GetLightningInvoice(string invoiceId, CancellationToken cancellationToken = default)
        {
            return await _client.GetLightningInvoice(_storeId, CryptoCode, invoiceId, cancellationToken);
        }

        public async Task<LightningNodeInformationData> GetLightningNodeInfo(CancellationToken cancellationToken = default)
        {
            return await _client.GetLightningNodeInfo(_storeId, CryptoCode, cancellationToken);
        }

        public async Task<IEnumerable<LightningChannelData>> ListLightningChannels(CancellationToken cancellationToken = default)
        {
            return await _client.GetLightningNodeChannels(_storeId, CryptoCode, cancellationToken);
        }

        public async Task<string> GetLightningDepositAddress(CancellationToken cancellationToken = default)
        {
            return await _client.GetLightningDepositAddress(_storeId, CryptoCode, cancellationToken);
        }

        public async Task<string> OpenLightningChannel(OpenLightningChannelRequest req, CancellationToken cancellationToken = default)
        {
            return await _client.OpenLightningChannel(_storeId, CryptoCode, req, cancellationToken);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BTCPayServer.Lightning;
using LNblitz.Data.Models;
using LNblitz.Data.Queries;
using LNblitz.Data.Services;
using LNblitz.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NBitcoin;
using Transaction = LNblitz.Data.Models.Transaction;

namespace LNblitz.Services
{
    public class WalletService
    {
        private readonly ILogger _logger;
        private readonly BTCPayService _btcpayService;
        private readonly WalletManager _walletManager;
        private readonly IHubContext<InvoiceHub> _invoiceHub;

        public WalletService(
            ILogger<WalletService> logger,
            BTCPayService btcpayService,
            WalletManager walletManager,
            IHubContext<InvoiceHub> invoiceHub)
        {
            _logger = logger;
            _btcpayService = btcpayService;
            _walletManager = walletManager;
            _invoiceHub = invoiceHub;
        }

        public async Task<IEnumerable<Wallet>> GetWallets(string userId)
        {
            return await _walletManager.GetWallets(new WalletsQuery
            {
                UserId = userId,
                IncludeTransactions = true
            });
        }

        public async Task<Wallet> GetWallet(string userId, string walletId)
        {
            return await _walletManager.GetWallet(new WalletQuery
            {
                UserId = userId,
                WalletId = walletId,
                IncludeTransactions = true
            });
        }

        public async Task<Transaction> Receive(Wallet wallet, long sats, string description)
        {
            var data = await _btcpayService.CreateInvoice(new CreateInvoiceRequest
            {
                Amount = LightMoney.Satoshis(sats),
                Description = description
            });

            return await _walletManager.CreateReceiveTransaction(new Transaction
            {
                WalletId = wallet.WalletId,
                InvoiceId = data.Id,
                Amount = data.Amount,
                ExpiresAt = data.ExpiresAt,
                PaymentRequest = data.BOLT11,
                Description = description
            });
        }

        public async Task<Transaction> Send(Wallet wallet, BOLT11PaymentRequest bolt11, string paymentRequest)
        {
            var amount = bolt11.MinimumAmount;

            if (wallet.Balance < amount)
            {
                throw new Exception($"Insufficient balance: {wallet.Balance} msat, tried to send {amount} msat.");
            }

            await _btcpayService.PayInvoice(new PayInvoiceRequest { PaymentRequest = paymentRequest });

            return await _walletManager.CreateSendTransaction(new Transaction
            {
                WalletId = wallet.WalletId,
                PaymentRequest = paymentRequest,
                Amount = amount,
                AmountSettled = new LightMoney(amount.MilliSatoshi * -1),
                ExpiresAt = bolt11.ExpiryDate,
                Description = bolt11.ShortDescription,
                PaidAt = DateTimeOffset.UtcNow
            });
        }

        public async Task<Wallet> AddOrUpdateWallet(Wallet wallet)
        {
            return await _walletManager.AddOrUpdateWallet(wallet);
        }

        public async Task RemoveWallet(Wallet wallet)
        {
            await _walletManager.RemoveWallet(wallet);
        }

        public async Task<IEnumerable<Transaction>> GetPendingTransactions()
        {
            return await _walletManager.GetTransactions(new TransactionsQuery
            {
                IncludingPending = true,
                IncludingExpired = false,
                IncludingPaid = false
            });
        }

        public async Task CheckPendingTransaction(Transaction transaction, CancellationToken stoppingToken)
        {
            var invoice = await _btcpayService.GetInvoice(transaction.InvoiceId, stoppingToken);
            if (invoice.Status == LightningInvoiceStatus.Paid)
            {
                _logger.LogInformation($"Marking invoice {invoice.Id} as paid.");

                transaction.AmountSettled = invoice.AmountReceived;
                transaction.PaidAt = invoice.PaidAt;

                await _invoiceHub.Clients.All.SendAsync("Message", $"Transaction {transaction.TransactionId} ({invoice.Id}) paid");
                await _walletManager.UpdateTransaction(transaction);
            }
        }

        public async Task<Transaction> GetTransaction(string userId, string walletId, string transactionId)
        {
            return await _walletManager.GetTransaction(new TransactionQuery
            {
                UserId = userId,
                WalletId = walletId,
                TransactionId = transactionId
            });
        }

        public async Task<Transaction> UpdateTransaction(Transaction transaction)
        {
            return await _walletManager.UpdateTransaction(transaction);
        }

        public async Task RemoveTransaction(Transaction transaction)
        {
            await _walletManager.RemoveTransaction(transaction);
        }

        public BOLT11PaymentRequest ParsePaymentRequest(string payReq)
        {
            // TODO: Configure network
            return BOLT11PaymentRequest.Parse(payReq, Network.RegTest);
        }
    }
}

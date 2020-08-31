using System;
using System.Threading.Tasks;
using BTCPayServer.Lightning;
using LNblitz.Data.Models;
using LNblitz.Data.Queries;
using LNblitz.Data.Services;

namespace LNblitz.Services
{
    public class WalletService
    {
        private readonly BTCPayService _btcpayService;
        private readonly WalletManager _walletManager;

        public WalletService(
            BTCPayService btcpayService,
            WalletManager walletManager)
        {
            _btcpayService = btcpayService;
            _walletManager = walletManager;
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
    }
}

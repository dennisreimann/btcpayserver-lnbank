using System.Threading.Tasks;
using BTCPayServer.Client;
using BTCPayServer.Lightning;
using LNblitz.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LNblitz.Data.Queries;
using LNblitz.Data.Services;
using Microsoft.Extensions.Logging;
using NBitcoin;
using Transaction = LNblitz.Data.Models.Transaction;

namespace LNblitz.Pages.Wallets.Transactions
{
    public class SendModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly WalletManager _walletManager;
        private readonly ILogger _logger;
        public Wallet Wallet { get; set; }
        public Transaction Transaction { get; set; }
        public BOLT11PaymentRequest Bolt11 { get; set; }
        public string ErrorMessage { get; set; }

        public SendModel(
            ILogger<SendModel> logger,
            UserManager<User> userManager,
            WalletManager walletManager)
        {
            _logger = logger;
            _userManager = userManager;
            _walletManager = walletManager;
        }

        public async Task<IActionResult> OnGet(string walletId)
        {
            var userId = _userManager.GetUserId(User);
            Wallet = await _walletManager.GetWallet(new WalletQuery
            {
                UserId = userId,
                WalletId = walletId
            });

            if (Wallet == null) return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string walletId, string action = "Decode")
        {
            var userId = _userManager.GetUserId(User);
            Wallet = await _walletManager.GetWallet(new WalletQuery
            {
                UserId = userId,
                WalletId = walletId
            });

            if (Wallet == null) return NotFound();
            if (!ModelState.IsValid) return Page();

            // TODO: Configure network
            var payReq = Request.Form["Transaction.PaymentRequest"].ToString();
            Bolt11 = BOLT11PaymentRequest.Parse(payReq, Network.RegTest);

            Transaction = new Transaction
            {
                WalletId = walletId,
                PaymentRequest = payReq,
                Amount = Bolt11.MinimumAmount,
                ExpiresAt = Bolt11.ExpiryDate,
                Description = Bolt11.ShortDescription
            };

            if (action == "Confirm")
            {
                try
                {
                    await _walletManager.CreateSendTransaction(Transaction, Bolt11);
                    return RedirectToPage("./Index", new { walletId });
                }
                catch (GreenFieldAPIException exception)
                {
                    _logger.LogError($"CreateSendTransaction failed: {exception}");
                    ErrorMessage = exception.Message;
                }
            }

            return Page();
        }
    }
}

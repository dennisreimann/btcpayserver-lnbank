using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BTCPayServer.Lightning;
using LNblitz.Data.Models;
using LNblitz.Services.Wallets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace LNblitz.Pages.Wallets
{
    public class SendModel : PageModel
    {
        private readonly ILogger _logger;
        private readonly WalletService _walletService;
        public Wallet Wallet { get; set; }
        public BOLT11PaymentRequest Bolt11 { get; set; }
        [BindProperty]
        [DisplayName("Payment Request")]
        [Required]
        public string PaymentRequest { get; set; }
        public string ErrorMessage { get; set; }

        public SendModel(
            ILogger<SendModel> logger,
            WalletService walletService)
        {
            _logger = logger;
            _walletService = walletService;
        }

        public async Task<IActionResult> OnGet(string walletId)
        {
            var userId = User.Claims.First(c => c.Type == "UserId").Value;
            Wallet = await _walletService.GetWallet(new WalletQuery {
                UserId = userId,
                WalletId = walletId,
                IncludeTransactions = true
            });

            if (Wallet == null) return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostDecodeAsync(string walletId)
        {
            var userId = User.Claims.First(c => c.Type == "UserId").Value;
            Wallet = await _walletService.GetWallet(new WalletQuery {
                UserId = userId,
                WalletId = walletId,
                IncludeTransactions = true
            });

            if (Wallet == null) return NotFound();
            if (!ModelState.IsValid) return Page();

            Bolt11 = _walletService.ParsePaymentRequest(PaymentRequest);

            return Page();
        }

        public async Task<IActionResult> OnPostConfirmAsync(string walletId)
        {
            var userId = User.Claims.First(c => c.Type == "UserId").Value;
            Wallet = await _walletService.GetWallet(new WalletQuery {
                UserId = userId,
                WalletId = walletId,
                IncludeTransactions = true
            });

            if (Wallet == null) return NotFound();
            if (!ModelState.IsValid) return Page();

            Bolt11 = _walletService.ParsePaymentRequest(PaymentRequest);

            try
            {
                await _walletService.Send(Wallet, Bolt11, PaymentRequest);
                return RedirectToPage("./Index", new { walletId });
            }
            catch (Exception exception)
            {
                _logger.LogError($"Sending failed! {exception}");
                ErrorMessage = exception.Message;
            }

            return Page();
        }
    }
}

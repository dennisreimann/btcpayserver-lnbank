using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using BTCPayServer.Lightning;
using LNblitz.Data.Models;
using LNblitz.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace LNblitz.Pages.Wallets.Transactions
{
    public class SendModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly WalletService _walletService;
        private readonly ILogger _logger;
        public Wallet Wallet { get; set; }
        public BOLT11PaymentRequest Bolt11 { get; set; }
        [BindProperty]
        [DisplayName("Payment Request")]
        [Required]
        public string PaymentRequest { get; set; }
        public string ErrorMessage { get; set; }

        public SendModel(
            ILogger<SendModel> logger,
            UserManager<User> userManager,
            WalletService walletService)
        {
            _logger = logger;
            _userManager = userManager;
            _walletService = walletService;
        }

        public async Task<IActionResult> OnGet(string walletId)
        {
            var userId = _userManager.GetUserId(User);
            Wallet = await _walletService.GetWallet(userId, walletId);

            if (Wallet == null) return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostDecodeAsync(string walletId)
        {
            var userId = _userManager.GetUserId(User);
            Wallet = await _walletService.GetWallet(userId, walletId);

            if (Wallet == null) return NotFound();
            if (!ModelState.IsValid) return Page();

            Bolt11 = _walletService.ParsePaymentRequest(PaymentRequest);

            return Page();
        }

        public async Task<IActionResult> OnPostConfirmAsync(string walletId)
        {
            var userId = _userManager.GetUserId(User);
            Wallet = await _walletService.GetWallet(userId, walletId);

            if (Wallet == null) return NotFound();
            if (!ModelState.IsValid) return Page();

            Bolt11 = _walletService.ParsePaymentRequest(PaymentRequest);

            try
            {
                await _walletService.Send(Wallet, Bolt11, PaymentRequest);
                return RedirectToPage("/Index", new { walletId });
            }
            catch (Exception exception)
            {
                _logger.LogError($"CreateSendTransaction failed! {exception}");
                ErrorMessage = exception.Message;
            }

            return Page();
        }
    }
}

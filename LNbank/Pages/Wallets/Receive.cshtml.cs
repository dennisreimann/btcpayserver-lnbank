using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using LNbank.Data.Models;
using LNbank.Services.Settings;
using LNbank.Services.Wallets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LNbank.Pages.Wallets
{
    public class ReceiveModel : BasePageModel
    {
        private readonly ILogger _logger;
        private readonly WalletService _walletService;
        public Wallet Wallet { get; set; }
        [BindProperty]
        public string Description { get; set; }
        [BindProperty]
        [DisplayName("Amount in sats")]
        [Required]
        public long Amount { get; set; }
        public string ErrorMessage { get; set; }

        public ReceiveModel(
            ILogger<SendModel> logger,
            WalletService walletService,
            SettingsService settingsService) : base(settingsService)
        {
            _logger = logger;
            _walletService = walletService;
        }

        public async Task<IActionResult> OnGet(string walletId)
        {
            Wallet = await _walletService.GetWallet(new WalletQuery {
                UserId = UserId,
                WalletId = walletId,
                IncludeTransactions = true
            });

            if (Wallet == null) return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string walletId)
        {
            Wallet = await _walletService.GetWallet(new WalletQuery {
                UserId = UserId,
                WalletId = walletId,
                IncludeTransactions = true
            });

            if (Wallet == null) return NotFound();
            if (!ModelState.IsValid) return Page();

            try
            {
                var transaction = await _walletService.Receive(Wallet, Amount, Description);
                var transactionId = transaction.TransactionId;
                return RedirectToPage("/Transactions/Details", new { walletId, transactionId });
            }
            catch (Exception exception)
            {
                _logger.LogError($"Receiving failed! {exception}");
                ErrorMessage = exception.Message;
            }

            return Page();
        }
    }
}

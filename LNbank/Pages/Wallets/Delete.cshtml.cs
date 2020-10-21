using System.Linq;
using System.Threading.Tasks;
using LNbank.Data.Models;
using LNbank.Services.Settings;
using LNbank.Services.Wallets;
using Microsoft.AspNetCore.Mvc;

namespace LNbank.Pages.Wallets
{
    public class DeleteModel : BasePageModel
    {
        private readonly WalletService _walletService;
        public Wallet Wallet { get; set; }

        public DeleteModel(
            WalletService walletService,
            SettingsService settingsService) : base(settingsService)
        {
            _walletService = walletService;
        }

        public async Task<IActionResult> OnGetAsync(string walletId)
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

        public async Task<IActionResult> OnPostAsync(string walletId)
        {
            var userId = User.Claims.First(c => c.Type == "UserId").Value;
            Wallet = await _walletService.GetWallet(new WalletQuery {
                UserId = userId,
                WalletId = walletId
            });

            if (Wallet == null) return NotFound();

            await _walletService.RemoveWallet(Wallet);

            return RedirectToPage("./Index");
        }
    }
}

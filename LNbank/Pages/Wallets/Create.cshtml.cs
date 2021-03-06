using System;
using System.Linq;
using System.Threading.Tasks;
using LNbank.Data.Models;
using LNbank.Services.Settings;
using LNbank.Services.Wallets;
using Microsoft.AspNetCore.Mvc;

namespace LNbank.Pages.Wallets
{
    public class CreateModel : BasePageModel
    {
        private readonly WalletService _walletService;
        public Wallet Wallet { get; set; }

        public CreateModel(
            WalletService walletService,
            SettingsService settingsService) : base(settingsService)
        {
            _walletService = walletService;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            Wallet = new Wallet
            {
                UserId = UserId
            };

            if (await TryUpdateModelAsync<Wallet>(Wallet, "wallet", w => w.Name))
            {
                await _walletService.AddOrUpdateWallet(Wallet);
                return RedirectToPage("./Index", new { walletId = Wallet.WalletId });
            }

            return Page();
        }
    }
}

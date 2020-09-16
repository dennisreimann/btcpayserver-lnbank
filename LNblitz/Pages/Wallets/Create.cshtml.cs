using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LNblitz.Data.Models;
using LNblitz.Services;

namespace LNblitz.Pages.Wallets
{
    public class CreateModel : PageModel
    {
        private readonly WalletService _walletService;
        public Wallet Wallet { get; set; }

        public CreateModel(WalletService walletService)
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

            var userId = User.Claims.First(c => c.Type == "UserId").Value;
            Wallet = new Wallet
            {
                UserId = userId,
                AdminKey = Guid.NewGuid().ToString(),
                InvoiceKey = Guid.NewGuid().ToString(),
                ReadonlyKey = Guid.NewGuid().ToString()
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

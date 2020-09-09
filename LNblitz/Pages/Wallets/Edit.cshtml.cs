using System.Linq;
using System.Threading.Tasks;
using LNblitz.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LNblitz.Services;

namespace LNblitz.Pages.Wallets
{
    public class EditModel : PageModel
    {
        private readonly WalletService _walletService;
        public Wallet Wallet { get; set; }

        public EditModel(WalletService walletService)
        {
            _walletService = walletService;
        }

        public async Task<IActionResult> OnGetAsync(string walletId)
        {
            var userId = User.Claims.First(c => c.Type == "UserId").Value;
            Wallet = await _walletService.GetWallet(userId, walletId);

            if (Wallet == null) return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string walletId)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userId = User.Claims.First(c => c.Type == "UserId").Value;
            Wallet = await _walletService.GetWallet(userId, walletId);

            if (Wallet == null) return NotFound();

            if (await TryUpdateModelAsync<Wallet>(Wallet, "wallet", w => w.Name))
            {
                await _walletService.AddOrUpdateWallet(Wallet);
                return RedirectToPage("./Index", new { walletId });
            }

            return Page();
        }
    }
}

using System.Threading.Tasks;
using LNblitz.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using LNblitz.Data.Queries;
using LNblitz.Data.Services;

namespace LNblitz.Pages.Wallets
{
    public class EditModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly WalletManager _walletManager;
        public Wallet Wallet { get; set; }

        public EditModel(UserManager<User> userManager, WalletManager walletManager)
        {
            _userManager = userManager;
            _walletManager = walletManager;
        }

        public async Task<IActionResult> OnGetAsync(string walletId)
        {
            var userId = _userManager.GetUserId(User);
            Wallet = await _walletManager.GetWallet(new WalletQuery()
            {
                UserId = userId,
                WalletId = walletId
            });

            if (Wallet == null) return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string walletId)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userId = _userManager.GetUserId(User);
            Wallet = await _walletManager.GetWallet(new WalletQuery
            {
                UserId = userId,
                WalletId = walletId
            });

            if (Wallet == null) return NotFound();

            if (await TryUpdateModelAsync<Wallet>(
                Wallet,
                "wallet",
                w => w.Name))
            {
                await _walletManager.AddOrUpdateWallet(Wallet);
                return RedirectToPage("./Index");
            }

            return Page();
        }
    }
}

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using LNblitz.Data.Queries;
using LNblitz.Data.Services;
using LNblitz.Models;

namespace LNblitz.Pages.Wallets
{
    public class DeleteModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly WalletManager _walletManager;
        public Wallet Wallet { get; set; }

        public DeleteModel(UserManager<User> userManager, WalletManager walletManager)
        {
            _userManager = userManager;
            _walletManager = walletManager;
        }

        public async Task<IActionResult> OnGetAsync(string walletId)
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

        public async Task<IActionResult> OnPostAsync(string walletId)
        {
            var userId = _userManager.GetUserId(User);
            Wallet = await _walletManager.GetWallet(new WalletQuery
            {
                UserId = userId,
                WalletId = walletId
            });

            if (Wallet == null) return NotFound();

            await _walletManager.RemoveWallet(Wallet);

            return RedirectToPage("./Index");
        }
    }
}

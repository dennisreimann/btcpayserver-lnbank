using System.Threading.Tasks;
using LNblitz.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using LNblitz.Data.Queries;
using LNblitz.Data.Services;
using LNblitz.Services;

namespace LNblitz.Pages.Wallets
{
    public class DeleteModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly WalletService _walletService;
        public Wallet Wallet { get; set; }

        public DeleteModel(UserManager<User> userManager, WalletService walletService)
        {
            _userManager = userManager;
            _walletService = walletService;
        }

        public async Task<IActionResult> OnGetAsync(string walletId)
        {
            var userId = _userManager.GetUserId(User);
            Wallet = await _walletService.GetWallet(userId, walletId);

            if (Wallet == null) return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string walletId)
        {
            var userId = _userManager.GetUserId(User);
            Wallet = await _walletService.GetWallet(userId, walletId);

            if (Wallet == null) return NotFound();

            await _walletService.RemoveWallet(Wallet);

            return RedirectToPage("./Index");
        }
    }
}

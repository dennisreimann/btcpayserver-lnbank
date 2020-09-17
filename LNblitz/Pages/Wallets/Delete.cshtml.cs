using System.Linq;
using System.Threading.Tasks;
using LNblitz.Data.Models;
using LNblitz.Services.Wallets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LNblitz.Pages.Wallets
{
    public class DeleteModel : PageModel
    {
        private readonly WalletService _walletService;
        public Wallet Wallet { get; set; }

        public DeleteModel(WalletService walletService)
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

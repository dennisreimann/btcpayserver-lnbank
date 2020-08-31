using System.Collections.Generic;
using System.Threading.Tasks;
using LNblitz.Data.Models;
using LNblitz.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LNblitz.Pages.Wallets.Transactions
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly WalletService _walletService;
        public Wallet Wallet { get; set; }
        public IList<Transaction> Transactions { get; set; }

        public IndexModel(
            UserManager<User> userManager,
            WalletService walletService)
        {
            _userManager = userManager;
            _walletService = walletService;
        }

        public async Task<IActionResult> OnGetAsync(string walletId)
        {
            var userId = _userManager.GetUserId(User);
            Wallet = await _walletService.GetWallet(userId, walletId);

            if (Wallet == null) return NotFound();

            Transactions = Wallet.Transactions;

            return Page();
        }
    }
}

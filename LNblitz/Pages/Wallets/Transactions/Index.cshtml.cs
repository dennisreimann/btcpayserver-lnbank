using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LNblitz.Data.Queries;
using LNblitz.Data.Services;
using LNblitz.Models;

namespace LNblitz.Pages.Wallets.Transactions
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly WalletManager _walletManager;
        public Wallet Wallet { get; set; }
        public IList<Transaction> Transactions { get; set; }

        public IndexModel(UserManager<User> userManager, WalletManager walletManager)
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
                WalletId = walletId,
                IncludeTransactions = true
            });

            if (Wallet == null) return NotFound();

            Transactions = Wallet.Transactions;

            return Page();
        }
    }
}

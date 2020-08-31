using System.Collections.Generic;
using System.Threading.Tasks;
using LNblitz.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LNblitz.Data.Queries;
using LNblitz.Data.Services;

namespace LNblitz.Pages.Wallets
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly WalletManager _walletManager;
        public IEnumerable<Wallet> Wallets { get; set; }

        public IndexModel(UserManager<User> userManager, WalletManager walletManager)
        {
            _userManager = userManager;
            _walletManager = walletManager;
        }

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
            Wallets = await _walletManager.GetWallets(new WalletsQuery
            {
                UserId = userId,
                IncludeTransactions = true
            });
        }
    }
}

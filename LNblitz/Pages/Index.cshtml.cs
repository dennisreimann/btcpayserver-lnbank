using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LNblitz.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LNblitz.Services;

namespace LNblitz.Pages.Wallets
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly WalletService _walletService;
        public IEnumerable<Wallet> Wallets { get; set; }
        public Wallet SelectedWallet { get; set; }
        public IEnumerable<Transaction> Transactions { get; set; }

        public IndexModel(
            UserManager<User> userManager,
            WalletService walletService)
        {
            _userManager = userManager;
            _walletService = walletService;
        }

        public async Task OnGetAsync(string walletId)
        {
            var userId = _userManager.GetUserId(User);
            Wallets = await _walletService.GetWallets(userId);

            var list = Wallets.ToList();
            if (!list.Any())
            {
                RedirectToRoute("./Wallets/Create");
            }
            else if (walletId != null)
            {
                SelectedWallet = list.FirstOrDefault(w => w.WalletId == walletId);
                Transactions = SelectedWallet?.Transactions.OrderByDescending(t => t.CreatedAt);
            }
        }
    }
}

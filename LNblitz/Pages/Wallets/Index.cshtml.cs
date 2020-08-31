using System.Collections.Generic;
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

        public IndexModel(
            UserManager<User> userManager,
            WalletService walletService)
        {
            _userManager = userManager;
            _walletService = walletService;
        }

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
            Wallets = await _walletService.GetWallets(userId);
        }
    }
}

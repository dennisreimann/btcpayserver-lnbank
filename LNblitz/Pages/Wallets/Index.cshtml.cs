using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LNblitz.Data.Models;
using LNblitz.Services.Settings;
using LNblitz.Services.Wallets;

namespace LNblitz.Pages.Wallets
{
    public class IndexModel : BasePageModel
    {
        private readonly WalletService _walletService;
        public IEnumerable<Wallet> Wallets { get; set; }
        public Wallet SelectedWallet { get; set; }
        public IEnumerable<Transaction> Transactions { get; set; }

        public IndexModel(
            WalletService walletService,
            SettingsService settingsService) : base(settingsService)
        {
            _walletService = walletService;
        }

        public async Task OnGetAsync(string walletId)
        {
            var userId = User.Claims.First(c => c.Type == "UserId").Value;
            Wallets = await _walletService.GetWallets(new WalletsQuery {
                UserId = userId,
                IncludeTransactions = true
            });

            var list = Wallets.ToList();
            if (!list.Any())
            {
                RedirectToRoute("./Create");
            }
            else if (walletId != null)
            {
                SelectedWallet = list.FirstOrDefault(w => w.WalletId == walletId);
                Transactions = SelectedWallet?.Transactions.OrderByDescending(t => t.CreatedAt);
            }
        }
    }
}

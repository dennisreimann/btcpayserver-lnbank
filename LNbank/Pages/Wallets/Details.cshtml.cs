using System.Linq;
using System.Threading.Tasks;
using LNbank.Data.Models;
using LNbank.Services.Settings;
using LNbank.Services.Users;
using LNbank.Services.Wallets;
using Microsoft.AspNetCore.Mvc;

namespace LNbank.Pages.Wallets
{
    public class DetailsModel : BasePageModel
    {
        private readonly WalletService _walletService;
        private readonly SettingsService _settingsService;
        private readonly UserService _userService;

        public Wallet Wallet { get; set; }
        public string ConnectionString { get; set; }

        public DetailsModel(
            WalletService walletService,
            SettingsService settingsService,
            UserService userService) : base(settingsService)
        {
            _settingsService = settingsService;
            _walletService = walletService;
            _userService = userService;
        }

        public async Task<IActionResult> OnGetAsync(string walletId)
        {
            Wallet = await _walletService.GetWallet(new WalletQuery {
                UserId = UserId,
                WalletId = walletId,
                IncludeTransactions = true
            });

            if (Wallet == null) return NotFound();

            ConnectionString = await CreateConnectionString();

            return Page();
        }

        private async Task<string> CreateConnectionString()
        {
            var user = await _userService.FindUserById(UserId);
            var server = $"{_settingsService.BtcPay.Endpoint}lnbank";
            return $"type=lnbank;server={server};api-token={user.BTCPayApiKey};wallet-id={Wallet.WalletId}";
        }
    }
}

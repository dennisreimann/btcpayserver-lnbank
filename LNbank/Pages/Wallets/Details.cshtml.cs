using System;
using System.Threading.Tasks;
using LNbank.Data.Models;
using LNbank.Services.Settings;
using LNbank.Services.Users;
using LNbank.Services.Wallets;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace LNbank.Pages.Wallets
{
    public class DetailsModel : BasePageModel
    {
        private readonly WalletService _walletService;
        private readonly UserService _userService;

        public Wallet Wallet { get; set; }
        public string ConnectionString { get; set; }

        public DetailsModel(
            WalletService walletService,
            SettingsService settingsService,
            UserService userService) : base(settingsService)
        {
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
            var url = HttpContext.Request.GetDisplayUrl();
            var index = url.IndexOf("/Wallets/Details/", StringComparison.InvariantCultureIgnoreCase);
            var server =  url.Substring(0, index);
            var user = await _userService.FindUserById(UserId);
            return $"type=lnbank;server={server};api-token={user.BTCPayApiKey};wallet-id={Wallet.WalletId}";
        }
    }
}

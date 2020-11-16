using System.Threading.Tasks;
using LNbank.Data.Models;
using LNbank.Services.Settings;
using LNbank.Services.Wallets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LNbank.Pages.Transactions
{
    [AllowAnonymous]
    public class ShareModel : BasePageModel
    {
        private readonly WalletService _walletService;
        public Transaction Transaction { get; set; }

        public ShareModel(
            WalletService walletService,
            SettingsService settingsService) : base(settingsService)
        {
            _walletService = walletService;
        }

        public async Task<IActionResult> OnGetAsync(string transactionId)
        {
            Transaction = await _walletService.GetTransaction(new TransactionQuery
            {
                TransactionId = transactionId
            });

            if (Transaction == null || Transaction.IsExpired) return NotFound();

            return Page();
        }
    }
}

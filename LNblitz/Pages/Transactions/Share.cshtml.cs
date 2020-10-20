using System.Threading.Tasks;
using LNblitz.Data.Models;
using LNblitz.Services.Settings;
using LNblitz.Services.Wallets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LNblitz.Pages.Transactions
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

            if (Transaction == null) return NotFound();

            return Page();
        }
    }
}

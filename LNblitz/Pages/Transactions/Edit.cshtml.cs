using System.Linq;
using System.Threading.Tasks;
using LNblitz.Data.Models;
using LNblitz.Services.Settings;
using LNblitz.Services.Wallets;
using Microsoft.AspNetCore.Mvc;

namespace LNblitz.Pages.Transactions
{
    public class EditModel : BasePageModel
    {
        private readonly WalletService _walletService;
        public string WalletId { get; set; }
        public Transaction Transaction { get; set; }

        public EditModel(
            WalletService walletService,
            SettingsService settingsService) : base(settingsService)
        {
            _walletService = walletService;
        }

        public async Task<IActionResult> OnGetAsync(string walletId, string transactionId)
        {
            var userId = User.Claims.First(c => c.Type == "UserId").Value;
            WalletId = walletId;
            Transaction = await _walletService.GetTransaction(new TransactionQuery
            {
                UserId = userId,
                WalletId = walletId,
                TransactionId = transactionId
            });

            if (Transaction == null) return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string walletId, string transactionId)
        {
            var userId = User.Claims.First(c => c.Type == "UserId").Value;
            WalletId = walletId;
            Transaction = await _walletService.GetTransaction(new TransactionQuery
            {
                UserId = userId,
                WalletId = walletId,
                TransactionId = transactionId
            });

            if (!ModelState.IsValid) return Page();
            if (Transaction == null) return NotFound();

            if (await TryUpdateModelAsync<Transaction>(Transaction, "transaction", t => t.Description))
            {
                await _walletService.UpdateTransaction(Transaction);
                return RedirectToPage("./Index", new { walletId });
            }

            return Page();
        }
    }
}

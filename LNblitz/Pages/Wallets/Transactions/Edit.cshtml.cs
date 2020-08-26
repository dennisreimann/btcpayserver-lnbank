using System.Threading.Tasks;
using LNblitz.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LNblitz.Data.Queries;
using LNblitz.Data.Services;

namespace LNblitz.Pages.Wallets.Transactions
{
    public class EditModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly WalletManager _walletManager;
        public string WalletId { get; set; }
        public Transaction Transaction { get; set; }

        public EditModel(UserManager<User> userManager, WalletManager walletManager)
        {
            _userManager = userManager;
            _walletManager = walletManager;
        }

        public async Task<IActionResult> OnGetAsync(string walletId, string transactionId)
        {
            var userId = _userManager.GetUserId(User);
            WalletId = walletId;
            Transaction = await _walletManager.GetTransaction(new TransactionQuery
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
            var userId = _userManager.GetUserId(User);
            WalletId = walletId;
            Transaction = await _walletManager.GetTransaction(new TransactionQuery
            {
                UserId = userId,
                WalletId = walletId,
                TransactionId = transactionId
            });

            if (!ModelState.IsValid) return Page();
            if (Transaction == null) return NotFound();

            if (await TryUpdateModelAsync<Transaction>(
                Transaction,
                "transaction",
                t => t.Description))
            {
                await _walletManager.UpdateTransaction(Transaction);
                return RedirectToPage("./Index", new { walletId });
            }

            return Page();
        }
    }
}

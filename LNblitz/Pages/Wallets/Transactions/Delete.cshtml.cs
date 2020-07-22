using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LNblitz.Data.Queries;
using LNblitz.Data.Services;
using LNblitz.Models;

namespace LNblitz.Pages.Wallets.Transactions
{
    public class DeleteModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly WalletManager _walletManager;
        public string WalletId { get; set; }
        public Transaction Transaction { get; set; }

        public DeleteModel(UserManager<User> userManager, WalletManager walletManager)
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

            if (Transaction == null) return NotFound();

            await _walletManager.RemoveTransaction(Transaction);

            return RedirectToPage("./Index", new { walletId });
        }
    }
}

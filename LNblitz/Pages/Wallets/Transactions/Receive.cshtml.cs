using System.Threading.Tasks;
using BTCPayServer.Lightning;
using LNblitz.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LNblitz.Data.Queries;
using LNblitz.Data.Services;

namespace LNblitz.Pages.Wallets.Transactions
{
    public class ReceiveModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly WalletManager _walletManager;
        public Wallet Wallet { get; set; }
        public Transaction Transaction { get; set; }

        public ReceiveModel(UserManager<User> userManager, WalletManager walletManager)
        {
            _userManager = userManager;
            _walletManager = walletManager;
        }

        public async Task<IActionResult> OnGet(string walletId)
        {
            var userId = _userManager.GetUserId(User);
            Wallet = await _walletManager.GetWallet(new WalletQuery
            {
                UserId = userId,
                WalletId = walletId
            });

            if (Wallet == null) return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string walletId)
        {
            var userId = _userManager.GetUserId(User);
            Wallet = await _walletManager.GetWallet(new WalletQuery
            {
                UserId = userId,
                WalletId = walletId
            });

            if (Wallet == null) return NotFound();
            if (!ModelState.IsValid) return Page();

            long sats = long.Parse(Request.Form["Transaction.Amount"].ToString());

            Transaction = new Transaction
            {
                WalletId = walletId,
                Amount = new LightMoney(sats, LightMoneyUnit.Satoshi)
            };

            if (await TryUpdateModelAsync<Transaction>(Transaction, "Transaction", t => t.Description))
            {
                await _walletManager.CreateReceiveTransaction(Transaction);
                return RedirectToPage("./Index", new { walletId });
            }

            return Page();
        }
    }
}

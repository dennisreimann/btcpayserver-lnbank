using System.Threading.Tasks;
using LNblitz.Data.Models;
using LNblitz.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LNblitz.Pages.Wallets.Transactions
{
    public class DetailsModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly WalletService _walletService;
        public string WalletId { get; set; }
        public Transaction Transaction { get; set; }

        public DetailsModel(
            UserManager<User> userManager,
            WalletService walletService)
        {
            _userManager = userManager;
            _walletService = walletService;
        }

        public async Task<IActionResult> OnGetAsync(string walletId, string transactionId)
        {
            var userId = _userManager.GetUserId(User);
            WalletId = walletId;
            Transaction = await _walletService.GetTransaction(userId, walletId, transactionId);

            if (Transaction == null) return NotFound();

            return Page();
        }
    }
}

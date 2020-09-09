using System.Linq;
using System.Threading.Tasks;
using LNblitz.Data.Models;
using LNblitz.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LNblitz.Pages.Transactions
{
    public class DetailsModel : PageModel
    {
        private readonly WalletService _walletService;
        public string WalletId { get; set; }
        public Transaction Transaction { get; set; }

        public DetailsModel(WalletService walletService)
        {
            _walletService = walletService;
        }

        public async Task<IActionResult> OnGetAsync(string walletId, string transactionId)
        {
            var userId = User.Claims.First(c => c.Type == "UserId").Value;
            WalletId = walletId;
            Transaction = await _walletService.GetTransaction(userId, walletId, transactionId);

            if (Transaction == null) return NotFound();

            return Page();
        }
    }
}

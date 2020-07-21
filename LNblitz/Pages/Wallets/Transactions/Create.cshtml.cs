using System.Threading.Tasks;
using LNblitz.Data;
using LNblitz.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LNblitz.Pages.Wallets.Transactions
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        public Wallet Wallet { get; set; }
        public Transaction Transaction { get; set; }

        public CreateModel(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGet(int walletId)
        {
            var userId = _userManager.GetUserId(User);
            Wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.Id == walletId && w.UserId == userId);

            if (Wallet == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int walletId)
        {
            var userId = _userManager.GetUserId(User);
            Wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.Id == walletId && w.UserId == userId);

            if (Wallet == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            Transaction = new Transaction
            {
                WalletId = walletId
            };

            if (await TryUpdateModelAsync<Transaction>(Transaction, "transaction", t => t.Description, t => t.Amount))
            {
                _context.Transactions.Add(Transaction);
                await _context.SaveChangesAsync();
                return RedirectToPage("./Index", new { walletId = Wallet.Id });
            }

            return Page();
        }
    }
}

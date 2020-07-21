using System.Threading.Tasks;
using LNblitz.Data;
using LNblitz.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LNblitz.Pages.Wallets.Transactions
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        public Wallet Wallet { get; set; }
        public Transaction Transaction { get; set; }

        public DeleteModel(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync(int walletId, int id)
        {
            var userId = _userManager.GetUserId(User);
            Transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == id && t.WalletId == walletId);

            if (Transaction == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int walletId, int id)
        {
            var userId = _userManager.GetUserId(User);
            Wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.Id == walletId && w.UserId == userId);

            if (Wallet == null)
            {
                return NotFound();
            }

            Transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == id && t.WalletId == Wallet.Id);

            if (Transaction == null)
            {
                return NotFound();
            }

            _context.Transactions.Remove(Transaction);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index", new { walletId = Wallet.Id });
        }
    }
}

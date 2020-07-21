using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LNblitz.Data;
using LNblitz.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LNblitz.Pages.Wallets.Transactions
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        public Wallet Wallet { get; set; }
        public IList<Transaction> Transactions { get; set; }

        public IndexModel(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync(int walletId)
        {
            var userId = _userManager.GetUserId(User);
            Wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.Id == walletId && w.UserId == userId);

            if (Wallet == null)
            {
                return NotFound();
            }

            Transactions = await _context.Transactions
                .Where(t => t.WalletId == walletId)
                .ToListAsync();

            return Page();
        }
    }
}

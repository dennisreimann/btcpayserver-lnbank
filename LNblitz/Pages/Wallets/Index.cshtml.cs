using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LNblitz.Data;
using LNblitz.Models;
using Microsoft.EntityFrameworkCore;

namespace LNblitz.Pages.Wallets
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        public IList<Wallet> Wallets { get; set; }

        public IndexModel(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
            Wallets = await _context.Wallets
                .Where(w => w.UserId == userId)
                .ToListAsync();
        }
    }
}

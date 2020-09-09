using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using LNblitz.Data.Models;
using LNblitz.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LNblitz.Pages.Wallets
{
    public class ReceiveModel : PageModel
    {
        private readonly WalletService _walletService;
        public Wallet Wallet { get; set; }
        [BindProperty]
        public string Description { get; set; }
        [BindProperty]
        [DisplayName("Amount in sats")]
        [Required]
        public long Amount { get; set; }

        public ReceiveModel(WalletService walletService)
        {
            _walletService = walletService;
        }

        public async Task<IActionResult> OnGet(string walletId)
        {
            var userId = User.Claims.First(c => c.Type == "UserId").Value;
            Wallet = await _walletService.GetWallet(userId, walletId);

            if (Wallet == null) return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string walletId)
        {
            var userId = User.Claims.First(c => c.Type == "UserId").Value;
            Wallet = await _walletService.GetWallet(userId, walletId);

            if (Wallet == null) return NotFound();
            if (!ModelState.IsValid) return Page();

            try
            {
                await _walletService.Receive(Wallet, Amount, Description);
                return RedirectToPage("./Index", new { walletId });
            }
            catch (Exception)
            {
                return Page();
            }
        }
    }
}

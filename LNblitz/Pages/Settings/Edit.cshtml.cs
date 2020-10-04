using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LNblitz.Services.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LNblitz.Pages.Settings
{
    [Authorize("RequireAdmin")]
    public class EditModel : PageModel
    {
        private readonly SettingsService _settingsService;
        public AppSettings App { get; set; }
        public BtcPaySettings BtcPay { get; set; }

        public EditModel(SettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public void OnGet()
        {
            App = _settingsService.App;
            BtcPay = _settingsService.BtcPay;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            App = _settingsService.App;
            BtcPay = _settingsService.BtcPay;

            var results = await Task.WhenAll(new List<Task<bool>>
            {
                TryUpdateModelAsync(App, "App", w => w.Name),
                TryUpdateModelAsync(BtcPay, "BtcPay", w => w.Endpoint, w => w.StoreId, w => w.ApiKey)
            });

            if (results.All(r => r))
            {
                await _settingsService.SaveAsync();
                return RedirectToPage("./Edit");
            }

            return Page();
        }
    }
}

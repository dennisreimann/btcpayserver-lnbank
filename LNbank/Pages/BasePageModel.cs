using LNbank.Services.Settings;
using Microsoft.AspNetCore.Mvc.RazorPages;

// https://dotnetstories.com/blog/How-to-implement-a-custom-base-class-for-razor-views-in-ASPNET-Core-en-7106773524?o=rss
namespace LNbank.Pages
{
    public class BasePageModel : PageModel
    {
        public string AppName { get; }
        public bool NeedsSettings { get; }

        public BasePageModel(SettingsService settingsService)
        {
            AppName = settingsService.App.Name;
            NeedsSettings =
                string.IsNullOrEmpty(settingsService.BtcPay.Endpoint) ||
                string.IsNullOrEmpty(settingsService.BtcPay.StoreId) ||
                string.IsNullOrEmpty(settingsService.BtcPay.ApiKey);
        }

        public void SetTitle(string title) { ViewData["Title"] = title; }
    }
}

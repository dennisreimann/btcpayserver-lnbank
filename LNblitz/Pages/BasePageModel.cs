using LNblitz.Services.Settings;
using Microsoft.AspNetCore.Mvc.RazorPages;

// https://dotnetstories.com/blog/How-to-implement-a-custom-base-class-for-razor-views-in-ASPNET-Core-en-7106773524?o=rss
namespace LNblitz.Pages
{
    public class BasePageModel : PageModel
    {
        public string AppName { get; }

        public BasePageModel(SettingsService settingsService)
        {
            AppName = settingsService.App.Name;
        }

        public void SetTitle(string title) { ViewData["Title"] = title; }
    }
}

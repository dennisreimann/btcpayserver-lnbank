using System.Linq;
using LNbank.Services.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

// https://dotnetstories.com/blog/How-to-implement-a-custom-base-class-for-razor-views-in-ASPNET-Core-en-7106773524?o=rss
namespace LNbank.Pages
{
    public abstract class BasePageModel : PageModel
    {
        public const string SetupPath = "/Setup";
        public string AppName { get; }
        public bool NeedsSettings { get; }

        public BasePageModel(SettingsService settingsService)
        {
            AppName = settingsService.App.Name;
            NeedsSettings = settingsService.NeedsSettings;
        }

        public void SetTitle(string title) { ViewData["Title"] = title; }
        public bool IsSignedIn => User.HasClaim(c => c.Type == "UserId");
        public bool IsAdmin => User.HasClaim("IsAdmin", "True");
        public string UserId => IsSignedIn ? User.Claims.First(c => c.Type == "UserId").Value : null;

        public override void OnPageHandlerSelected(PageHandlerSelectedContext context)
        {
            EnsureSetup(context.HttpContext);
        }

        private void EnsureSetup(HttpContext ctx)
        {
            if (NeedsSettings)
            {
                if (ctx.Request.Path != SetupPath)
                {
                    ctx.Response.Redirect(SetupPath);
                }
            }
            else if (ctx.Request.Path == SetupPath)
            {
                var path = IsAdmin ? "/Settings/Edit" : "/";
                ctx.Response.Redirect(Url.Page(path));
            }
        }
    }
}

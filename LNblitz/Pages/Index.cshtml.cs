using LNblitz.Services.Settings;
using Microsoft.AspNetCore.Authorization;

namespace LNblitz.Pages
{
    [AllowAnonymous]
    public class IndexModel : BasePageModel
    {
        public IndexModel(SettingsService settingsService) : base(settingsService) {}

        public void OnGet()
        {
        }
    }
}

using LNbank.Services.Settings;
using Microsoft.AspNetCore.Authorization;

namespace LNbank.Pages
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

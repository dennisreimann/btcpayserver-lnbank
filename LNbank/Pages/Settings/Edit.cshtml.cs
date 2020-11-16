using LNbank.Services.Settings;
using Microsoft.AspNetCore.Authorization;

namespace LNbank.Pages.Settings
{
    [Authorize("RequireAdmin")]
    public class EditModel : SettingsModel
    {
        public EditModel(SettingsService settingsService) : base(settingsService)
        {
        }
    }
}

using System.Diagnostics;
using LNbank.Services.Settings;
using Microsoft.AspNetCore.Authorization;

namespace LNbank.Pages
{
    [AllowAnonymous]
    public class StatusCodeModel : BasePageModel
    {
        public string ErrorStatusCode { get; set; }
        public string ErrorDescription { get; set; }

        public StatusCodeModel(SettingsService settingsService) : base(settingsService) {}

        public void OnGet(string code)
        {
            ErrorStatusCode = code;

            switch (code)
            {
                case "404":
                    ErrorDescription = "This page does not exist.";
                    break;
                default:
                    ErrorDescription = "An error occurred while processing your request.";
                    break;
            }
        }
    }
}

using LNbank.Services.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;

namespace LNbank.Extensions
{
    public static class AuthenticationExtensions
    {
        public static void AddAppAuthentication(this IServiceCollection services)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/";
                    options.LogoutPath = "/logout";
                    options.Cookie.Name = "LNbank";
                })
                .AddScheme<AuthenticationSchemeOptions, BTCPayAPIKeyAuthenticationHandler>(AuthenticationSchemes.ApiBTCPayAPIKey, o => {})
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(AuthenticationSchemes.ApiBasic, o => {});
        }
    }
}

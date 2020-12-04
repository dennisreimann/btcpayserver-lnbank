using LNbank.Authentication;
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
                .AddScheme<AuthenticationSchemeOptions, BTCPayAPIKeyAuthenticationHandler>(AuthenticationSchemes.ApiBTCPayAPIKey, o => {})
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(AuthenticationSchemes.ApiBasic, o => {})
                .AddCookie(options =>
                {
                    // Foward API and Hub requests to API key scheme
                    options.ForwardDefaultSelector = ctx =>
                        ctx.Request.Path.StartsWithSegments("/api") || ctx.Request.Path.StartsWithSegments("/Hubs")
                            ? AuthenticationSchemes.ApiBTCPayAPIKey
                            : null;

                    options.LoginPath = "/Login";
                    options.LogoutPath = "/Logout";
                    options.Cookie.Name = "LNbank";
                });
        }
    }
}

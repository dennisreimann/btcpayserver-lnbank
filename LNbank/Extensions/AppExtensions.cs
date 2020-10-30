using LNbank.Configuration;
using LNbank.Services;
using LNbank.Services.Settings;
using LNbank.Services.Users;
using LNbank.Services.Wallets;
using Microsoft.Extensions.DependencyInjection;

namespace LNbank.Extensions
{
    public static class AppExtensions
    {
        public static void AddAppServices(this IServiceCollection services, AppOptions appOptions)
        {
            services.AddSingleton<IAppOptions>(appOptions);
            services.AddHostedService<LightningInvoiceWatcher>();
            services.AddScoped<BTCPayService>();
            services.AddScoped<SettingsService>();
            services.AddScoped<WalletService>();
            services.AddScoped<UserService>();
        }
    }
}

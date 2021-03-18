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
        public static void AddAppServices(this IServiceCollection services)
        {
            services.AddHostedService<LightningInvoiceWatcher>();
            services.AddSingleton<BTCPayService>();
            services.AddSingleton<SettingsService>();
            services.AddSingleton<WalletService>();
            services.AddSingleton<UserService>();
        }
    }
}

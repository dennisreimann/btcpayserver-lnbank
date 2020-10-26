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
        public static void AddAppServices(this IServiceCollection collection, AppOptions appOptions)
        {
            collection.AddSingleton<IAppOptions>(appOptions);
            collection.AddHostedService<LightningInvoiceWatcher>();
            collection.AddScoped<BTCPayService>();
            collection.AddScoped<SettingsService>();
            collection.AddScoped<WalletService>();
            collection.AddScoped<UserService>();
        }
    }
}

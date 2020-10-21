using LNblitz.Services;
using LNblitz.Services.Settings;
using LNblitz.Services.Users;
using LNblitz.Services.Wallets;
using Microsoft.Extensions.DependencyInjection;

namespace LNblitz.Extensions
{
    public static class AppExtensions
    {
        public static void AddAppServices(this IServiceCollection collection)
        {
            collection.AddHostedService<LightningInvoiceWatcher>();
            collection.AddScoped<BTCPayService>();
            collection.AddScoped<SettingsService>();
            collection.AddScoped<WalletService>();
            collection.AddScoped<UserService>();
        }
    }
}

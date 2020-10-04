using LNblitz.Services;
using LNblitz.Services.Settings;
using LNblitz.Services.Users;
using LNblitz.Services.Wallets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LNblitz.Extensions
{
    public static class LNblitzExtensions
    {
        public static void AddAppServices(this IServiceCollection collection, IConfiguration configuration)
        {
            collection.Configure<LNblitzConfiguration>(configuration.GetSection("BTCPay"));
            collection.AddHostedService<LightningInvoiceWatcher>();
            collection.AddSingleton<BTCPayService>();
            collection.AddScoped<SettingsService>();
            collection.AddScoped<WalletService>();
            collection.AddScoped<UserService>();
        }
    }
}

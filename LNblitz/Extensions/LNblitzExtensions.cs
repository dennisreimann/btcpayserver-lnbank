using LNblitz.Data.Services;
using LNblitz.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LNblitz.Extensions
{
    public static class LNblitzExtensions
    {
        public static void AddAppServices(this IServiceCollection collection, IConfiguration configuration)
        {
            collection.AddHostedService<LightningInvoiceWatcher>();
            collection.Configure<LNblitzConfiguration>(configuration.GetSection("BTCPay"));
            collection.AddSingleton<WalletManager>();
            collection.AddSingleton<WalletService>();
            collection.AddSingleton<BTCPayService>();
        }
    }
}

using LNblitz.Data.Services;
using LNblitz.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LNblitz.Extensions
{
    public static class LNblitzExtensions
    {
        public static void AddAppServices(this IServiceCollection collection)
        {
            collection.AddHostedService<LightningInvoiceWatcher>();

            collection.AddSingleton<WalletManager>();
            collection.AddSingleton<WalletService>();
            collection.AddSingleton<BTCPayService>();
        }
    }
}

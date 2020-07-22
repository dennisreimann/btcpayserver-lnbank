using LNblitz.Data.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LNblitz.Extensions
{
    public static class LNblitzExtensions
    {
        public static void AddAppServices(this IServiceCollection collection)
        {
            collection.AddSingleton<WalletManager>();
        }
    }
}

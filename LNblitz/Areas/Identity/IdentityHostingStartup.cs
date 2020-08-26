using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(LNblitz.Areas.Identity.IdentityHostingStartup))]
namespace LNblitz.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
            });
        }
    }
}

using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace LNblitz.Hubs
{
    public class InvoiceHub : Hub
    {
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("Message", message);
        }
    }
}

using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using VueServer.Models.Chat;

namespace VueServer.Services.Hubs
{
    public class ChatHub : Hub<IChatHub>
    {
        public async Task SendMessage(ChatMessage message)
        {
            if (Clients != null)
            {
                await Clients.All.SendMessage(message);
            }
        }

        public async Task ReadMessage(ReadReceipt receipt)
        {
            if (Clients != null)
            {
                await Clients.All.ReadMessage(receipt);
            }
        }
    }
}

using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using VueServer.Models.Chat;

namespace VueServer.Services.Hubs
{
    public class ChatHub : Hub<IChatHub>
    {
        public async Task SendMessage(ChatMessage message)
        {
            await Clients.All.SendMessage(message);
        }
    }
}

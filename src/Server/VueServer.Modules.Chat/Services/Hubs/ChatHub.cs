using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VueServer.Modules.Chat.Models;
using VueServer.Modules.Chat.Services.Chat;

namespace VueServer.Modules.Chat.Services.Hubs
{
    [Authorize]
    public class ChatHub : Hub<IChatHub>
    {
        //private static readonly ConnectionMapping<string> _connections = new ConnectionMapping<string>();

        // TODO: Convert this to using the Caching system that will link each UserId to a list of ConversationIds
        private readonly IChatService _chatService;
        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

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

        public override async Task OnConnectedAsync()
        {
            string name = GetUserName(Context.User);
            var convos = await GetConversationsForUser(name);
            foreach (var convo in convos)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, convo.Id.ToString());
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string name = GetUserName(Context.User);
            var convos = await GetConversationsForUser(name);
            foreach (var convo in convos)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, convo.Id.ToString());
            }
        }

        private string GetUserName(ClaimsPrincipal user)
        {
            return user.Claims.Where(x => x.Type == "sub").Select(x => x.Value).FirstOrDefault();
        }

        private async Task<IEnumerable<Conversation>> GetConversationsForUser(string user)
        {
            if (string.IsNullOrWhiteSpace(user))
            {
                return await Task.FromResult(Enumerable.Empty<Conversation>());
            }

            var convos = await _chatService.GetAllConversationsForUser(user);
            if (convos.Obj == null || convos.Obj.Count() == 0)
            {
                return await Task.FromResult(Enumerable.Empty<Conversation>());
            }

            return convos.Obj;

        }
    }
}

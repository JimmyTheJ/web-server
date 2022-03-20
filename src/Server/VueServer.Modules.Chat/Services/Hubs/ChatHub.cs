using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VueServer.Modules.Chat.Models;

namespace VueServer.Modules.Chat.Services.Hubs
{
    [Authorize]
    public class ChatHub : Hub<IChatHub>
    {
        private static readonly UserConnectionMapping _userConnections = new UserConnectionMapping();

        private readonly IChatService _chatService;
        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public async Task SendMessage(ChatMessage message)
        {
            if (message != null)
            {
                await Clients.All.SendMessage(message);
            }
        }

        public async Task ReadMessage(ReadReceipt receipt)
        {
            if (receipt != null)
            {
                await Clients.All.ReadMessage(receipt);
            }
        }

        public async Task ConversationCreated(Conversation conversation)
        {
            if (conversation != null)
            {
                await Clients.All.ConversationCreated(conversation);
            }
        }

        public async Task ConversationDeleted(long conversationId)
        {
            if (conversationId > 0)
            {
                await Clients.All.ConversationDeleted(conversationId);
            }
        }

        public async Task CreateConversation(Conversation conversation)
        {
            if (conversation != null)
            {
                var userIds = conversation.ConversationUsers.Select(x => x.UserId).ToList();
                foreach (var conn in _userConnections.GetConnections(userIds))
                {
                    await Groups.AddToGroupAsync(conn, conversation.Id.ToString());
                }
                await Clients.Clients(_userConnections.GetConnections(userIds.Where(x => x != GetUserName(Context.User)))).ConversationCreated(conversation);
            }
        }

        public async Task DeleteConversation(Conversation conversation)
        {
            if (conversation != null)
            {
                var userIds = conversation.ConversationUsers.Select(x => x.UserId).ToList();
                foreach (var conn in _userConnections.GetConnections(userIds))
                {
                    await Groups.RemoveFromGroupAsync(conn, conversation.Id.ToString());
                }
                await Clients.Clients(_userConnections.GetConnections(userIds.Where(x => x != GetUserName(Context.User)))).ConversationDeleted(conversation.Id);
            }
        }

        public override async Task OnConnectedAsync()
        {
            string name = GetUserName(Context.User);
            // TODO: Convert this to using the Caching system that will link each UserId to a list of ConversationIds
            var convos = await GetConversationsForUser(name);
            foreach (var convo in convos)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, convo.Id.ToString());
                _userConnections.Add(name, Context.ConnectionId);
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string name = GetUserName(Context.User);
            // TODO: Convert this to using the Caching system that will link each UserId to a list of ConversationIds
            var convos = await GetConversationsForUser(name);
            foreach (var convo in convos)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, convo.Id.ToString());
                _userConnections.Remove(name, Context.ConnectionId);
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

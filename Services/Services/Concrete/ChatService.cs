using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using VueServer.Domain.Concrete;
using VueServer.Domain.Interface;
using VueServer.Models.Chat;
using VueServer.Models.Context;
using VueServer.Models.Request;
using VueServer.Services.Hubs;
using VueServer.Services.Interface;

namespace VueServer.Services.Concrete
{
    public class ChatService : IChatService
    {
        private readonly IWSContext _context;
        private readonly IHubContext<ChatHub, IChatHub> _chatHubContext;
        private readonly IUserService _user;
        private readonly ILogger _logger;

        public ChatService(IWSContext context, IHubContext<ChatHub, IChatHub> chatHubContext, ILoggerFactory logger, IUserService user)
        {
            _context = context ?? throw new ArgumentNullException("WSContext is null");
            _chatHubContext = chatHubContext ?? throw new ArgumentNullException("Chat Hub context is null");
            _user = user ?? throw new ArgumentNullException("User service is null");
            _logger = logger?.CreateLogger<ChatService>() ?? throw new ArgumentNullException("Logger factory is null");
        }

        public async Task<IResult<Conversation>> StartConversation(StartConversationRequest request)
        {
            if (request == null || request.Users == null || request.Users.Count() == 0)
            {
                return new Result<Conversation>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            var conversation = new Conversation();
            _context.Conversations.Add(conversation);

            var conversationUserList = new List<ConversationHasUser>();

            // Add self to conversation
            var selfUser = new ConversationHasUser()
            {
                ConversationId = conversation.Id,
                UserId = _user.Id,
                Owner = true
            };
            _context.ConversationHasUser.Add(selfUser);
            conversationUserList.Add(selfUser);

            // Add all the other users to the conversation
            foreach (var user in request.Users.Where(x => x != _user.Id))
            {
                if (string.IsNullOrWhiteSpace(user)) {
                    continue;
                }

                var conversationUser = new ConversationHasUser()
                {
                    ConversationId = conversation.Id,
                    UserId = user
                };
                _context.ConversationHasUser.Add(conversationUser);
                conversationUserList.Add(conversationUser);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogError("StartConversation: Error saving database on starting a conversation");
                return new Result<Conversation>(null, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            conversation.ConversationUsers = conversationUserList;
            conversation.Messages = new List<ChatMessage>();
            return new Result<Conversation>(conversation, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<Conversation>> GetConversation(Guid id)
        {
            var conversation = await _context.Conversations
                .Include(x => x.Messages)
                .Include(x => x.ConversationUsers)
                .Where(x => x.Id == id)
                .OrderByDescending(x => x.Messages.OrderByDescending(y => y.Timestamp))
                .SingleOrDefaultAsync();

            return new Result<Conversation>(conversation, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<IEnumerable<Conversation>>> GetAllConversations(string userId)
        {
            var conversationUsers = await _context.ConversationHasUser.Where(x => x.UserId == userId).ToListAsync();
            if (conversationUsers == null)
            {
                return new Result<IEnumerable<Conversation>>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            var conversationList = await _context.Conversations
                .Include(x => x.Messages)
                .Include(x => x.ConversationUsers)
                .Where(x => x.ConversationUsers.Any(y => y.ConversationId == x.Id && y.UserId == userId))
                .ToListAsync();

            return new Result<IEnumerable<Conversation>>(conversationList, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<ChatMessage>> GetMessage(Guid id)
        {
            var message = await _context.Messages.Where(x => x.Id == id).SingleOrDefaultAsync();
            if (message == null)
            {
                return new Result<ChatMessage>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            return new Result<ChatMessage>(message, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<ChatMessage>> AddMessage(ChatMessage message)
        {
            if (message == null || string.IsNullOrWhiteSpace(message.Text))
            {
                return new Result<ChatMessage>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            var newMessage = new ChatMessage()
            {
                ConversationId = message.ConversationId,
                Text = message.Text,
                UserId = _user.Name,
                Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds()
            };

            _context.Messages.Add(newMessage);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogError("AddMessage: Error saving database on adding a message");
                return new Result<ChatMessage>(null, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            await _chatHubContext.Clients.All.SendMessage(newMessage);

            return new Result<ChatMessage>(newMessage, Domain.Enums.StatusCode.OK);
        }
    }    
}

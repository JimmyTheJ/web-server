using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using VueServer.Domain;
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

            // Get current user, this shouldn't be able to fail
            var currentUser = await _user.GetUserByNameAsync(_user.Name);

            // Get all this users conversations to see if they are trying to make a conversation that already exists
            var allUsersConversations = (await GetAllConversations(currentUser.Id))?.Obj;
            if (allUsersConversations != null && allUsersConversations.Count() > 0)
            {
                var allUsersInEachConvo = allUsersConversations.Select(x => x.ConversationUsers).ToList();
                foreach (var convo in allUsersInEachConvo)
                {
                    // Same number of users, so it's possible we have a match
                    if (convo.Count == request.Users.Count())
                    {
                        var userMatches = convo.Where(x => request.Users.Contains(x.UserId));
                        // All users match, can't create this conversation
                        if (userMatches.Count() == request.Users.Count())
                        {
                            return new Result<Conversation>(null, Domain.Enums.StatusCode.NO_CONTENT);
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            // Add current user to conversation
            var selfUser = new ConversationHasUser()
            {
                ConversationId = conversation.Id,
                UserId = currentUser.Id,
                Owner = true
            };
            _context.ConversationHasUser.Add(selfUser);
            conversationUserList.Add(selfUser);           

            // Add all the other users to the conversation
            foreach (var username in request.Users.Where(x => x != currentUser.Id))
            {
                if (string.IsNullOrWhiteSpace(username)) {
                    continue;
                }

                var user = await _user.GetUserByIdAsync(username);
                if (user == null) 
                {
                    _logger.LogInformation($"Invalid userId: {username}. Cannot create a conversation with this user as they don't exist.");
                    continue;
                }

                var conversationUser = new ConversationHasUser()
                {
                    ConversationId = conversation.Id,
                    UserId = user.Id,
                    UserDisplayName = user.DisplayName
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
                .SingleOrDefaultAsync();

            // Sort messages
            conversation.Messages.OrderByDescending(x => x.Timestamp);

            // TODO: This is very inneficient. Figure out a better way with LINQ
            foreach (var userConversation in conversation.ConversationUsers)
            {
                var usr = await _user.GetUserByIdAsync(userConversation.UserId);
                userConversation.UserDisplayName = usr.DisplayName;
            }

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

            foreach(var conversation in conversationList)
            {
                if (conversation.ConversationUsers != null)
                {
                    // TODO: This is very inneficient. Figure out a better way with LINQ
                    foreach (var userConversation in conversation.ConversationUsers)
                    {
                        // Don't bother doing a lookup on our own name
                        if (userConversation.UserId == userId)
                        {
                            continue;
                        }

                        var usr = await _user.GetUserByIdAsync(userConversation.UserId);
                        userConversation.UserDisplayName = usr.DisplayName;
                    }
                }

                if (conversation.Messages != null)
                {
                    // Sort messages
                    conversation.Messages.OrderByDescending(x => x.Timestamp);
                }
            }

            return new Result<IEnumerable<Conversation>>(conversationList, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<bool>> UpdateConversationTitle(Guid conversationId, string title)
        {
            var user = await _user.GetUserByNameAsync(_user.Name);
            if (user == null)
            {
                _logger.LogWarning($"UpdateConversationTitle: Unable to get user by name with name ({_user.Name})");
                return new Result<bool>(false, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            var conversation = (await GetConversation(conversationId))?.Obj;
            if (conversation == null)
            {
                _logger.LogInformation($"UpdateConversationTitle: Conversation with id ({conversationId}) does not exist. Cannot delete it");
                return new Result<bool>(false, Domain.Enums.StatusCode.NO_CONTENT);
            }

            var selfUser = conversation.ConversationUsers.Where(x => x.UserId == user.Id).SingleOrDefault();
            if (selfUser == null)
            {
                _logger.LogWarning($"UpdateConversationTitle: Conversation with id ({conversationId}) does not include user ({user.Id}) as one of it's members. This is likely an escalation attack");
                return new Result<bool>(false, Domain.Enums.StatusCode.FORBIDDEN);
            }

            if (!selfUser.Owner)
            {
                _logger.LogInformation($"UpdateConversationTitle: User ({user.Id}) is not the owner of this conversation ({conversationId}). They cannot change it's title.");
                return new Result<bool>(false, Domain.Enums.StatusCode.UNAUTHORIZED);
            }

            conversation.Title = title;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogError("UpdateConversationTitle: Error saving database on updating the conversation's title");
                return new Result<bool>(false, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            return new Result<bool>(true, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<bool>> DeleteConversation(Guid conversationId)
        {
            var user = await _user.GetUserByNameAsync(_user.Name);
            if (user == null)
            {
                _logger.LogWarning($"DeleteConversation: Unable to get user by name with name ({_user.Name})");
                return new Result<bool>(false, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            if (_context.UserHasFeature.Where(x => x.ModuleFeatureId == Constants.Models.ModuleFeatures.Chat.DELETE_CONVERSATION_ID && x.UserId == user.Id).SingleOrDefault() == null)
            {
                _logger.LogInformation($"DeleteConversation: User ({user.Id}) does not have permission to delete conversations");
                return new Result<bool>(false, Domain.Enums.StatusCode.FORBIDDEN);
            }

            var conversation = (await GetConversation(conversationId))?.Obj;
            if (conversation == null)
            {
                _logger.LogInformation($"DeleteConversation: Conversation with id ({conversationId}) does not exist. Cannot delete it");
                return new Result<bool>(false, Domain.Enums.StatusCode.NO_CONTENT);
            }

            _context.ConversationHasUser.RemoveRange(conversation.ConversationUsers);
            _context.Messages.RemoveRange(conversation.Messages);
            _context.Conversations.Remove(conversation);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogError("DeleteConversation: Error saving database on deleting conversation");
                return new Result<bool>(false, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            return new Result<bool>(true, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<IEnumerable<ChatMessage>>> GetMessagesForConversation(Guid id)
        {
            var user = await _user.GetUserByNameAsync(_user.Name);
            if (user == null)
            {
                _logger.LogWarning($"GetMessagesForConversation: Unable to get user by name with name ({_user.Name})");
                return new Result<IEnumerable<ChatMessage>>(null, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            var conversation = await _context.Conversations.Include(x => x.ConversationUsers).Include(x => x.Messages).Where(x => x.Id == id).SingleOrDefaultAsync();
            if (conversation == null)
            {
                _logger.LogWarning($"GetMessagesForConversation: Unable to get conversation by id ({id})");
                return new Result<IEnumerable<ChatMessage>>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            if (!conversation.ConversationUsers.Any(x => x.UserId == user.Id))
            {
                _logger.LogInformation($"GetMessagesForConversation: User ({user.Id}) is not part of the conversation with id ({id}), cannot access this conversation");
                return new Result<IEnumerable<ChatMessage>>(null, Domain.Enums.StatusCode.FORBIDDEN);
            }

            return new Result<IEnumerable<ChatMessage>>(conversation.Messages, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<bool>> DeleteMessage(Guid messageId)
        {
            var user = await _user.GetUserByNameAsync(_user.Name);
            if (user == null)
            {
                _logger.LogWarning($"DeleteMessage: Unable to get user by name with name ({_user.Name})");
                return new Result<bool>(false, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            if (_context.UserHasFeature.Where(x => x.ModuleFeatureId == Constants.Models.ModuleFeatures.Chat.DELETE_MESSAGE_ID && x.UserId == user.Id).SingleOrDefault() == null)
            {
                _logger.LogInformation($"DeleteMessage: User ({user.Id}) does not have permission to delete messages");
                return new Result<bool>(false, Domain.Enums.StatusCode.FORBIDDEN);
            }

            var message = _context.Messages.Where(x => x.Id == messageId).SingleOrDefault();
            if (message == null)
            {
                _logger.LogInformation($"DeleteMessage: Message with id ({messageId}) does not exist. Cannot delete it");
                return new Result<bool>(false, Domain.Enums.StatusCode.NO_CONTENT);
            }

            // User deleting the message is the one who sent the message
            if (message.UserId == user.Id)
            {
                _context.Messages.Remove(message);
            }
            else
            {
                // If another user is trying to delete a user's messages ensure that user is an administrator
                var userRoles = await _user.GetUserRolesAsync(user);
                if (userRoles != null && userRoles.Contains(Constants.Authentication.ADMINISTRATOR_STRING))
                {
                    _context.Messages.Remove(message);
                }
                else
                {
                    _logger.LogInformation($"DeleteMessage: User ({user.Id}) does not have permission to delete messages");
                    return new Result<bool>(false, Domain.Enums.StatusCode.FORBIDDEN);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogError("DeleteMessage: Error saving database on deleting message");
                return new Result<bool>(false, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            return new Result<bool>(true, Domain.Enums.StatusCode.OK);
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

            var user = await _user.GetUserByNameAsync(_user.Name);
            if (user == null)
            {
                _logger.LogWarning($"AddMessage: Unable to get user by name with name ({_user.Name})");
                return new Result<ChatMessage>(null, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            var newMessage = new ChatMessage()
            {
                ConversationId = message.ConversationId,
                Text = message.Text,
                UserId = user.Id,
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

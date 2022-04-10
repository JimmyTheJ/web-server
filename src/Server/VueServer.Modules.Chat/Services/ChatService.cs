using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VueServer.Core.Objects;
using VueServer.Domain;
using VueServer.Domain.Interface;
using VueServer.Modules.Chat.Context;
using VueServer.Modules.Chat.Models;
using VueServer.Modules.Chat.Models.Request;
using VueServer.Modules.Chat.Services.Hubs;
using VueServer.Modules.Core.Cache;
using VueServer.Modules.Core.Models.Response;
using VueServer.Modules.Core.Services.User;

namespace VueServer.Modules.Chat.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatContext _context;
        private readonly IVueServerCache _cache;
        private readonly IHubContext<ChatHub, IChatHub> _chatHubContext;
        private readonly IUserService _user;
        private readonly ILogger _logger;

        private const int NUM_MESSAGES = 50;

        public ChatService(IChatContext context, IVueServerCache cache, IHubContext<ChatHub, IChatHub> chatHubContext, ILoggerFactory logger, IUserService user)
        {
            _context = context ?? throw new ArgumentNullException("Chat Context is null");
            _cache = cache ?? throw new ArgumentNullException("VueServer Cache is null");
            _chatHubContext = chatHubContext ?? throw new ArgumentNullException("Chat Hub context is null");
            _user = user ?? throw new ArgumentNullException("User service is null");
            _logger = logger?.CreateLogger<ChatService>() ?? throw new ArgumentNullException("Logger factory is null");
        }

        #region -> Public Functions

        public async Task<IResult<IEnumerable<WSUserResponse>>> GetActiveConversationUsers()
        {
            var conversationUsers = await _context.Conversations.Include(x => x.ConversationUsers)
                .Where(x => x.ConversationUsers.Any(y => y.ConversationId == x.Id && y.UserId == _user.Id))
                .SelectMany(x => x.ConversationUsers)
                .Select(x => x.UserId)
                .Distinct()
                .ToListAsync();
            if (conversationUsers == null)
            {
                return new Result<IEnumerable<WSUserResponse>>(Enumerable.Empty<WSUserResponse>(), Domain.Enums.StatusCode.NO_CONTENT);
            }

            var users = await _context.Users.Where(x => conversationUsers.Contains(x.Id)).Include(x => x.UserProfile).ToListAsync();
            return new Result<IEnumerable<WSUserResponse>>(users.Select(x => WSUserResponse.ConvertWSUserToResponse(x)), Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<IEnumerable<WSUserResponse>>> GetUsersFromConversation(long id)
        {
            var conversationUsers = await _context.Conversations.Include(x => x.ConversationUsers)
                .Where(x => x.Id == id)
                .SelectMany(x => x.ConversationUsers)
                .Select(x => x.UserId)
                .ToListAsync();
            if (conversationUsers == null)
            {
                return new Result<IEnumerable<WSUserResponse>>(Enumerable.Empty<WSUserResponse>(), Domain.Enums.StatusCode.NO_CONTENT);
            }

            var users = await _context.Users.Where(x => conversationUsers.Contains(x.Id)).Include(x => x.UserProfile).ToListAsync();
            return new Result<IEnumerable<WSUserResponse>>(users.Select(x => WSUserResponse.ConvertWSUserToResponse(x)), Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<Conversation>> StartConversation(StartConversationRequest request)
        {
            if (request == null || request.Users == null || request.Users.Count() == 0)
            {
                return new Result<Conversation>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            // Get all this users conversations to see if they are trying to make a conversation that already exists
            var allUsersConversations = (await GetAllConversations())?.Obj;
            if (allUsersConversations != null && allUsersConversations.Count() > 0)
            {
                var allUsersInEachConvo = allUsersConversations.Select(x => x.ConversationUsers).ToList();
                foreach (var convo in allUsersInEachConvo)
                {
                    // Same number of users, so it's possible. Add 1 is because the request doesn't contain the active user making the request
                    if (convo.Count == request.Users.Count() + 1)
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

            var conversationUserList = new List<ConversationHasUser>();
            var conversation = new Conversation();
            _context.Conversations.Add(conversation);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError($"[{this.GetType().Name}] {nameof(StartConversation)}: Error saving database on starting a conversation", e.StackTrace);
                return new Result<Conversation>(null, Domain.Enums.StatusCode.SERVER_ERROR);
            }
            var colorId = 0;

            // Add current user to conversation
            var selfUser = new ConversationHasUser()
            {
                ConversationId = conversation.Id,
                UserId = _user.Id,
                Owner = true,
                Color = GetUserColor(colorId++)

            };
            conversationUserList.Add(selfUser);

            // Add all the other users to the conversation
            foreach (var username in request.Users.Where(x => x != _user.Id))
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    continue;
                }

                var user = await _user.GetUserByIdAsync(username);
                if (user == null)
                {
                    _logger.LogInformation($"[{this.GetType().Name}] {nameof(StartConversation)}: Invalid userId: '{username}'. Cannot create a conversation with this user as they don't exist.");
                    continue;
                }

                var conversationUser = new ConversationHasUser()
                {
                    ConversationId = conversation.Id,
                    UserId = user.Id,
                    Color = GetUserColor(colorId++)
                };

                conversationUserList.Add(conversationUser);
            }

            _context.ConversationHasUser.AddRange(conversationUserList);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError($"[{this.GetType().Name}] {nameof(StartConversation)}: Error saving database on adding users to a conversation", e.StackTrace);
                return new Result<Conversation>(null, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            conversation.ConversationUsers = conversationUserList;
            conversation.Messages = new List<ChatMessage>();
            MapConversationMetaData(conversation);

            return new Result<Conversation>(conversation, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<Conversation>> GetConversation(long id)
        {
            var conversation = await _context.Conversations
                .Include(x => x.Messages)
                .Include(x => x.ConversationUsers)
                .Where(x => x.Id == id)
                .SingleOrDefaultAsync();
            if (conversation == null)
            {
                return new Result<Conversation>(null, Domain.Enums.StatusCode.NOT_FOUND);
            }

            // Block bad actors attempting to access or modify information from conversations they are not part of
            if (!conversation.ConversationUsers.Any(x => x.UserId == _user.Id))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(GetConversation)}: User ({_user.Id}) is not part of the conversation with id ({conversation.Id}), this action is forbidden");
                return new Result<Conversation>(null, Domain.Enums.StatusCode.FORBIDDEN);
            }

            // Sort messages
            conversation.Messages.OrderByDescending(x => x.Timestamp);
            MapConversationMetaData(conversation);

            return new Result<Conversation>(conversation, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<IEnumerable<Conversation>>> GetNewMessageNotifications()
        {
            var conversationList = await GetAllConversationAsync(_user.Id, GetMessageType.New);
            if (conversationList == null)
            {
                return new Result<IEnumerable<Conversation>>(null, Domain.Enums.StatusCode.NO_CONTENT);
            }

            return new Result<IEnumerable<Conversation>>(conversationList, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<IEnumerable<Conversation>>> GetAllConversations()
        {
            return await GetAllConversationsForUser(_user.Id);
        }

        /// <summary>
        /// DO NOT CALL this from public API controller. Would allow unsafe code path to get any user's conversations
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public async Task<IResult<IEnumerable<Conversation>>> GetAllConversationsForUser(string userName)
        {
            var conversationList = await GetAllConversationAsync(userName);
            if (conversationList != null)
            {
                foreach (var convo in conversationList)
                {
                    foreach (var user in convo.ConversationUsers)
                    {
                        if (_cache.GetSubDictionaryValue(CacheMap.Users, user.UserId, out UserInfoCache info))
                        {
                            user.UserDisplayName = info.DisplayName;
                        }
                    }
                }
            }

            return new Result<IEnumerable<Conversation>>(conversationList, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<bool>> UpdateConversationTitle(long conversationId, string title)
        {
            var conversation = (await GetConversation(conversationId))?.Obj;
            if (conversation == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(UpdateConversationTitle)}: Conversation with id ({conversationId}) does not exist. Cannot delete it");
                return new Result<bool>(false, Domain.Enums.StatusCode.NO_CONTENT);
            }

            // Block bad actors attempting to access or modify information from conversations they are not part of
            var selfUser = conversation.ConversationUsers.Where(x => x.UserId == _user.Id).SingleOrDefault();
            if (selfUser == null)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(UpdateConversationTitle)}: Conversation with id ({conversationId}) does not include user ({_user.Id}) as one of it's members. This is likely an escalation attack");
                return new Result<bool>(false, Domain.Enums.StatusCode.FORBIDDEN);
            }

            if (!selfUser.Owner)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(UpdateConversationTitle)}: User ({_user.Id}) is not the owner of this conversation ({conversationId}). They cannot change it's title.");
                return new Result<bool>(false, Domain.Enums.StatusCode.UNAUTHORIZED);
            }

            conversation.Title = title;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogError($"[{this.GetType().Name}] {nameof(UpdateConversationTitle)}: Error saving database on updating the conversation's title");
                return new Result<bool>(false, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            return new Result<bool>(true, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<string>> UpdateUserColor(long conversationId, string userId, int colorId)
        {
            var conversationHasUsers = await _context.ConversationHasUser.Where(x => x.ConversationId == conversationId).ToListAsync();
            if (conversationHasUsers == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(UpdateUserColor)}: ConversationHasUser with id ({conversationId}) does not exist. Cannot update the color for it");
                return new Result<string>(null, Domain.Enums.StatusCode.NO_CONTENT);
            }

            // Block bad actors attempting to access or modify information from conversations they are not part of
            if (!conversationHasUsers.Any(x => x.UserId == _user.Id))
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(UpdateUserColor)}: User ({_user.Id}) is not part of the conversation with id ({conversationId}), this action is forbidden");
                return new Result<string>(null, Domain.Enums.StatusCode.FORBIDDEN);
            }

            var user = conversationHasUsers.Where(x => x.UserId == userId).SingleOrDefault();
            if (user == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(UpdateUserColor)}: Color request for user ({userId}) failed because they are not part of the conversation with id ({conversationId})");
                return new Result<string>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            user.Color = GetUserColor(colorId);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogError($"[{this.GetType().Name}] {nameof(UpdateUserColor)}: Error saving database on deleting conversation");
                return new Result<string>(null, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            return new Result<string>(user.Color, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<bool>> DeleteConversation(long conversationId)
        {
            var conversation = (await GetConversation(conversationId))?.Obj;
            if (conversation == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(DeleteConversation)}: Conversation with id ({conversationId}) does not exist. Cannot delete it");
                return new Result<bool>(false, Domain.Enums.StatusCode.NO_CONTENT);
            }

            // Block bad actors attempting to access or modify information from conversations they are not part of
            if (!conversation.ConversationUsers.Any(x => x.UserId == _user.Id))
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(DeleteConversation)}: User ({_user.Id}) is not part of the conversation with id ({conversationId}), this action is forbidden");
                return new Result<bool>(false, Domain.Enums.StatusCode.FORBIDDEN);
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
                _logger.LogError($"[{this.GetType().Name}] {nameof(DeleteConversation)}: Error saving database on deleting conversation");
                return new Result<bool>(false, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            return new Result<bool>(true, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<IEnumerable<ChatMessage>>> GetMessagesForConversation(long conversationId)
        {
            return await GetMessagesForConversation(conversationId, -1);
        }

        public async Task<IResult<IEnumerable<ChatMessage>>> GetMessagesForConversation(long conversationId, long msgId)
        {
            var conversation = await _context.Conversations.Include(x => x.ConversationUsers).Where(x => x.Id == conversationId).SingleOrDefaultAsync();
            if (conversation == null)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(GetMessagesForConversation)}: Unable to get conversation by id ({conversationId})");
                return new Result<IEnumerable<ChatMessage>>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            if (!conversation.ConversationUsers.Any(x => x.UserId == _user.Id))
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(GetMessagesForConversation)}: User ({_user.Id}) is not part of the conversation with id ({conversationId}), cannot access this conversation");
                return new Result<IEnumerable<ChatMessage>>(null, Domain.Enums.StatusCode.FORBIDDEN);
            }

            // If we pass in -1, get all messages.
            // If we pass in -2, get the last X messages
            // If we pass in a valid message id, then get the next X number of messages that are lower than that Id.
            // Front-end will need to invert this list to prepend it
            if (msgId == -1)
            {
                conversation.Messages = await _context.Messages.Include(x => x.ReadReceipts).Where(x => x.ConversationId == conversationId).ToListAsync();
            }
            else if (msgId == -2)
            {
                conversation.Messages = await _context.Messages.Include(x => x.ReadReceipts).Where(x => x.ConversationId == conversationId).OrderByDescending(x => x.Id).Take(NUM_MESSAGES).ToListAsync();
            }
            else
            {
                conversation.Messages = await _context.Messages.Include(x => x.ReadReceipts).Where(x => x.ConversationId == conversationId && x.Id < msgId).OrderByDescending(x => x.Id).Take(NUM_MESSAGES).ToListAsync();
            }

            foreach (var msg in conversation.Messages)
            {
                msg.Conversation = null;
            }

            return new Result<IEnumerable<ChatMessage>>(conversation.Messages, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<bool>> DeleteMessage(long messageId)
        {
            var user = await _user.GetUserByIdAsync(_user.Id);
            if (user == null)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(DeleteMessage)}: Unable to get user by name with name ({_user.Id})");
                return new Result<bool>(false, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            var message = _context.Messages.Where(x => x.Id == messageId).SingleOrDefault();
            if (message == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(DeleteMessage)}: Message with id ({messageId}) does not exist. Cannot delete it");
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
                if (userRoles != null && userRoles.Contains(DomainConstants.Authentication.ADMINISTRATOR_STRING))
                {
                    _context.Messages.Remove(message);
                }
                else
                {
                    _logger.LogInformation($"[{this.GetType().Name}] {nameof(DeleteMessage)}: User ({user.Id}) does not have permission to delete messages");
                    return new Result<bool>(false, Domain.Enums.StatusCode.FORBIDDEN);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogError($"[{this.GetType().Name}] {nameof(DeleteMessage)}: Error saving database on deleting message");
                return new Result<bool>(false, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            return new Result<bool>(true, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<ChatMessage>> GetMessage(long id)
        {
            var message = await _context.Messages.Where(x => x.Id == id).SingleOrDefaultAsync();
            if (message == null)
            {
                return new Result<ChatMessage>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            // Block bad actors attempting to access or modify information from conversations they are not part of
            var conversationUser = await _context.ConversationHasUser.Where(x => x.ConversationId == message.ConversationId && x.UserId == _user.Id).SingleOrDefaultAsync();
            if (conversationUser == null)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(GetMessage)}: User is trying to get a message in a conversation they do not have access to");
                return new Result<ChatMessage>(null, Domain.Enums.StatusCode.FORBIDDEN);
            }

            return new Result<ChatMessage>(message, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<ChatMessage>> AddMessage(ChatMessage message)
        {
            if (message == null || string.IsNullOrWhiteSpace(message.Text))
            {
                return new Result<ChatMessage>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            // Block bad actors attempting to access or modify information from conversations they are not part of
            var conversationUser = await _context.ConversationHasUser.Where(x => x.ConversationId == message.ConversationId && x.UserId == _user.Id).SingleOrDefaultAsync();
            if (conversationUser == null)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(AddMessage)}: User is trying to create a message in a conversation they do not have access to");
                return new Result<ChatMessage>(null, Domain.Enums.StatusCode.FORBIDDEN);
            }

            var newMessage = new ChatMessage()
            {
                ConversationId = message.ConversationId,
                Text = message.Text,
                UserId = _user.Id,
                Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds()
            };

            _context.Messages.Add(newMessage);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogError($"[{this.GetType().Name}] {nameof(AddMessage)}: Error saving database on adding a message");
                return new Result<ChatMessage>(null, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            await _chatHubContext.Clients.Group(newMessage.ConversationId.ToString()).SendMessage(newMessage);

            return new Result<ChatMessage>(newMessage, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<ReadReceipt>> ReadMessage(long conversationId, long messageId)
        {
            ReadReceipt receipt = null;

            // Block bad actors attempting to access or modify information from conversations they are not part of
            var conversationUser = await _context.ConversationHasUser.Where(x => x.ConversationId == conversationId && x.UserId == _user.Id).SingleOrDefaultAsync();
            if (conversationUser == null)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(ReadMessage)}: User ({_user.Id}) is not part of conversation ({conversationId}), this action is forbidden");
                return new Result<ReadReceipt>(receipt, Domain.Enums.StatusCode.FORBIDDEN);
            }

            var message = await _context.Messages.Include(x => x.ReadReceipts)
                .Where(x => x.UserId != _user.Id && x.Id == messageId &&
                    (x.ReadReceipts == null || (x.ReadReceipts != null && !x.ReadReceipts.Any(y => y.UserId == _user.Id)))
                ).SingleOrDefaultAsync();
            if (message == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(ReadMessage)}: Message ({messageId}) doesn't exist");
                return new Result<ReadReceipt>(receipt, Domain.Enums.StatusCode.NOT_FOUND);
            }
            receipt = CreateReadReceipts(_user.Id, new List<ChatMessage> { message }).FirstOrDefault();

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogError($"[{this.GetType().Name}] {nameof(ReadMessage)}: Error saving database on updating the read status of the message {messageId}");
                return new Result<ReadReceipt>(receipt, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            await _chatHubContext.Clients.Group(conversationId.ToString()).ReadMessage(receipt);

            return new Result<ReadReceipt>(receipt, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<IEnumerable<ReadReceipt>>> ReadMessageList(long conversationId, long[] messageIds)
        {
            var receipts = new List<ReadReceipt>();

            // Block bad actors attempting to access or modify information from conversations they are not part of
            var conversationUser = await _context.ConversationHasUser.Where(x => x.ConversationId == conversationId && x.UserId == _user.Id).SingleOrDefaultAsync();
            if (conversationUser == null)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {nameof(ReadMessageList)}: User ({_user.Id}) is not part of conversation ({conversationId}), this action is forbidden");
                return new Result<IEnumerable<ReadReceipt>>(receipts, Domain.Enums.StatusCode.FORBIDDEN);
            }

            var messageList = await _context.Messages.Include(x => x.ReadReceipts)
                .Where(x => x.UserId != _user.Id && messageIds.Contains(x.Id) &&
                    (x.ReadReceipts == null || (x.ReadReceipts != null && !x.ReadReceipts.Any(y => y.UserId == _user.Id)))
                ).ToListAsync();
            if (messageList == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(ReadMessageList)}: Message list with values: ({messageList}) doesn't exist");
                return new Result<IEnumerable<ReadReceipt>>(receipts, Domain.Enums.StatusCode.NOT_FOUND);
            }

            receipts.AddRange(CreateReadReceipts(_user.Id, messageList));

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogError($"[{this.GetType().Name}] {nameof(ReadMessageList)}: Error saving database on updating the read status of the message list: ({messageList})");
                return new Result<IEnumerable<ReadReceipt>>(receipts, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            await _chatHubContext.Clients.Group(conversationId.ToString()).ReadMessage(receipts.OrderByDescending(x => x.Id).FirstOrDefault());

            return new Result<IEnumerable<ReadReceipt>>(receipts, Domain.Enums.StatusCode.OK);
        }

        #endregion

        #region -> Private Functions

        private void MapConversationMetaData(Conversation conversation)
        {
            MapConversationMetaData(conversation, _user.Id);
        }

        private void MapConversationMetaData(Conversation conversation, string userName)
        {
            if (conversation == null || conversation.ConversationUsers == null || conversation.ConversationUsers.Count == 0)
            {
                return;
            }

            // 1 on 1 conversation
            if (conversation.ConversationUsers.Count == 2)
            {
                var newUserId = conversation.ConversationUsers.Where(x => x.UserId != userName).Select(x => x.UserId).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(newUserId) && _cache.GetSubDictionaryValue(CacheMap.Users, newUserId, out UserInfoCache info))
                {
                    conversation.Title = info.DisplayName;
                    conversation.Avatar = info.Avatar;
                }
            }
            else
            {
                // Group conversation that doesn't have a title
                if (string.IsNullOrWhiteSpace(conversation.Title))
                {
                    conversation.Title = string.Empty;
                    foreach (var user in conversation.ConversationUsers)
                    {
                        if (_cache.GetSubDictionaryValue(CacheMap.Users, user.UserId, out UserInfoCache info))
                        {
                            conversation.Title += info.DisplayName + ", ";
                        }
                    }
                    conversation.Title = conversation.Title.Substring(0, conversation.Title.Length - 2);
                }
            }
        }

        private async Task<IEnumerable<Conversation>> GetAllConversationAsync(string userId, GetMessageType getMessages = GetMessageType.None)
        {
            var conversationList = await _context.Conversations
                .Where(x => x.ConversationUsers.Any(y => y.ConversationId == x.Id && y.UserId == userId))
                .Select(x => new Conversation()
                {
                    ConversationUsers = x.ConversationUsers.Select(y => new ConversationHasUser()
                    {
                        ConversationId = y.ConversationId,
                        Color = y.Color,
                        Owner = y.Owner,
                        UserId = y.UserId,
                        User = y.User
                    }).ToList(),
                    Id = x.Id,
                    Title = x.Title,
                    Messages = getMessages == GetMessageType.All
                        ? _context.Messages.Where(y => y.ConversationId == x.Id).ToList()
                        : getMessages == GetMessageType.New
                            ? _context.Messages.Where(y => y.ConversationId == x.Id && y.UserId != userId
                                && (y.ReadReceipts == null || !y.ReadReceipts.Any(z => z.UserId == userId)))
                                .OrderByDescending(y => y.Timestamp).ToList()
                            : new List<ChatMessage>()
                }).ToListAsync();

            if (conversationList == null || conversationList.Count == 0)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {nameof(GetAllConversationAsync)}: Conversation list is empty for user ({userId})");
                return null;
            }

            if (getMessages == GetMessageType.New)
            {
                conversationList.ForEach(conversation =>
                {
                    conversation.UnreadMessages = conversation.Messages?.Count() ?? 0;
                });
            }

            foreach (var conversation in conversationList)
            {
                MapConversationMetaData(conversation, userId);
            }

            return conversationList;
        }

        private IEnumerable<ReadReceipt> CreateReadReceipts(string userId, IList<ChatMessage> messages)
        {
            var receipts = new List<ReadReceipt>();
            foreach (var message in messages)
            {
                receipts.Add(new ReadReceipt()
                {
                    MessageId = message.Id,
                    UserId = userId,
                    Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds()
                });
            }

            _context.ReadReceipts.AddRange(receipts);
            return receipts;
        }

        // TODO: Update to include more colors, and probably make this a lookup table
        private static string GetUserColor(int id)
        {
            return id switch
            {
                0 => "indigo",// #3F51B
                1 => "purple",// #9C27B
                2 => "teal",// #00968
                3 => "cyan",// #00BCD
                4 => "pink",// #E91E6
                5 => "green",// #4CAF5
                6 => "orange",// #FF980
                7 => "blue-grey",// #607D8
                8 => "brown",// #79554
                _ => "indigo",// #3F51B
            };
        }

        #endregion
    }
}

﻿using Microsoft.AspNetCore.SignalR;
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
using VueServer.Services.Enums;
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

        #region -> Public Functions

        public async Task<IResult<Conversation>> StartConversation(StartConversationRequest request)
        {
            if (request == null || request.Users == null || request.Users.Count() == 0)
            {
                return new Result<Conversation>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            var conversation = new Conversation();
            _context.Conversations.Add(conversation);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError("StartConversation: Error saving database on starting a conversation", e.StackTrace);
                return new Result<Conversation>(null, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            var conversationUserList = new List<ConversationHasUser>();

            // Get all this users conversations to see if they are trying to make a conversation that already exists
            var allUsersConversations = (await GetAllConversations())?.Obj;
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

            // Get current user, this shouldn't be able to fail
            var currentUser = await _user.GetUserByNameAsync(_user.Name);

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
            catch (Exception e)
            {
                _logger.LogError("StartConversation: Error saving database on adding users to a conversation", e.StackTrace);
                return new Result<Conversation>(null, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            conversation.ConversationUsers = conversationUserList;
            conversation.Messages = new List<ChatMessage>();
            return new Result<Conversation>(conversation, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<Conversation>> GetConversation(long id)
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

        public async Task<IResult<IEnumerable<Conversation>>> GetNewMessageNotifications()
        {
            var conversationList = await GetAllConversationAsync(GetMessageType.New);
            if (conversationList == null)
            {
                return new Result<IEnumerable<Conversation>>(null, Domain.Enums.StatusCode.NO_CONTENT);
            }

            return new Result<IEnumerable<Conversation>>(conversationList, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<IEnumerable<Conversation>>> GetAllConversations()
        {
            var conversationList = await GetAllConversationAsync();
            return new Result<IEnumerable<Conversation>>(conversationList, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<bool>> UpdateConversationTitle(long conversationId, string title)
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

        public async Task<IResult<bool>> DeleteConversation(long conversationId)
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

        public async Task<IResult<IEnumerable<ChatMessage>>> GetMessagesForConversation(long id)
        {
            var user = await _user.GetUserByNameAsync(_user.Name);
            if (user == null)
            {
                _logger.LogWarning($"GetMessagesForConversation: Unable to get user by name with name ({_user.Name})");
                return new Result<IEnumerable<ChatMessage>>(null, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            var conversation = await _context.Conversations.Include(x => x.ConversationUsers).Include(x => x.Messages).ThenInclude(x => x.ReadReceipts).Where(x => x.Id == id).SingleOrDefaultAsync();
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

        public async Task<IResult<bool>> DeleteMessage(long messageId)
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

        public async Task<IResult<ChatMessage>> GetMessage(long id)
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

        public async Task<IResult<ReadReceipt>> ReadMessage(long conversationId, long messageId)
        {
            ReadReceipt receipt = null;

            var user = await _user.GetUserByNameAsync(_user.Name);
            if (user == null)
            {
                _logger.LogWarning($"ReadMessage: Unable to get user by name with name ({_user.Name})");
                return new Result<ReadReceipt>(receipt, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            var conversationUser = await _context.ConversationHasUser.Where(x => x.ConversationId == conversationId && x.UserId == user.Id).FirstOrDefaultAsync();
            if (conversationUser == null)
            {
                _logger.LogInformation($"ReadMessage: User ({user.Id}) is not part of conversation ({conversationId})");
                return new Result<ReadReceipt>(receipt, Domain.Enums.StatusCode.FORBIDDEN);
            }

            var message = await _context.Messages.Include(x => x.ReadReceipts).Where(x => x.Id == messageId).SingleOrDefaultAsync();
            if (message == null)
            {
                _logger.LogInformation($"ReadMessage: Message ({message.Id}) doesn't exist");
                return new Result<ReadReceipt>(receipt, Domain.Enums.StatusCode.NOT_FOUND);
            }

            if (message.UserId == user.Id)
            {
                // Can't read your own messages
                return new Result<ReadReceipt>(receipt, Domain.Enums.StatusCode.NO_CONTENT);
            }

            if (message.ReadReceipts != null && message.ReadReceipts.Count() > 0)
            {
                var userReceipt = message.ReadReceipts.Where(x => x.UserId == user.Id).SingleOrDefault();
                if (userReceipt != null)
                {
                    // Can't read an already read messages
                    return new Result<ReadReceipt>(receipt, Domain.Enums.StatusCode.NO_CONTENT);
                }
            }

            ReadReceipt CreateReadReceipt() {
                var readReceipt = new ReadReceipt()
                {
                    MessageId = message.Id,
                    UserId = user.Id,
                    Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds()
                };

                _context.ReadReceipts.Add(readReceipt);
                return readReceipt;
            }

            
            if (message.ReadReceipts == null || message.ReadReceipts.Count() == 0)
            {
                receipt = CreateReadReceipt();
            }
            else
            {
                var oldReceipt = message.ReadReceipts.Where(x => x.Message.UserId == user.Id).SingleOrDefault();
                if (oldReceipt != null)
                {
                    return new Result<ReadReceipt>(oldReceipt, Domain.Enums.StatusCode.NO_CONTENT);
                }
                else
                {
                    receipt = CreateReadReceipt();
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogError($"ReadMessage: Error saving database on updating the read status of the message {messageId}");
                return new Result<ReadReceipt>(receipt, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            return new Result<ReadReceipt>(receipt, Domain.Enums.StatusCode.OK);
        }

        #endregion

        #region -> Private Functions

        private async Task<IEnumerable<Conversation>> GetAllConversationAsync (GetMessageType getMessages = GetMessageType.None)
        {
            var user = await _user.GetUserByNameAsync(_user.Name);
            if (user == null)
            {
                _logger.LogWarning($"GetAllConversationAsync: Unable to get user by name with name ({_user.Name})");
                return null;
            }

            IQueryable<Conversation> conversationQuery = _context.Set<Conversation>().AsQueryable();
            conversationQuery = conversationQuery.Include(x => x.ConversationUsers)
                .Where(x => x.ConversationUsers.Any(y => y.ConversationId == x.Id && y.UserId == user.Id))
                .Select(x => new Conversation()
                {
                    ConversationUsers = x.ConversationUsers.Select(y => new ConversationHasUser()
                    {
                        ConversationId = y.ConversationId,
                        Color = y.Color,
                        Owner = y.Owner,
                        UserId = y.UserId,
                        UserDisplayName = _context.Users.Where(z => z.Id == y.UserId).Select(z => z.DisplayName).SingleOrDefault()
                    }).ToList(),
                    Id = x.Id,
                    Title = x.Title
                });
            
            if (getMessages == GetMessageType.All)
            {
                conversationQuery = conversationQuery.Include(x => x.Messages).ThenInclude(x => x.ReadReceipts);
            }                
            else if (getMessages == GetMessageType.New)
            {
                conversationQuery = conversationQuery.Select(x => new Conversation()
                    {
                        ConversationUsers = x.ConversationUsers,
                        Id = x.Id,
                        Title = x.Title,
                        Messages = _context.Messages.Include(y => y.ReadReceipts).Where(y => y.ConversationId == x.Id && y.UserId != user.Id &&
                                (y.ReadReceipts == null || (y.ReadReceipts != null && !y.ReadReceipts.Any(z => z.UserId == user.Id))))
                                .OrderByDescending(y => y.Timestamp)
                });
            }

            var conversationList = await conversationQuery.ToListAsync();
            if (conversationList == null)
            {
                _logger.LogInformation($"GetAllConversationAsync: Conversation list is empty for user ({_user.Name})");
                return null;
            }

            return conversationList;
        }

        #endregion
    }
}

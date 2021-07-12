using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VueServer.Core.Objects;
using VueServer.Domain;
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

            var conversationUserList = new List<ConversationHasUser>();
            var conversation = new Conversation();
            _context.Conversations.Add(conversation);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Error saving database on starting a conversation", e.StackTrace);
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
                    _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Invalid userId: '{username}'. Cannot create a conversation with this user as they don't exist.");
                    continue;
                }

                var conversationUser = new ConversationHasUser()
                {
                    ConversationId = conversation.Id,
                    UserId = user.Id,
                    UserDisplayName = user.DisplayName,
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
                _logger.LogError($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Error saving database on adding users to a conversation", e.StackTrace);
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

            if (conversation == null)
            {
                return new Result<Conversation>(null, Domain.Enums.StatusCode.NOT_FOUND);
            }

            // Sort messages
            conversation.Messages.OrderByDescending(x => x.Timestamp);

            // TODO: This is very inneficient. Figure out a better way with LINQ
            foreach (var userConversation in conversation.ConversationUsers)
            {
                var usr = await _user.GetUserByIdAsync(userConversation.UserId);
                userConversation.UserDisplayName = usr.DisplayName;
            }

            if (conversation.ConversationUsers.Count == 2)
            {
                conversation.Title = conversation.ConversationUsers.Where(x => x.UserId != _user.Id).Select(x => x.UserDisplayName).SingleOrDefault();
            }

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
            var conversationList = await GetAllConversationAsync(_user.Id);
            return new Result<IEnumerable<Conversation>>(conversationList, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<bool>> UpdateConversationTitle(long conversationId, string title)
        {
            var conversation = (await GetConversation(conversationId))?.Obj;
            if (conversation == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Conversation with id ({conversationId}) does not exist. Cannot delete it");
                return new Result<bool>(false, Domain.Enums.StatusCode.NO_CONTENT);
            }

            var selfUser = conversation.ConversationUsers.Where(x => x.UserId == _user.Id).SingleOrDefault();
            if (selfUser == null)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Conversation with id ({conversationId}) does not include user ({_user.Id}) as one of it's members. This is likely an escalation attack");
                return new Result<bool>(false, Domain.Enums.StatusCode.FORBIDDEN);
            }

            if (!selfUser.Owner)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: User ({_user.Id}) is not the owner of this conversation ({conversationId}). They cannot change it's title.");
                return new Result<bool>(false, Domain.Enums.StatusCode.UNAUTHORIZED);
            }

            conversation.Title = title;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogError($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Error saving database on updating the conversation's title");
                return new Result<bool>(false, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            return new Result<bool>(true, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<string>> UpdateUserColor(long conversationId, string userId, int colorId)
        {
            var conversationHasUser = _context.ConversationHasUser.Where(x => x.ConversationId == conversationId && x.UserId == userId).FirstOrDefault();
            if (conversationHasUser == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: ConversationHasUser with id ({conversationId}) and ({userId}) does not exist. Cannot update the color for it");
                return new Result<string>(null, Domain.Enums.StatusCode.NO_CONTENT);
            }

            conversationHasUser.Color = GetUserColor(colorId);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogError($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Error saving database on deleting conversation");
                return new Result<string>(null, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            return new Result<string>(conversationHasUser.Color, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<bool>> DeleteConversation(long conversationId)
        {
            if (_context.UserHasFeature.Where(x => x.ModuleFeatureId == DomainConstants.Models.ModuleFeatures.Chat.DELETE_CONVERSATION_ID && x.UserId == _user.Id).SingleOrDefault() == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: User ({_user.Id}) does not have permission to delete conversations");
                return new Result<bool>(false, Domain.Enums.StatusCode.FORBIDDEN);
            }

            var conversation = (await GetConversation(conversationId))?.Obj;
            if (conversation == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Conversation with id ({conversationId}) does not exist. Cannot delete it");
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
                _logger.LogError($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Error saving database on deleting conversation");
                return new Result<bool>(false, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            return new Result<bool>(true, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<IEnumerable<ChatMessage>>> GetMessagesForConversation(long id)
        {
            var conversation = await _context.Conversations.Include(x => x.ConversationUsers).Include(x => x.Messages).ThenInclude(x => x.ReadReceipts).Where(x => x.Id == id).SingleOrDefaultAsync();
            if (conversation == null)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Unable to get conversation by id ({id})");
                return new Result<IEnumerable<ChatMessage>>(null, Domain.Enums.StatusCode.BAD_REQUEST);
            }

            if (!conversation.ConversationUsers.Any(x => x.UserId == _user.Id))
            {
                _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: User ({_user.Id}) is not part of the conversation with id ({id}), cannot access this conversation");
                return new Result<IEnumerable<ChatMessage>>(null, Domain.Enums.StatusCode.FORBIDDEN);
            }

            return new Result<IEnumerable<ChatMessage>>(conversation.Messages, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<bool>> DeleteMessage(long messageId)
        {
            var user = await _user.GetUserByIdAsync(_user.Id);
            if (user == null)
            {
                _logger.LogWarning($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Unable to get user by name with name ({_user.Id})");
                return new Result<bool>(false, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            if (_context.UserHasFeature.Where(x => x.ModuleFeatureId == DomainConstants.Models.ModuleFeatures.Chat.DELETE_MESSAGE_ID && x.UserId == user.Id).SingleOrDefault() == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: User ({user.Id}) does not have permission to delete messages");
                return new Result<bool>(false, Domain.Enums.StatusCode.FORBIDDEN);
            }

            var message = _context.Messages.Where(x => x.Id == messageId).SingleOrDefault();
            if (message == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Message with id ({messageId}) does not exist. Cannot delete it");
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
                    _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: User ({user.Id}) does not have permission to delete messages");
                    return new Result<bool>(false, Domain.Enums.StatusCode.FORBIDDEN);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogError($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Error saving database on deleting message");
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
                _logger.LogError($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Error saving database on adding a message");
                return new Result<ChatMessage>(null, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            await _chatHubContext.Clients.All.SendMessage(newMessage);

            return new Result<ChatMessage>(newMessage, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<ReadReceipt>> ReadMessage(long conversationId, long messageId)
        {
            ReadReceipt receipt = null;

            var conversationUser = await _context.ConversationHasUser.Where(x => x.ConversationId == conversationId && x.UserId == _user.Id).FirstOrDefaultAsync();
            if (conversationUser == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: User ({_user.Id}) is not part of conversation ({conversationId})");
                return new Result<ReadReceipt>(receipt, Domain.Enums.StatusCode.FORBIDDEN);
            }

            var message = await _context.Messages.Include(x => x.ReadReceipts)
                .Where(x => x.UserId != _user.Id && x.Id == messageId &&
                    (x.ReadReceipts == null || (x.ReadReceipts != null && !x.ReadReceipts.Any(y => y.UserId == _user.Id)))
                ).SingleOrDefaultAsync();
            if (message == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Message ({messageId}) doesn't exist");
                return new Result<ReadReceipt>(receipt, Domain.Enums.StatusCode.NOT_FOUND);
            }
            receipt = CreateReadReceipts(_user.Id, new List<ChatMessage> { message }).FirstOrDefault();

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogError($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Error saving database on updating the read status of the message {messageId}");
                return new Result<ReadReceipt>(receipt, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            await _chatHubContext.Clients.All.ReadMessage(receipt);

            return new Result<ReadReceipt>(receipt, Domain.Enums.StatusCode.OK);
        }

        public async Task<IResult<IEnumerable<ReadReceipt>>> ReadMessageList(long conversationId, long[] messageIds)
        {
            var receipts = new List<ReadReceipt>();

            var conversationUser = await _context.ConversationHasUser.Where(x => x.ConversationId == conversationId && x.UserId == _user.Id).FirstOrDefaultAsync();
            if (conversationUser == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: User ({_user.Id}) is not part of conversation ({conversationId})");
                return new Result<IEnumerable<ReadReceipt>>(receipts, Domain.Enums.StatusCode.FORBIDDEN);
            }

            var messageList = await _context.Messages.Include(x => x.ReadReceipts)
                .Where(x => x.UserId != _user.Id && messageIds.Contains(x.Id) &&
                    (x.ReadReceipts == null || (x.ReadReceipts != null && !x.ReadReceipts.Any(y => y.UserId == _user.Id)))
                ).ToListAsync();
            if (messageList == null)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Message list with values: ({messageList}) doesn't exist");
                return new Result<IEnumerable<ReadReceipt>>(receipts, Domain.Enums.StatusCode.NOT_FOUND);
            }

            receipts.AddRange(CreateReadReceipts(_user.Id, messageList));

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogError($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Error saving database on updating the read status of the message list: ({messageList})");
                return new Result<IEnumerable<ReadReceipt>>(receipts, Domain.Enums.StatusCode.SERVER_ERROR);
            }

            await _chatHubContext.Clients.All.ReadMessage(receipts.OrderByDescending(x => x.Id).FirstOrDefault());

            return new Result<IEnumerable<ReadReceipt>>(receipts, Domain.Enums.StatusCode.OK);
        }

        #endregion

        #region -> Private Functions

        private async Task<IEnumerable<Conversation>> GetAllConversationAsync(string userId, GetMessageType getMessages = GetMessageType.None)
        {
            IQueryable<Conversation> conversationQuery = _context.Set<Conversation>().AsQueryable();
            conversationQuery = conversationQuery.Include(x => x.ConversationUsers)
                .Where(x => x.ConversationUsers.Any(y => y.ConversationId == x.Id && y.UserId == userId))
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
                    Messages = _context.Messages.Include(y => y.ReadReceipts).Where(y => y.ConversationId == x.Id && y.UserId != userId &&
                            (y.ReadReceipts == null || (y.ReadReceipts != null && !y.ReadReceipts.Any(z => z.UserId == userId))))
                                .OrderByDescending(y => y.Timestamp)
                });
            }

            var conversationList = await conversationQuery.ToListAsync();
            if (conversationList == null || conversationList.Count == 0)
            {
                _logger.LogInformation($"[{this.GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: Conversation list is empty for user ({_user.Id})");
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
                if (conversation.ConversationUsers.Count == 2)
                {
                    conversation.Title = conversation.ConversationUsers.Where(x => x.UserId != userId).Select(x => x.UserDisplayName).SingleOrDefault();
                }
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

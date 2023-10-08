using System.Collections.Generic;
using System.Threading.Tasks;
using VueServer.Domain.Interface;
using VueServer.Modules.Chat.Models;
using VueServer.Modules.Chat.Models.Request;
using VueServer.Modules.Core.Models.Response;

namespace VueServer.Modules.Chat.Services
{
    public interface IChatService
    {
        Task<IServerResult<Conversation>> StartConversation(StartConversationRequest request);
        Task<IServerResult<Conversation>> GetConversation(long id);
        Task<IServerResult<IEnumerable<Conversation>>> GetNewMessageNotifications();
        Task<IServerResult<IEnumerable<Conversation>>> GetAllConversations();
        Task<IServerResult<IEnumerable<Conversation>>> GetAllConversationsForUser(string userName);
        Task<IServerResult<bool>> UpdateConversationTitle(long conversationId, string title);
        Task<IServerResult<string>> UpdateUserColor(long conversationId, string userId, int colorId);
        Task<IServerResult<bool>> DeleteConversation(long conversationId);
        Task<IServerResult<IEnumerable<ChatMessage>>> GetMessagesForConversation(long conversationId);
        Task<IServerResult<IEnumerable<ChatMessage>>> GetMessagesForConversation(long conversationId, long msgId);
        Task<IServerResult<bool>> DeleteMessage(long messageId);
        Task<IServerResult<ChatMessage>> GetMessage(long id);
        Task<IServerResult<ChatMessage>> AddMessage(ChatMessage message);
        Task<IServerResult<ReadReceipt>> ReadMessage(long conversationId, long messageId);
        Task<IServerResult<IEnumerable<ReadReceipt>>> ReadMessageList(long conversationId, long[] messageIds);

        Task<IServerResult<IEnumerable<WSUserResponse>>> GetActiveConversationUsers();
        Task<IServerResult<IEnumerable<WSUserResponse>>> GetUsersFromConversation(long id);
    }
}

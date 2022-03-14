using System.Collections.Generic;
using System.Threading.Tasks;
using VueServer.Domain.Interface;
using VueServer.Modules.Chat.Models;
using VueServer.Modules.Chat.Models.Request;

namespace VueServer.Modules.Chat.Services
{
    public interface IChatService
    {
        Task<IResult<Conversation>> StartConversation(StartConversationRequest request);
        Task<IResult<Conversation>> GetConversation(long id);
        Task<IResult<IEnumerable<Conversation>>> GetNewMessageNotifications();
        Task<IResult<IEnumerable<Conversation>>> GetAllConversations();
        Task<IResult<IEnumerable<Conversation>>> GetAllConversationsForUser(string userName);
        Task<IResult<bool>> UpdateConversationTitle(long conversationId, string title);
        Task<IResult<string>> UpdateUserColor(long conversationId, string userId, int colorId);
        Task<IResult<bool>> DeleteConversation(long conversationId);
        Task<IResult<IEnumerable<ChatMessage>>> GetMessagesForConversation(long conversationId);
        Task<IResult<IEnumerable<ChatMessage>>> GetMessagesForConversation(long conversationId, long msgId);
        Task<IResult<bool>> DeleteMessage(long messageId);
        Task<IResult<ChatMessage>> GetMessage(long id);
        Task<IResult<ChatMessage>> AddMessage(ChatMessage message);
        Task<IResult<ReadReceipt>> ReadMessage(long conversationId, long messageId);
        Task<IResult<IEnumerable<ReadReceipt>>> ReadMessageList(long conversationId, long[] messageIds);
    }
}

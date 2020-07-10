using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VueServer.Domain.Interface;
using VueServer.Models.Chat;
using VueServer.Models.Request;

namespace VueServer.Services.Interface
{
    public interface IChatService
    {
        Task<IResult<Conversation>> StartConversation(StartConversationRequest request);
        Task<IResult<Conversation>> GetConversation(long id);
        Task<IResult<IEnumerable<Conversation>>> GetNewMessageNotifications();
        Task<IResult<IEnumerable<Conversation>>> GetAllConversations();
        Task<IResult<bool>> UpdateConversationTitle(long conversationId, string title);
        Task<IResult<bool>> DeleteConversation(long conversationId);
        Task<IResult<IEnumerable<ChatMessage>>> GetMessagesForConversation(long id);
        Task<IResult<bool>> DeleteMessage(long messageId);
        Task<IResult<ChatMessage>> GetMessage(long id);
        Task<IResult<ChatMessage>> AddMessage(ChatMessage message);
        Task<IResult<bool>> ReadMessage(long conversationId, long messageId);
    }
}

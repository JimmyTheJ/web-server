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
        Task<IResult<Conversation>> GetConversation(Guid id);
        Task<IResult<IEnumerable<Conversation>>> GetNewMessageNotifications();
        Task<IResult<IEnumerable<Conversation>>> GetAllConversations();
        Task<IResult<bool>> UpdateConversationTitle(Guid conversationId, string title);
        Task<IResult<bool>> DeleteConversation(Guid conversationId);
        Task<IResult<IEnumerable<ChatMessage>>> GetMessagesForConversation(Guid id);
        Task<IResult<bool>> DeleteMessage(Guid messageId);
        Task<IResult<ChatMessage>> GetMessage(Guid id);
        Task<IResult<ChatMessage>> AddMessage(ChatMessage message);
    }
}

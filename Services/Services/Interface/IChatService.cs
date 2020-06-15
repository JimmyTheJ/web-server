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
        Task<IResult<IEnumerable<Conversation>>> GetAllConversations(string userId);
        Task<IResult<ChatMessage>> GetMessage(Guid id);
        Task<IResult<ChatMessage>> AddMessage(ChatMessage message);
    }
}

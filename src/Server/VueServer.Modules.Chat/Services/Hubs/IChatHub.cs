using System.Threading.Tasks;
using VueServer.Modules.Chat.Models;

namespace VueServer.Modules.Chat.Services.Hubs
{
    public interface IChatHub
    {
        Task SendMessage(ChatMessage message);
        Task ReadMessage(ReadReceipt receipt);

        /// <summary>
        /// Called from the client to Create a conversation for all users involved
        /// </summary>
        /// <param name="conversation"></param>
        /// <returns></returns>
        Task CreateConversation(Conversation conversation);
        /// <summary>
        /// Called from the Client to Delete a conversation for all users involved
        /// </summary>
        /// <param name="conversation"></param>
        /// <returns></returns>
        Task DeleteConversation(Conversation conversation);

        /// <summary>
        /// Used to inform the Client that a conversation has been created
        /// </summary>
        /// <param name="conversation"></param>
        /// <returns></returns>
        Task ConversationCreated(Conversation conversation);
        /// <summary>
        /// Used to inform the Client that a conversation has been deleted
        /// </summary>
        /// <param name="conversationId"></param>
        /// <returns></returns>
        Task ConversationDeleted(long conversationId);
    }
}

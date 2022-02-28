using System.Threading.Tasks;
using VueServer.Modules.Chat.Models;

namespace VueServer.Modules.Chat.Services.Hubs
{
    public interface IChatHub
    {
        Task SendMessage(ChatMessage message);
        Task ReadMessage(ReadReceipt receipt);
    }
}

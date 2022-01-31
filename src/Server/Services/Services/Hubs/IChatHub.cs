using System.Threading.Tasks;
using VueServer.Models.Chat;

namespace VueServer.Services.Hubs
{
    public interface IChatHub
    {
        Task SendMessage(ChatMessage message);
        Task ReadMessage(ReadReceipt receipt);
        Task GetConnectionId();
    }
}

using Microsoft.EntityFrameworkCore;
using VueServer.Modules.Chat.Models;
using VueServer.Modules.Core.Context;

namespace VueServer.Modules.Chat.Context
{
    public interface IChatContext : IWSContext
    {
        DbSet<Conversation> Conversations { get; set; }
        DbSet<ConversationHasUser> ConversationHasUser { get; set; }
        DbSet<ChatMessage> Messages { get; set; }
        DbSet<ReadReceipt> ReadReceipts { get; set; }
    }
}

using Microsoft.EntityFrameworkCore;
using VueServer.Modules.Chat.Models;
using VueServer.Modules.Core.Context;

namespace VueServer.Modules.Chat.Context
{
    public class ChatContext : WSContext, IChatContext
    {
        public ChatContext() : base() { }
        public ChatContext(DbContextOptions options) : base(options) { }

        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ConversationHasUser> ConversationHasUser { get; set; }
        public DbSet<ChatMessage> Messages { get; set; }
        public DbSet<ReadReceipt> ReadReceipts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //// Conversation User many to many setup
            modelBuilder.Entity<ConversationHasUser>().HasKey(x => new { x.ConversationId, x.UserId });
            modelBuilder.Entity<ConversationHasUser>().HasOne(x => x.Conversation).WithMany(x => x.ConversationUsers).HasForeignKey(x => x.ConversationId);
            //modelBuilder.Entity<ConversationHasUser>().HasOne(x => x.User).WithMany(x => x.ConversationUsers).HasForeignKey(x => x.UserId);
        }
    }
}

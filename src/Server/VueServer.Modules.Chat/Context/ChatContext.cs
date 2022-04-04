using Microsoft.EntityFrameworkCore;
using VueServer.Modules.Chat.Models;
using VueServer.Modules.Core.Context;
using VueServer.Modules.Core.Models.Modules;

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

            SeedModule(modelBuilder);
        }

        private void SeedModule(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ModuleAddOn>().HasData(new ModuleAddOn { Id = ChatConstants.ModuleAddOn.Id, Name = ChatConstants.ModuleAddOn.Name });

            modelBuilder.Entity<ModuleFeature>().HasData(new ModuleFeature { Id = ChatConstants.ModuleFeatures.DELETE_MESSAGE_ID, Name = ChatConstants.ModuleFeatures.DELETE_MESSAGE_NAME, ModuleAddOnId = ChatConstants.ModuleAddOn.Id });
            modelBuilder.Entity<ModuleFeature>().HasData(new ModuleFeature { Id = ChatConstants.ModuleFeatures.DELETE_CONVERSATION_ID, Name = ChatConstants.ModuleFeatures.DELETE_CONVERSATION_NAME, ModuleAddOnId = ChatConstants.ModuleAddOn.Id });
        }
    }
}

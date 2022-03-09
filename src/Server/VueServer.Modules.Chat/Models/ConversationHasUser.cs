using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VueServer.Modules.Core.Models.User;

namespace VueServer.Modules.Chat.Models
{
    public class ConversationHasUser
    {
        public long ConversationId { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }

        public bool Owner { get; set; }

        public Conversation Conversation { get; set; }

        public WSUser User { get; set; }

        [MaxLength(10)]
        public string Color { get; set; }

        // TODO: See if we can remove this
        [NotMapped]
        public string UserDisplayName { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VueServer.Models.User;

namespace VueServer.Models.Chat
{
    public class Conversation
    {
        [Key]
        public Guid Id { get; set; }

        public string Title { get; set; }

        public virtual IEnumerable<ChatMessage> Messages { get; set; }

        public virtual IList<ConversationHasUser> ConversationUsers { get; set; }
    }
}

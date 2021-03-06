﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VueServer.Models.Chat
{
    public class Conversation
    {
        [Key]
        public long Id { get; set; }

        public string Title { get; set; }

        public virtual IEnumerable<ChatMessage> Messages { get; set; }

        public virtual IList<ConversationHasUser> ConversationUsers { get; set; }

        [NotMapped]
        public int UnreadMessages { get; set; }
    }
}

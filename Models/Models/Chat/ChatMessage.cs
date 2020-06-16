using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using VueServer.Models.User;

namespace VueServer.Models.Chat
{
    public class ChatMessage
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }

        [ForeignKey("Conversation")]
        public Guid ConversationId { get; set; }

        public bool Read { get; set; }

        public string Text { get; set; }

        public long Timestamp { get; set; }

        public virtual WSUser User { get; set; }

        public virtual Conversation Conversation { get; set; }
    }
}

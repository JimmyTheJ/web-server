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
        public long Id { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }

        [ForeignKey("Conversation")]
        public long ConversationId { get; set; }

        public string Text { get; set; }

        public long Timestamp { get; set; }

        public virtual WSUser User { get; set; }

        public virtual Conversation Conversation { get; set; }

        public virtual IEnumerable<ReadReceipt> ReadReceipts { get; set; }

        [NotMapped]
        public bool Highlighted { get; set; }

        [NotMapped]
        public string Color { get; set; }

        public ChatMessage () { }

        public ChatMessage (ChatMessage message)
        {
            if (message != null)
            {
                Color = message.Color;
                ConversationId = message.ConversationId;
                Highlighted = message.Highlighted;
                Text = message.Text;
                Timestamp = message.Timestamp;
                UserId = message.UserId;
            }
        }
    }
}

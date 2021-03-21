using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using VueServer.Models.User;

namespace VueServer.Models.Chat
{
    public class ConversationHasUser
    {
        public long ConversationId { get; set; }
        public string UserId { get; set; }

        public bool Owner { get; set; }

        public Conversation Conversation { get; set; }
        public WSUser User { get; set; }

        [MaxLength(10)]
        public string Color { get; set; }

        [NotMapped]
        public string UserDisplayName { get; set; }
    }
}

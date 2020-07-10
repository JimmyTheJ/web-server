using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using VueServer.Models.User;

namespace VueServer.Models.Chat
{
    public class ChatMessageHasReadReceipt
    {
        public Guid MessageId { get; set; }

        public long ReadReceiptId { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }

        public virtual ChatMessage Message { get; set; }

        public virtual ReadReceipt ReadReceipt { get; set; }

        public virtual WSUser User { get; set; }
    }
}

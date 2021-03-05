using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Permissions;
using System.Text;
using VueServer.Models.User;

namespace VueServer.Models.Chat
{
    public class ReadReceipt
    {
        public long Id { get; set; }

        public long Timestamp { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }

        [ForeignKey("Message")]
        public long MessageId { get; set; }

        public virtual WSUser User { get; set; }

        public virtual ChatMessage Message { get; set; }

        public ReadReceipt() { }

        public ReadReceipt(ReadReceipt receipt)
        {
            if (receipt != null)
            {
                MessageId = receipt.MessageId;
                Timestamp = receipt.Timestamp;
                UserId = receipt.UserId;
            }
        }
    }
}

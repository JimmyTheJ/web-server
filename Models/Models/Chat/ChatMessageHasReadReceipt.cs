using System;
using System.Collections.Generic;
using System.Text;

namespace VueServer.Models.Chat
{
    public class ChatMessageHasReadReceipt
    {
        public Guid MessageId { get; set; }

        public long ReadReceiptId { get; set; }

        public ChatMessage Message { get; set; }

        public ReadReceipt ReadReceipt { get; set; }
    }
}

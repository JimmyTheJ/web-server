﻿using System;
using System.Collections.Generic;
using System.Text;
using VueServer.Models.User;

namespace VueServer.Models.Chat
{
    public class ConversationHasUser
    {
        public Guid ConversationId { get; set; }
        public string UserId { get; set; }

        public bool Owner { get; set; }

        public Conversation Conversation { get; set; }
        public WSUser User { get; set; }
    }
}

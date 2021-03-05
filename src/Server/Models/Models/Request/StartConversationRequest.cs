using System;
using System.Collections.Generic;
using System.Text;

namespace VueServer.Models.Request
{
    public class StartConversationRequest
    {
        public IEnumerable<string> Users { get; set; }
    }
}

using System.Collections.Generic;

namespace VueServer.Modules.Chat.Models.Request
{
    public class StartConversationRequest
    {
        public IEnumerable<string> Users { get; set; }
    }
}

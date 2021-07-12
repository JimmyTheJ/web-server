using System.Collections.Generic;

namespace VueServer.Models.Request
{
    public class StartConversationRequest
    {
        public IEnumerable<string> Users { get; set; }
    }
}

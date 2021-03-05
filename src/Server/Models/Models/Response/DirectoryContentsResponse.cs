using System.Collections.Generic;

namespace VueServer.Models
{
    public class DirectoryContentsResponse
    {
        public string Folder { get; set; }

        public IEnumerable<string> Files { get; set; }
    }
}

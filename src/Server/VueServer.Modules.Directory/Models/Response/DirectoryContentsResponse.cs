using System.Collections.Generic;

namespace VueServer.Modules.Directory.Models.Response
{
    public class DirectoryContentsResponse
    {
        public string Folder { get; set; }

        public IEnumerable<string> Files { get; set; }
    }
}

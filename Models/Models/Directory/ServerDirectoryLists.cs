using System;
using System.Collections.Generic;
using System.Text;

namespace VueServer.Models.Directory
{
    public class ServerDirectoryLists
    {
        public IList<ServerDirectory> Admin { get; set; }

        public IList<ServerDirectory> Elevated { get; set; }

        public IList<ServerDirectory> General { get; set; }
    }
}

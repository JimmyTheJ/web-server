using System;
using System.Collections.Generic;
using System.Text;

namespace VueServer.Models.Directory
{
    public class ServerDirectoryLists
    {
        /// <summary>
        /// Admin Group
        /// </summary>
        public IList<ServerDirectory> Admin { get; set; }

        /// <summary>
        /// Elevated Group
        /// </summary>
        public IList<ServerDirectory> Elevated { get; set; }

        /// <summary>
        /// General Group
        /// </summary>
        public IList<ServerDirectory> General { get; set; }

        /// <summary>
        /// User account (not group)
        /// </summary>
        public IList<ServerDirectory> User { get; set; }
    }
}

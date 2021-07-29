using System.Collections.Generic;

namespace VueServer.Models.Directory
{
    public class ServerDirectoryLists
    {
        /// <summary>
        /// Admin Group
        /// </summary>
        public IList<ServerUserDirectory> Admin { get; set; }

        /// <summary>
        /// Elevated Group
        /// </summary>
        public IList<ServerUserDirectory> Elevated { get; set; }

        /// <summary>
        /// General Group
        /// </summary>
        public IList<ServerUserDirectory> General { get; set; }

        /// <summary>
        /// User account (not group)
        /// </summary>
        public IList<ServerUserDirectory> User { get; set; }
    }
}

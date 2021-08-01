using VueServer.Models.Directory;

namespace VueServer.Models.Response
{
    public class ServerDirectory
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public bool Default { get; set; }
        public DirectoryAccessFlags AccessFlags { get; set; }
    }
}

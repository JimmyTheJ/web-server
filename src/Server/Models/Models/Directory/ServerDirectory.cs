namespace VueServer.Models.Directory
{
    public class ServerDirectory
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public bool Default { get; set; }

        public ServerDirectory()
        {

        }

        public ServerDirectory(string name, string path)
        {
            Name = name;
            Path = path;
            Default = false;
        }

        public ServerDirectory(string name, string path, bool @default)
        {
            Name = name;
            Path = path;
            Default = @default;
        }
    }
}

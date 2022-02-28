namespace VueServer.Modules.Directory.Models.Request
{
    public class CopyRequest
    {
        public FileModel Source { get; set; }
        public FileModel Destination { get; set; }
    }
}

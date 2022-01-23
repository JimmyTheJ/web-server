namespace VueServer.Models.Request
{
    public class CopyRequest
    {
        public FileModel Source { get; set; }
        public FileModel Destination { get; set; }
    }
}

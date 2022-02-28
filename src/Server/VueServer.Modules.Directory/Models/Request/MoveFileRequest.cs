namespace VueServer.Modules.Directory.Models.Request
{
    public class MoveFileRequest : FileModel
    {
        public string NewName { get; set; }
    }
}

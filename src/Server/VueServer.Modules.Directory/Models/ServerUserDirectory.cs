using System.ComponentModel.DataAnnotations;

namespace VueServer.Modules.Directory.Models
{
    public class ServerUserDirectory
    {
        [Key]
        public long Id { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public bool Default { get; set; }

        public DirectoryAccessFlags AccessFlags { get; set; }

        public string UserId { get; set; }
    }
}

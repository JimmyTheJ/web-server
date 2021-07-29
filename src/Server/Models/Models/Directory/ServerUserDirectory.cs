using System.ComponentModel.DataAnnotations;

namespace VueServer.Models.Directory
{
    public class ServerUserDirectory
    {
        [Key]
        public long Id { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public bool Default { get; set; }

        public bool AllowSubDirs { get; set; }

        public string UserId { get; set; }
    }
}

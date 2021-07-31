using System.ComponentModel.DataAnnotations;

namespace VueServer.Models.Directory
{
    public class ServerGroupDirectory
    {
        [Key]
        public int Id { get; set; }

        public string Role { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public bool AllowSubDirs { get; set; }

        public bool CanUpload { get; set; }

        public bool CanDelete { get; set; }
    }
}

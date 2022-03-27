using System.ComponentModel.DataAnnotations;

namespace VueServer.Modules.Core.Models.Modules
{
    public class RouteMeta
    {
        [Key]
        public int Id { get; set; }
        public string Display { get; set; }
        public string Relative { get; set; }
        public int AuthLevel { get; set; }
        public bool Hidden { get; set; }
    }
}

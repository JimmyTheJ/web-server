using System.ComponentModel.DataAnnotations;

namespace VueServer.Modules.Core.Models
{
    public class ServerSettings
    {
        [Key]
        public string Key { get; set; }
        public string Value { get; set; }
    }
}

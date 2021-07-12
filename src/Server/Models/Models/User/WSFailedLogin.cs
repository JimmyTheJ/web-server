using System.ComponentModel.DataAnnotations;

namespace VueServer.Models.User
{
    public class WSFailedLogin
    {
        [Key]
        public long Id { get; set; }
        [Required]
        [MaxLength(45)]
        public string IPAddress { get; set; }

        public long Timestamp { get; set; }
        [Required]
        public string Username { get; set; }
    }
}

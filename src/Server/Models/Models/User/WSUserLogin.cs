using System.ComponentModel.DataAnnotations;

namespace VueServer.Models.User
{
    public class WSUserLogin
    {
        [Key]
        public long Id { get; set; }

        [MaxLength(45)]
        public string IPAddress { get; set; }

        public string Username { get; set; }

        public bool Success { get; set; }

        public long Timestamp { get; set; }
    }
}

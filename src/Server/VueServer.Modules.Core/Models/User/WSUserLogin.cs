using System.ComponentModel.DataAnnotations;
using VueServer.Domain;

namespace VueServer.Modules.Core.Models.User
{
    public class WSUserLogin
    {
        [Key]
        public long Id { get; set; }

        [MaxLength(45)]
        public string IPAddress { get; set; }

        [MaxLength(DomainConstants.Authentication.MAX_USERNAME_LENGTH)]
        public string Username { get; set; }

        public bool Success { get; set; }

        public long Timestamp { get; set; }
    }
}

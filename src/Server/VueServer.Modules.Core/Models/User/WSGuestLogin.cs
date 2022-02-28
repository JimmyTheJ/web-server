using System.ComponentModel.DataAnnotations;

namespace VueServer.Modules.Core.Models.User
{
    public class WSGuestLogin
    {
        // PK
        [MaxLength(45)]
        public string IPAddress { get; set; }

        public long ClusterId { get; set; }

        public int FailedLogins { get; set; }

        public bool Blocked { get; set; }
    }
}

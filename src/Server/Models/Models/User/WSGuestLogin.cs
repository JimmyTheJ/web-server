using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VueServer.Models.User
{
    public class WSGuestLogin
    {
        public int ClusterId { get; set; }
        [Required]
        [MaxLength(45)]
        public string IPAddress { get; set; }

        public int FailedLogins { get; set; }
        public bool Blocked { get; set; }
    }
}

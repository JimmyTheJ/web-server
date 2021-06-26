using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

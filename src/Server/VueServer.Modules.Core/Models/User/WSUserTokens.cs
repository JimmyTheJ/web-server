using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VueServer.Modules.Core.Models.User
{
    public class WSUserTokens
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Token { get; set; }

        [MaxLength(45)]
        public string IPAddress { get; set; }

        public bool Valid { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Issued { get; set; }

        [ForeignKey("User")]
        [Required]
        public string UserId { get; set; }

        public string ClientId { get; set; }

        public WSUser User { get; set; }
    }
}

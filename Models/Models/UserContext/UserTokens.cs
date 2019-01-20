using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VueServer.Models.Account;

namespace VueServer.Models.Account
{
    public class UserTokens
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Token { get; set; }

        public string Source { get; set; }

        public bool Valid { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Issued { get; set; }

        [ForeignKey("User")]
        [Required]
        public string UserId { get; set; }

        public ServerIdentity User { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace VueServer.Models.Account
{
    public class LoginRequest
    {
        [Required]
        [MaxLength(50)]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string CodeChallenge { get; set; }

        public LoginRequest (string username, string password, string codeChallenge = null)
        {
            Username = username;
            Password = password;
            CodeChallenge = codeChallenge;
        }
    }
}

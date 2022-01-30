using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

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

        [JsonIgnore]
        public string ClientId { get; set; }

        public LoginRequest(string username, string password, string codeChallenge = null)
        {
            Username = username;
            Password = password;
            CodeChallenge = codeChallenge;
        }
    }
}

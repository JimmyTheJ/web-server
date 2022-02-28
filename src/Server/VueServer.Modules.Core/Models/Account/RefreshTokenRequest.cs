using System.ComponentModel.DataAnnotations;

namespace VueServer.Modules.Core.Models.Account
{
    public class RefreshTokenRequest
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public string CodeChallenge { get; set; }
    }
}

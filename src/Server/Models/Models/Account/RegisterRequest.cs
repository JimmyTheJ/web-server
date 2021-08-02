using System.ComponentModel.DataAnnotations;
using VueServer.Domain;

namespace VueServer.Models.Account
{
    public class RegisterRequest
    {
        [Required]
        [MaxLength(DomainConstants.Authentication.MAX_USERNAME_LENGTH), MinLength(1)]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "Passwords must match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "Role")]
        public string Role { get; set; }

        public RegisterRequest(
            string username,
            string password,
            string confirmPassword,
            string role)
        {
            Username = username;
            Password = password;
            ConfirmPassword = confirmPassword;
            Role = role;
        }
    }
}

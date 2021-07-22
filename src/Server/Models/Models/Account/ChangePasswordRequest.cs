using System.ComponentModel.DataAnnotations;

namespace VueServer.Models.Account
{
    public class ChangePasswordRequest
    {
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "Passwords must match")]
        public string ConfirmNewPassword { get; set; }

        public ChangePasswordRequest(
            string oldPassword,
            string newPassword,
            string newConfirmPassword)
        {
            OldPassword = oldPassword;
            NewPassword = newPassword;
            ConfirmNewPassword = newConfirmPassword;
        }
    }
}

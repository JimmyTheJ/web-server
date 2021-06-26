using System;
using System.ComponentModel.DataAnnotations;
using VueServer.Domain.Interface;

namespace VueServer.Models.User
{
    public class WSUserLogin : IPK<int>
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [MaxLength(45)]
        public string IpAddress { get; set; }

        [Required]
        public bool Success { get; set; }

        [Required]
        public DateTime LoginDate { get; set; }

        public WSUserLogin()
        {

        }

        public WSUserLogin(string username, string ipAddress, bool success)
        {
            Username = username;
            IpAddress = ipAddress;
            Success = success;
            LoginDate = DateTime.UtcNow;
        }
    }
}

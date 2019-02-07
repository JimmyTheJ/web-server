using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VueServer.Domain.Interface;

namespace VueServer.Models.User
{
    public class WSUser : IPK<string>
    {
        [Key]
        public string Id { get; set; }

        public string UserName { get; set; }

        public string NormalizedUserName { get; set; }

        public string DisplayName { get; set; }

        public string PasswordHash { get; set; }

        public virtual List<WSUserInRoles> Roles { get; set; }
    }
}

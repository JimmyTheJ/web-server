using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VueServer.Models.User
{
    public class WSUserInRoles
    {
        public WSUserInRoles() { }

        public WSUserInRoles(string userId, string roleId)
        {
            UserId = userId;
            RoleId = roleId;
        }

        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }

        public virtual WSUser User { get; set; }

        [ForeignKey("Role")]
        public string RoleId { get; set; }

        public virtual WSRole Role { get; set; }
    }
}

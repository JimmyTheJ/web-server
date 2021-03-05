using System.ComponentModel.DataAnnotations;
using VueServer.Domain.Interface;

namespace VueServer.Models.User
{
    public class WSRole : IPK<int>
    {
        [Key]
        public int Id { get; set; }

        public string RoleId { get; set; }

        public string Name { get; set; }

        public string NormalizedName { get; set; }
    }
}

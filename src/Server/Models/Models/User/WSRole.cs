using System.ComponentModel.DataAnnotations;
using VueServer.Domain.Interface;

namespace VueServer.Models.User
{
    public class WSRole : IPK<string>
    {
        [Key]
        public string Id { get; set; }

        public string Name { get; set; }

        public string NormalizedName { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using VueServer.Domain;
using VueServer.Domain.Interface;

namespace VueServer.Models.User
{
    public class WSRole : IPK<string>
    {
        [MaxLength(DomainConstants.Authentication.MAX_USERNAME_LENGTH), MinLength(1)]
        public string Id { get; set; }

        [JsonIgnore]
        public int ClusterId { get; set; }

        public string DisplayName { get; set; }

        [JsonIgnore]
        public string NormalizedName { get; set; }
    }
}

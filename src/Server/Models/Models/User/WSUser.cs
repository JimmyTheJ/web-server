using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VueServer.Domain;
using VueServer.Domain.Interface;
using VueServer.Models.Chat;
using VueServer.Models.Modules;

namespace VueServer.Models.User
{
    public class WSUser : IPK<string>
    {
        [MaxLength(DomainConstants.Authentication.MAX_USERNAME_LENGTH), MinLength(1)]
        public string Id { get; set; }

        [JsonIgnore]
        public long ClusterId { get; set; }

        [JsonIgnore]
        public string NormalizedUserName { get; set; }

        public string DisplayName { get; set; }

        [JsonIgnore]
        public string PasswordHash { get; set; }

        [JsonIgnore]
        public bool PasswordExpired { get; set; }

        [JsonIgnore]
        public bool Active { get; set; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public virtual IList<WSUserInRoles> Roles { get; set; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public virtual IEnumerable<UserHasModuleAddOn> UserModuleAddOns { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public virtual IEnumerable<UserHasModuleFeature> UserModuleFeatures { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public virtual IEnumerable<ConversationHasUser> ConversationUsers { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public virtual WSUserProfile UserProfile { get; set; }
    }
}

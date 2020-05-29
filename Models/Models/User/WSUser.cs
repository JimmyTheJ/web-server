using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VueServer.Domain.Interface;
using VueServer.Models.Modules;

namespace VueServer.Models.User
{
    public class WSUser : IPK<string>
    {
        [Key]
        public string Id { get; set; }

        public string UserName { get; set; }

        public string NormalizedUserName { get; set; }

        public string DisplayName { get; set; }

        [JsonIgnore]
        public string PasswordHash { get; set; }

        public virtual IList<WSUserInRoles> Roles { get; set; }

        public virtual IList<UserHasModuleAddOn> ModuleAddOns { get; set; }
        public virtual IList<UserHasModuleFeature> ModuleFeatures { get; set; }
    }
}

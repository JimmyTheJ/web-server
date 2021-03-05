using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VueServer.Models.User;

namespace VueServer.Models.Modules
{
    public class ModuleAddOn
    {
        [Key]
        public string Id { get; set; }

        public string Name { get; set; }

        public virtual IEnumerable<UserHasModuleAddOn> UserModuleAddOns { get; set; }
        public virtual IEnumerable<UserHasModuleFeature> UserModuleFeatures { get; set; }

        public virtual IList<ModuleFeature> Features { get; set; }
    }
}

﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VueServer.Modules.Core.Models.Modules
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

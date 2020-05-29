using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VueServer.Models.Modules
{
    public class ModuleFeature
    {
        [Key]
        public string Id { get; set; }

        public string Name { get; set; }

        [ForeignKey("ModuleAddOn")]
        public string ModuleAddOnId { get; set; }

        public virtual ModuleAddOn ModuleAddOn { get; set; }

        public virtual IEnumerable<UserHasModuleFeature> ModuleFeatures { get; set; }
    }
}

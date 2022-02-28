using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VueServer.Modules.Core.Models.Modules
{
    public class ModuleFeature
    {
        [Key]
        public string Id { get; set; }

        public string Name { get; set; }

        [ForeignKey("ModuleAddOn")]
        public string ModuleAddOnId { get; set; }

        public virtual ModuleAddOn ModuleAddOn { get; set; }

        public virtual IEnumerable<UserHasModuleFeature> UserModuleFeatures { get; set; }
    }
}

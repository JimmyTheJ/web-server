using VueServer.Modules.Core.Models.User;

namespace VueServer.Modules.Core.Models.Modules
{
    public class UserHasModuleAddOn
    {
        public string UserId { get; set; }

        public string ModuleAddOnId { get; set; }

        public virtual WSUser User { get; set; }

        public virtual ModuleAddOn ModuleAddOn { get; set; }
    }
}

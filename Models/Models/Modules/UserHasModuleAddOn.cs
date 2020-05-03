using System;
using System.Collections.Generic;
using System.Text;
using VueServer.Models.User;

namespace VueServer.Models.Modules
{
    public class UserHasModuleAddOn
    {
        public string UserId { get; set; }

        public string ModuleAddOnId { get; set; }

        public virtual WSUser User { get; set; }

        public virtual ModuleAddOn ModuleAddOn { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using VueServer.Models.User;

namespace VueServer.Models.Modules
{
    public class UserHasModuleFeature
    {
        public string UserId { get; set; }

        public string ModuleFeatureId { get; set; }

        public virtual WSUser User { get; set; }

        public virtual ModuleFeature ModuleFeature { get; set; }
    }
}

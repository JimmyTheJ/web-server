using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VueServer.Models.Models
{
    public class ModuleAddOn
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }


    }
}

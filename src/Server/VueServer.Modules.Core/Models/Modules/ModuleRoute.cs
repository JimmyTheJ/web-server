using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VueServer.Modules.Core.Models.Modules
{
    public class ModuleRoute
    {
        [Key]
        public int Id { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }

        [ForeignKey("Meta")]
        public int MetaId { get; set; }
        public RouteMeta Meta { get; set; }

        public IEnumerable<ModuleRoute> Children { get; set; }
    }
}

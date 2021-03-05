using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VueServer.Models
{
    public class DeleteFileModel
    {
        public string Name { get; set; }

        public string Directory { get; set; }

        public string SubDirectory { get; set; }
    }
}

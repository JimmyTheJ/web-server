using System;
using System.Collections.Generic;
using System.Text;

namespace VueServer.Models.Directory
{
    public class ServerDirectory
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public ServerDirectory()
        {

        }

        public ServerDirectory(string name, string path)
        {
            Name = name;
            Path = path;
        }
    }
}

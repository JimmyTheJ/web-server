using System;
using System.IO;

namespace VueServer.Models
{
    public class WebServerFile
    {
        public string Title { get; set; }
        public bool IsFolder { get; set; }
        public long Size { get; set; }

        public WebServerFile(FileSystemInfo fsi)
        {
            BuildFileInfo(fsi);
        }

        public void BuildFileInfo(FileSystemInfo fsi)
        {
            if (fsi == null)
            {
                Console.WriteLine($"[WebServerFile] BuildFileInfo: fsi is null");
                return;
            }

            Title = fsi.Name;

            //Console.WriteLine("Attributes: " + fsi.Attributes.ToString());

            if (fsi.Attributes.HasFlag(FileAttributes.Directory))
            {
                Size = 0;
                IsFolder = true;
            }
            else
            {
                FileInfo info = new FileInfo(fsi.FullName);
                Size = info.Length;
                IsFolder = false;
            }
        }
    }
}

using System;
using System.IO;

namespace VueServer.Models
{
    public class WebServerFile
    {
        public string Title { get; set; }
        public bool IsFolder { get; set; }
        public long Size { get; set; }
        public string Extension { get; set; }

        public WebServerFile(FileSystemInfo fsi)
        {
            BuildFileInfo(fsi);
        }

        public void BuildFileInfo(FileSystemInfo fsi)
        {
            if (fsi == null)
            {
                Console.WriteLine($"[{GetType().Name}] {System.Reflection.MethodBase.GetCurrentMethod().Name}: fsi is null");
                return;
            }

            Title = fsi.Name;

            if (fsi.Attributes.HasFlag(FileAttributes.Directory))
            {
                IsFolder = true;
            }
            else
            {
                FileInfo info = new FileInfo(fsi.FullName);
                Size = info.Length;
                Extension = info.Extension;
            }
        }
    }
}

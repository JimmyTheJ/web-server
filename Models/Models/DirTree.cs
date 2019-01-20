using System.Collections.Generic;
using System.IO;

namespace VueServer.Models
{
    public class DirTree
    {
        public string Title { get; set; }
        public bool IsFolder { get; set; }
        public string Key { get; set; }
        public long Size { get; set; }

        public List<DirTree> Children;

        public DirTree(FileSystemInfo fsi)
        {
            BuildTree(fsi);
        }

        public void BuildTree(FileSystemInfo fsi)
        {
            Title = fsi.Name;
            Children = new List<DirTree>();

            if (fsi.Attributes.HasFlag(FileAttributes.Directory))
            {
                Size = 0;
                IsFolder = true;
                foreach (FileSystemInfo f in (fsi as DirectoryInfo).GetFileSystemInfos())
                {
                    Children.Add(new DirTree(f));
                }
            }
            else
            {
                FileInfo info = new FileInfo(fsi.FullName);
                Size = info.Length;
                IsFolder = false;
            }
            Key = Title.Replace(" ", "").ToLower();
        }
    }
}

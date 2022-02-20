using System;
using System.IO;

namespace VueServer.Core.Helper
{
    public static class FolderBuilder
    {
        public static bool CreateFolder(string name, string errMsg = "")
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            if (Directory.Exists(name))
            {
                return true;
            }

            try
            {
                Directory.CreateDirectory(name);
                return true;
            }
            catch (ArgumentException)
            {
                Console.WriteLine($"{nameof(CreateFolder)}: ({errMsg}) Failed to create folder. Argument exception.");
            }
            catch (PathTooLongException)
            {
                Console.WriteLine($"{nameof(CreateFolder)}: ({errMsg}) Failed to create folder. Path too long exception.");
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine($"{nameof(CreateFolder)}: ({errMsg}) Failed to create folder. Directory not found exception.");
            }
            catch (NotSupportedException)
            {
                Console.WriteLine($"{nameof(CreateFolder)}: ({errMsg}) Failed to create folder. Not supported exception.");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"{nameof(CreateFolder)}: ({errMsg}) Failed to create folder. Unauthorized access exception.");
            }
            catch (IOException)
            {
                Console.WriteLine($"{nameof(CreateFolder)}: ({errMsg}) Failed to create folder. IO exception.");
            }

            return false;
        }
    }
}

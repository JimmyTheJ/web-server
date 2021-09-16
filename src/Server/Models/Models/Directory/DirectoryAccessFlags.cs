using System;

namespace VueServer.Models.Directory
{
    [Flags]
    public enum DirectoryAccessFlags : byte
    {
        ReadFile = 1,       // Allow downloading / viewing files
        ReadFolder = 2,     // Allow subdirectory access
        UploadFile = 4,     // Allow uploading files
        DeleteFile = 8,     // Allowing deleting files
        CreateFolder = 16,  // Allow creating folders
        DeleteFolder = 32,  // Allow deleting folders
        MoveFolder = 64,    // Allow rename/move/copy folders
        MoveFile = 128,     // Allow rename/move/copy files
    }
}

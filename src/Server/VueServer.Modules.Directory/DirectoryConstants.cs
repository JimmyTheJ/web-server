namespace VueServer.Modules.Directory
{
    internal class DirectoryConstants
    {
        internal static class Controller
        {
            internal const string BasePath = "/api/directory";
            internal const string Upload = "upload";
            internal const string LoadDirectory = "folder/{directory}/{*dir}";
            internal const string ServeMedia = "/api/serve-file/{*filename}";
            internal const string DownloadProtectedFile = "download/file/{*filename}";
            internal const string CreateFolder = "create-folder";
            internal const string RenameFile = "rename-file";
            internal const string RenameFolder = "rename-folder";
            internal const string MoveFile = "move-file";
            internal const string MoveFolder = "move-folder";
            internal const string CopyFile = "copy-file";
            internal const string CopyFolder = "copy-folder";

            internal static class Admin
            {
                internal const string AdminString = "admin/";
                internal const string GetDirectorySettings = AdminString + "settings/get";
                internal const string GetGroupDirectories = AdminString + "group/get";
                internal const string AddGroupDirectory = AdminString + "group/add";
                internal const string DeleteGroupDirectory = AdminString + "group/delete/{id}";
                internal const string GetUserDirectories = AdminString + "user/get";
                internal const string AddUserDirectory = AdminString + "user/add";
                internal const string DeleteUserDirectory = AdminString + "user/delete/{id}";
                internal const string CreateDefaultFolder = AdminString + "user/create-default-directory/{id}";
            }
        }

        internal static class ModuleAddOn
        {
            internal const string Id = "directory";
            internal const string Name = "Directory";
        }

        internal static class ModuleFeatures
        {
            internal const string DELETE_ID = "directory-delete";
            internal const string UPLOAD_ID = "directory-upload";
            internal const string VIEWER_ID = "directory-viewer";
            internal const string CREATE_ID = "directory-create";
            internal const string MOVE_ID = "directory-move";

            internal const string DELETE_NAME = "Delete";
            internal const string UPLOAD_NAME = "Upload";
            internal const string VIEWER_NAME = "Viewer";
            internal const string CREATE_NAME = "Create";
            internal const string MOVE_NAME = "Move";
        }
    }
}

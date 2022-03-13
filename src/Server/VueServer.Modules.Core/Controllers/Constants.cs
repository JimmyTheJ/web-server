namespace VueServer.Modules.Core.Controllers
{
    public static class Constants
    {
        public static class API_ENDPOINTS
        {
            public static class Generic
            {
                public const string Get = "get";
                public const string GetAll = "getall";
                public const string List = "list";
                public const string Add = "add";
                public const string Create = "create";
                public const string Edit = "edit";
                public const string Update = "update";
                public const string Delete = "delete";
            }

            public static class Admin
            {
                public const string Controller = "/api/admin";
                public const string ChangePassword = "change-password";
                public const string GetAllRoles = "roles/get-all";
                public const string SetServerSetting = "settings/set";
                public const string DeleteServerSetting = "settings/delete/{key}";
            }

            public static class Weight
            {
                public const string Controller = "/api/weight";
            }

            public static class Note
            {
                public const string Controller = "/api/note";
            }

            public static class Module
            {
                public const string Controller = "/api/modules";
                public const string GetModulesForUser = "get-modules-for-user";
                public const string GetUserModuleAndFeatures = "get-user-modules-and-features";
                public const string GetAllModules = "get-all-modules";
                public const string GetModulesForAllUsers = "get-modules-for-all-users";
                public const string AddModuleToUser = "add-module-to-user";
                public const string DeleteModuleFromUser = "delete-module-from-user";
                public const string AddFeatureToUser = "add-feature-to-user";
                public const string DeleteFeatureFromUser = "delete-feature-from-user";
            }

            public static class Library
            {
                public const string Controller = "/api/library";
                public const string Book = "book";
                public const string Author = "author";
                public const string Genre = "genre";
                public const string Bookcase = "bookcase";
                public const string Series = "series";
                public const string Shelf = "shelf";
            }

            public static class Directory
            {
                public const string Controller = "/api/directory";
                public const string Upload = "upload";
                public const string LoadDirectory = "folder/{directory}/{*dir}";
                public const string ServeMedia = "/api/serve-file/{*filename}";
                public const string DownloadProtectedFile = "download/file/{*filename}";
                public const string CreateFolder = "create-folder";
                public const string RenameFile = "rename-file";
                public const string RenameFolder = "rename-folder";
                public const string MoveFile = "move-file";
                public const string MoveFolder = "move-folder";
                public const string CopyFile = "copy-file";
                public const string CopyFolder = "copy-folder";

                public static class Admin
                {
                    public const string AdminString = "admin/";
                    public const string GetDirectorySettings = AdminString + "settings/get";
                    public const string GetGroupDirectories = AdminString + "group/get";
                    public const string AddGroupDirectory = AdminString + "group/add";
                    public const string DeleteGroupDirectory = AdminString + "group/delete/{id}";
                    public const string GetUserDirectories = AdminString + "user/get";
                    public const string AddUserDirectory = AdminString + "user/add";
                    public const string DeleteUserDirectory = AdminString + "user/delete/{id}";
                    public const string CreateDefaultFolder = AdminString + "user/create-default-folder/{id}";
                }
            }

            public static class Chat
            {
                public const string Controller = "/api/chat";
                public const string StartConversation = "conversation/start";
                public const string GetConversation = "conversation/get/{id}";
                public const string GetNewMessageNotifications = "conversation/notifications/get-all";
                public const string GetAllConversations = "conversation/get-all";
                public const string UpdateConversationTitle = "conversation/update-title/{conversationId}";
                public const string UpdateUserColor = "conversation/update-conversation-color/{conversationId}/{userId}";
                public const string DeleteConversation = "conversation/delete/{conversationId}";
                public const string GetMessagesForConversation = "conversation/get/messages/{id}";
                public const string GetPaginatedMessagesForConversation = "conversation/get/messages/{conversationId}/{msgId}";
                public const string DeleteMessage = "message/delete/{messageId}";
                public const string GetMessage = "message/get/{id}";
                public const string AddMessage = "message/send";
                public const string ReadMessage = "message/read/{conversationId}/{messageId}";
                public const string ReadMessageList = "message/read/{conversationId}/list";
            }

            public static class Account
            {
                public const string Controller = "/api/account";
                public const string Register = "register";
                public const string Login = "login";
                public const string Logout = "logout";
                public const string RefreshJwt = "refresh-jwt";
                public const string ValidateToken = "validate-tokens";

                public const string UserChangePassword = "user/change-password";
                public const string GetUserProfile = "user/profile/get/{id}";
                public const string GetAllUsers = "user/get-all";
                public const string GetAllOtherUsers = "user/get-all-others";
                public const string FuzzyUserSearch = "user/get-users-fuzzy";
                public const string UpdateAvatar = "user/update-avatar";
                public const string UpdateDisplayName = "user/update-display-name";
                public const string GetGuestLogins = "guest/logins";
                public const string UnblockGuest = "guest/unblock";
            }
        }
    }
}

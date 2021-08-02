namespace VueServer.Domain
{
    public class DomainConstants
    {
        public const string INVALID_FOLDER = "__NA__";

        public static class Authentication
        {
            public const int MAX_USERNAME_LENGTH = 64;
            public const string DEFAULT_PASSWORD = "SimplePa$$word420-69_777";

            public const string INVALID_STRING = "INVALID";

            public const string USER_STRING = "User";
            public const string ELEVATED_STRING = "Elevated";
            public const string ADMINISTRATOR_STRING = "Administrator";
            public const string ADMIN_STRING = "Admin";

            public const string ROLES_ELEVATED_USER = "Elevated, User";
            public const string ROLES_ADMIN_ELEVATED = "Administrator, Elevated";
            public const string ROLES_ALL = "Administrator, Elevated, User";
        }

        public static class Helper
        {
            public const string MIMETYPE_FOLDER = "_FOLDER";
        }

        public static class ServerSettings
        {
            public static class BaseKeys
            {
                public const string Directory = "Directory_";
            }

            public static class Directory
            {
                public const string ShouldUseDefaultPath = "ShouldUseDefaultPath";
                public const string DefaultPathValue = "DefaultPathValue";
            }
        }

        public static class Models
        {
            public static class ModuleAddOns
            {
                public static class Browser
                {
                    public const string Id = "browser";
                    public const string Name = "Browser";
                }
                public static class Chat
                {
                    public const string Id = "chat";
                    public const string Name = "Chat";
                }
                public static class Documentation
                {
                    public const string Id = "documentation";
                    public const string Name = "Documentation";
                }
                public static class Library
                {
                    public const string Id = "library";
                    public const string Name = "Library";
                }
                public static class Notes
                {
                    public const string Id = "notes";
                    public const string Name = "Notes";
                }
                public static class Weight
                {
                    public const string Id = "weight";
                    public const string Name = "Weight";
                }
            }

            public static class ModuleFeatures
            {
                public static class Browser
                {
                    public const string DELETE_ID = "browser-delete";
                    public const string UPLOAD_ID = "browser-upload";
                    public const string VIEWER_ID = "browser-viewer";

                    public const string DELETE_NAME = "Delete";
                    public const string UPLOAD_NAME = "Upload";
                    public const string VIEWER_NAME = "Viewer";
                }

                public static class Chat
                {
                    public const string DELETE_CONVERSATION_ID = "chat-delete-conversation";
                    public const string DELETE_MESSAGE_ID = "chat-delete-message";

                    public const string DELETE_CONVERSATION_NAME = "Delete Conversations";
                    public const string DELETE_MESSAGE_NAME = "Delete Messages";
                }
            }
        }
    }
}

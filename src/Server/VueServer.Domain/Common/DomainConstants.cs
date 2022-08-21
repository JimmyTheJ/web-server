namespace VueServer.Domain
{
    public class DomainConstants
    {
        public const string INVALID_FOLDER = "__NA__";
        public const string MIGRATION_ASSEMBLY = "VueServer";
        public static class Authentication
        {
            public const int MAX_USERNAME_LENGTH = 64;
            public const string DEFAULT_PASSWORD = "SimplePa$$word420-69_777";

            public const string REFRESH_TOKEN_COOKIE_KEY = "VueServer.Auth.RefreshToken";
            public const string CLIENT_COOKIE_KEY = "VueServer.ClientId";

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
                public static class Documentation
                {
                    public const string Id = "documentation";
                    public const string Name = "Documentation";
                }
                public static class Notes
                {
                    public const string Id = "notes";
                    public const string Name = "Notes";
                }
            }
        }
    }
}

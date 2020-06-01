namespace VueServer.Domain
{
    public class Constants
    {
        public const string INVALID_FOLDER = "__NA__";        

        public static class Authentication
        {
            public const string INVALID_STRING = "INVALID";

            public const string USER_STRING = "User";
            public const string ELEVATED_STRING = "Elevated";
            public const string ADMINISTRATOR_STRING = "Administrator";
            public const string ADMIN_STRING = "Admin";

            public const string ROLES_ELEVATED_USER = "Elevated, User";
            public const string ROLES_ADMIN_ELEVATED = "Administrator, Elevated";
            public const string ROLES_ALL = "Administrator, Elevated, User";
        }

        public static class Models
        {
            public static class ModuleFeatures
            {
                public const string DELETE_ID = "delete";
                public const string UPLOAD_ID = "upload";
                public const string VIEWER_ID = "viewer";
            }
        }
    }
}

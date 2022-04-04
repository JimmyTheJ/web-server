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

                public const string GetEnabledModules = "get-enabled-modules";
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

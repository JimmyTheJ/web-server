﻿namespace VueServer.Controllers
{
    internal static class Constants
    {
        internal static class API_ENDPOINTS
        {
            internal static class Generic
            {
                internal const string Get = "get";
                internal const string GetAll = "getall";
                internal const string List = "list";
                internal const string Add = "add";
                internal const string Create = "create";
                internal const string Edit = "edit";
                internal const string Update = "update";
                internal const string Delete = "delete";
            }

            internal static class Weight
            {
                internal const string Controller = "/api/weight";
            }

            internal static class Note
            {
                internal const string Controller = "/api/note";
            }

            internal static class Module
            {
                internal const string Controller = "/api/modules";
                internal const string GetModulesForUser = "get-modules-for-user";
                internal const string GetUserModuleAndFeatures = "get-user-modules-and-features";
                internal const string GetAllModules = "get-all-modules";
                internal const string GetModulesForAllUsers = "get-modules-for-all-users";
                internal const string AddModuleToUser = "add-module-to-user";
                internal const string DeleteModuleFromUser = "delete-module-from-user";
                internal const string AddFeatureToUser = "add-feature-to-user";
                internal const string DeleteFeatureFromUser = "delete-feature-from-user";
            }

            internal static class Library
            {
                internal const string Controller = "/api/library";
                internal const string Book = "book";
                internal const string Author = "author";
                internal const string Genre = "genre";
                internal const string Bookcase = "bookcase";
                internal const string Series = "series";
                internal const string Shelf = "shelf";
            }

            internal static class Directory
            {
                internal const string Controller = "/api/directory";
                internal const string Upload = "upload";
                internal const string LoadDirectory = "folder/{directory}/{*dir}";
                internal const string ServeMedia = "/api/serve-file/{*filename}";
                internal const string DownloadProtectedFile = "download/file/{*filename}";
            }

            internal static class Chat
            {
                internal const string Controller = "/api/chat";
                internal const string StartConversation = "conversation/start";
                internal const string GetConversation = "conversation/get/{id}";
                internal const string GetNewMessageNotifications = "conversation/notifications/get-all";
                internal const string GetAllConversations = "conversation/get-all";
                internal const string UpdateConversationTitle = "conversation/update-title/{conversationId}";
                internal const string UpdateUserColor = "conversation/update-conversation-color/{conversationId}/{userId}";
                internal const string DeleteConversation = "conversation/delete/{conversationId}";
                internal const string GetMessagesForConversation = "conversation/get/messages/{id}";
                internal const string DeleteMessage = "message/delete/{messageId}";
                internal const string GetMessage = "message/get/{id}";
                internal const string AddMessage = "message/send";
                internal const string ReadMessage = "message/read/{conversationId}/{messageId}";
                internal const string ReadMessageList = "message/read/{conversationId}/list";
            }

            internal static class Account
            {
                internal const string Controller = "/api/account";
                internal const string Register = "register";
                internal const string Login = "login";
                internal const string Logout = "logout";
                internal const string RefreshJwt = "refresh-jwt";
                internal const string GetAllUsers = "user/get-all";
                internal const string GetAllOtherUsers = "user/get-all-others";
                internal const string UpdateAvatar = "user/update-avatar";
                internal const string UpdateDisplayName = "user/update-display-name";
                internal const string GetGuestLogins = "guest/logins";
                internal const string UnblockGuest = "guest/unblock";
            }
        }
    }
}

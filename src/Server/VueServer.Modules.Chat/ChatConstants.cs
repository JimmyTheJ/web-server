namespace VueServer.Modules.Chat
{
    internal class ChatConstants
    {
        internal static class Controller
        {
            internal const string BasePath = "/api/chat";
            internal const string StartConversation = "conversation/start";
            internal const string GetConversation = "conversation/get/{id}";
            internal const string GetNewMessageNotifications = "conversation/notifications/get-all";
            internal const string GetAllConversations = "conversation/get-all";
            internal const string UpdateConversationTitle = "conversation/update-title/{conversationId}";
            internal const string UpdateUserColor = "conversation/update-conversation-color/{conversationId}/{userId}";
            internal const string DeleteConversation = "conversation/delete/{conversationId}";
            internal const string GetMessagesForConversation = "conversation/get/messages/{id}";
            internal const string GetPaginatedMessagesForConversation = "conversation/get/messages/{conversationId}/{msgId}";
            internal const string DeleteMessage = "message/delete/{messageId}";
            internal const string GetMessage = "message/get/{id}";
            internal const string AddMessage = "message/send";
            internal const string ReadMessage = "message/read/{conversationId}/{messageId}";
            internal const string ReadMessageList = "message/read/{conversationId}/list";

            internal const string GetActiveConversationUsers = "user/get-active";
            internal const string GetUser = "user/get/{id}";
        }

        internal static class ModuleAddOn
        {
            internal const string Id = "chat";
            internal const string Name = "Chat";
        }

        internal static class ModuleFeatures
        {
            internal const string DELETE_CONVERSATION_ID = "chat-delete-conversation";
            internal const string DELETE_MESSAGE_ID = "chat-delete-message";

            internal const string DELETE_CONVERSATION_NAME = "Delete Conversations";
            internal const string DELETE_MESSAGE_NAME = "Delete Messages";
        }
    }
}

import axios from '../axios'

const StartConversationUrl = 'api/chat/conversation/start'
const GetConversationUrl = 'api/chat/conversation/get'
const GetAllConversationsUrl = 'api/chat/conversation/get-all'
const GetAllNewMessagesForConversationsUrl = 'api/chat/conversation/notifications/get-all'
const UpdateConversationTitleUrl = 'api/chat/conversation/update-title'
const DeleteConversationUrl = `api/chat/conversation/delete`
const GetMessagesForConversation = `api/chat/conversation/get/messages`
const DeleteMessageUrl = `api/chat/message/delete`
const GetMessageUrl = 'api/chat/message/get'
const SendMessageUrl = 'api/chat/message/send'

export default {
    startConversation(obj) {
        return axios.post(StartConversationUrl, obj);
    },
    getConversation(id) {
        return axios.get(`${GetConversationUrl}/${id}`);
    },
    getAllConversations() {
        return axios.get(GetAllConversationsUrl);
    },
    getAllNewMessagesForConversations() {
        return axios.get(GetAllNewMessagesForConversationsUrl);
    },
    updateConversationTitle(conversationId, title) {
        return axios.post(`${UpdateConversationTitleUrl}/${conversationId}`, { title: title });
    },
    deleteConversation(conversationId) {
        return axios.request({
            url: `${DeleteConversationUrl}/${conversationId}`,
            method: 'delete',
        });
    },
    getMessagesForConversation(conversationId) {
        return axios.get(`${GetMessagesForConversation}/${conversationId}`);
    },
    deleteMessage(messageId) {
        return axios.request({
            url: `${DeleteMessageUrl}/${messageId}`,
            method: 'delete',
        });
    },
    getMessage(id) {
        return axios.get(`${GetMessageUrl}/${id}`);
    },
    sendMessage(obj) {
        return axios.post(SendMessageUrl, obj);
    },
}

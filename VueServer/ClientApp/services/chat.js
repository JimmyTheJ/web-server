import axios from '../axios'

const StartConversationUrl = 'api/chat/conversation/start'
const GetConversationUrl = 'api/chat/conversation/get'
const GetAllConversationsUrl = 'api/chat/conversation/get-all'
const updateConversationTitleUrl = 'api/chat/conversation/update-title'
const DeleteConversationUrl = `api/chat/conversation/delete`
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
    getAllConversations(userId) {
        return axios.get(`${GetAllConversationsUrl}/${userId}`);
    },
    updateConversationTitle(conversationId, title) {
        return axios.post(`${updateConversationTitleUrl}/${conversationId}`, { title: title });
    },
    deleteConversation(conversationId) {
        return axios.request({
            url: `${DeleteConversationUrl}/${conversationId}`,
            method: 'delete',
        });
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

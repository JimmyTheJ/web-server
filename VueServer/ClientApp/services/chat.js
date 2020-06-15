import axios from '../axios'

const StartConversationUrl = 'api/chat/conversation/start'
const GetConversationUrl = 'api/chat/conversation/get'
const GetAllConversationsUrl = 'api/chat/conversation/get-all'
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
        console.log('Getting all conversations');
        console.log(userId);
        return axios.get(`${GetAllConversationsUrl}/${userId}`);
    },
    getMessage(id) {
        return axios.get(`${GetMessageUrl}/${id}`);
    },
    sendMessage(obj) {
        return axios.post(SendMessageUrl, obj);
    },
}

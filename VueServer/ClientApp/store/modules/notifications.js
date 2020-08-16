import * as types from '../mutation_types'
import ConMsgs from '../../mixins/console';

let staticId = 1;

function createMessage(obj) {
    let message = {
        id: staticId++,
        text: '',
        type: 1,
        groupType: null,
        groupValue: null,
        read: false
    }

    if (typeof obj.text !== 'undefined')
        message.text = obj.text

    if (typeof obj.type !== 'undefined')
        message.type = obj.type

    if (typeof obj.read !== 'undefined')
        message.read = obj.read

    if (typeof obj.groupType !== 'undefined')
        message.groupType = obj.groupType

    if (typeof obj.groupValue !== 'undefined')
        message.groupValue = obj.groupValue

    return message
}

const state = {
    messages: [],
    numMessages: 0,
    numNewMessages: 0,
    opened: false
}

const getters = {

}

const actions = {
    async clearNotifications({ commit }) {
        ConMsgs.methods.$_console_log('[Vuex][Actions] Clearing notifications')

        commit(types.MESSAGE_CLEAR)
    },
    async openNotifications({ commit }, context) {
        ConMsgs.methods.$_console_log('[Vuex][Actions] Opening / closing notification message list')

        commit(types.MESSAGE_OPEN_DRAWER, context)
    },
    async readNotification({ commit }, context) {
        ConMsgs.methods.$_console_log('[Vuex][Actions] Setting message as read')

        commit(types.MESSAGE_READ, context)
    },
    async pushNotification({ commit, state }, context) {
        ConMsgs.methods.$_console_log('[Vuex][Actions] Pushing notification to message list')
        const msg = state.messages.find(x => x.groupType === context.groupType && x.groupValue === context.groupValue)
        if (typeof msg !== 'undefined' && msg.groupType !== null && msg.groupValue !== null)
            commit(types.MESSAGE_UPDATE, { newMsg: context, oldMsg: msg })
        else
            commit(types.MESSAGE_PUSH, context)
        
    },
    async popNotification({ commit }, context) {
        ConMsgs.methods.$_console_log('[Vuex][Actions] Popping notification from message list')

        commit(types.MESSAGE_READ, context)
        const item = commit(types.MESSAGE_POP, context)

        return item
    }
}

const mutations = {
    [types.MESSAGE_CLEAR](state) {
        ConMsgs.methods.$_console_log('[Vuex][Mutations] Clearing notifications')

        state.messages = []
        state.numMessages = 0
        state.numNewMessages = 0
        state.opened = false
    },
    [types.MESSAGE_OPEN_DRAWER](state) {
        ConMsgs.methods.$_console_log('[Vuex][Mutations] Open / Close message list')

        state.opened = !state.opened
    },
    [types.MESSAGE_READ](state, data) {
        ConMsgs.methods.$_console_log('[Vuex][Mutations] Setting a message as read')

        if (typeof data !== 'number') {
            ConMsgs.methods.$_console_log(`[Vuex][Mutations] Passed in value: ${data} is invalid`)
            return
        }

        const index = state.messages.findIndex(x => x.id === data)
        if (index !== -1) {
            if (state.messages[index].read === false) {
                state.messages[index].read = true
                state.numNewMessages--
            }
        }
        else {
            ConMsgs.methods.$_console_log(`[Vuex][Mutations] Couldn't find message with id ${data}`)
        }
             
    },
    [types.MESSAGE_PUSH](state, data) {
        ConMsgs.methods.$_console_log('[Vuex][Mutations] Pushing notification to message list')

        state.messages.push(createMessage({ text: data.text, type: data.type, groupType: data.groupType, groupValue: data.groupValue }))

        state.numMessages++
        state.numNewMessages++
    },
    [types.MESSAGE_UPDATE](state, data) {
        ConMsgs.methods.$_console_log('[Vuex][Mutations] Updating notification in message list')

        let oldMsgId = state.messages.findIndex(x => x.id === data.oldMsg.id)
        if (oldMsgId > -1) {
            if (state.messages[oldMsgId].read === true) {
                state.numMessages++
                state.numNewMessages++
            }

            state.messages[oldMsgId].text = data.newMsg.text
            state.messages[oldMsgId].read = false
        }        
    },
    [types.MESSAGE_POP](state, data) {
        ConMsgs.methods.$_console_log('[Vuex][Mutations] Popping notification from message list')

        if (typeof data !== 'number') {
            ConMsgs.methods.$_console_log(`[Vuex][Mutations] Passed in value: ${data} is invalid`)
            return null
        }
        else {
            const index = state.messages.findIndex(x => x.id === data)
            if (index !== -1) {
                const item = state.messages[index]

                state.messages.splice(index, 1)
                state.numMessages--

                return item
            }
            else {
                ConMsgs.methods.$_console_log(`[Vuex][Mutations] Notification with id ${data} not found.`)
            }
        }        
    }
}

export default {
    state,
    getters,
    actions,
    mutations
}

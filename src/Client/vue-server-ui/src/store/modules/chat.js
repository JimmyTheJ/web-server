import * as types from '../mutation_types'
import chatAPI from '@/services/chat'
import ConMsgs from '@/mixins/console'
import Dispatcher from '@/services/ws-dispatcher'

const state = {
  conversations: [],
  //selectedConversation: {},
}

const getters = {
  getConversationById: state => id => {
    return state.conversations.find(x => x.id === id)
  },
  getConversationIndexById: state => id => {
    return state.conversations.findIndex(x => x.id === id)
  },
  getConversationUserColor: state => (conversationId, userId) => {
    let conversation = state.conversations.find(x => x.id === conversationId)
    if (typeof conversation === 'undefined') return null

    let conversationUser = conversation.conversationUsers.find(
      x => x.userId === userId
    )
    if (typeof conversationUser === 'undefined') return null

    return conversationUser.color
  },
}

const actions = {
  clearChat({ commit }) {
    ConMsgs.methods.$_console_log('[Vuex][Actions] Clearing chat')

    commit(types.CHAT_CLEAR)
  },
  async getNewConversationNotifications({ commit }) {
    ConMsgs.methods.$_console_log(
      '[Vuex][Actions] Getting all new conversation notifications'
    )
    try {
      return Dispatcher.request(async () => {
        const res = await chatAPI.getAllNewMessagesForConversations()
        if (Array.isArray(res.data)) {
          commit(types.CHAT_CONVERSATION_UPDATE_NEW_MESSAGE_COUNT, res.data)
        }

        return await Promise.resolve(res)
      })
    } catch (e) {
      ConMsgs.methods.$_console_group(
        '[Vuex][Actions] Error from get all new conversation notifications',
        e.response
      )
      return await Promise.reject(e.response)
    }
  },
  async getAllConversationsForUser({ commit, rootState }) {
    ConMsgs.methods.$_console_log('[Vuex][Actions] Getting all conversations')

    try {
      return await Dispatcher.request(async () => {
        let res = await chatAPI.getAllConversations()
        ConMsgs.methods.$_console_log(res.data)
        commit(types.CHAT_CONVERSATION_GET_ALL, {
          list: res.data,
          userId: rootState.auth.user.id,
        })
      })
    } catch (e) {
      ConMsgs.methods.$_console_group(
        '[Vuex][Actions] Error from get all conversations',
        e.response
      )
      return await Promise.reject(e.response)
    }
  },
  async startNewConversation({ commit, rootState }, context) {
    ConMsgs.methods.$_console_log('[Vuex][Actions] Starting new conversation')
    try {
      return Dispatcher.request(async () => {
        const res = await chatAPI.startConversation(context)
        if (typeof res.data === 'object' && res.data !== null) {
          commit(types.CHAT_CONVERSATION_START_NEW, {
            obj: res.data,
            userId: rootState.auth.user.id,
          })
          return await Promise.resolve(res)
        } else {
          ConMsgs.methods.$_console_group(
            '[Vuex][Actions] Error from starting new conversation. API call succeeded but returned false.'
          )
          return await Promise.reject()
        }
      })
    } catch (e) {
      ConMsgs.methods.$_console_group(
        '[Vuex][Actions] Error from start new conversation',
        e.response
      )
      return await Promise.reject(e.response)
    }
  },
  async deleteConversation({ commit }, context) {
    ConMsgs.methods.$_console_log('[Vuex][Actions] Deleting conversation')
    try {
      return Dispatcher.request(async () => {
        const res = await chatAPI.deleteConversation(context)
        if (typeof res.data === 'boolean' && res.data === true) {
          commit(types.CHAT_CONVERSATION_DELETE, context)
          return await Promise.resolve(res)
        } else {
          ConMsgs.methods.$_console_group(
            '[Vuex][Actions] Error from deleting conversation. API call succeeded but returned false.'
          )
          return await Promise.reject()
        }
      })
    } catch (e) {
      ConMsgs.methods.$_console_group(
        '[Vuex][Actions] Error from deleting conversation',
        e.response
      )
      return await Promise.reject(e.response)
    }
  },
  async updateConversationTitle({ commit }, context) {
    ConMsgs.methods.$_console_log('[Vuex][Actions] Update conversation title')
    try {
      return Dispatcher.request(async () => {
        const res = await chatAPI.updateConversationTitle(
          context.conversationId,
          context.title
        )
        if (typeof res.data === 'boolean' && res.data === true) {
          commit(types.CHAT_CONVERSATION_UPDATE_TITLE, context)
          return await Promise.resolve(res)
        } else {
          ConMsgs.methods.$_console_group(
            '[Vuex][Actions] Error from updating conversation title. API call succeeded but returned false.'
          )
          return await Promise.reject()
        }
      })
    } catch (e) {
      ConMsgs.methods.$_console_group(
        '[Vuex][Actions] Error from updating conversation title',
        e.response
      )
      return await Promise.reject(e.response)
    }
  },
  async changeConversationUserColor({ commit }, context) {
    ConMsgs.methods.$_console_log(
      '[Vuex][Actions] Update conversation user color'
    )
    try {
      return Dispatcher.request(async () => {
        const res = await chatAPI.updateConversationUserColor(
          context.conversationId,
          context.userId,
          context.colorId
        )

        if (
          typeof res.data === 'string' &&
          res.data !== null &&
          res.data !== ''
        ) {
          commit(types.CHAT_CONVERSATION_UPDATE_USER_COLOR, {
            conversationId: context.conversationId,
            userId: context.userId,
            color: res.data,
          })
          return await Promise.resolve(res)
        } else {
          ConMsgs.methods.$_console_group(
            '[Vuex][Actions] Error from updating conversation user color. API call succeeded but returned null or empty.'
          )
          return await Promise.reject()
        }
      })
    } catch (e) {
      ConMsgs.methods.$_console_group(
        '[Vuex][Actions] Error from updating conversation user color',
        e.response
      )
      return await Promise.reject(e.response)
    }
  },
  async getMessagesForConversation({ commit }, context) {
    ConMsgs.methods.$_console_log(
      '[Vuex][Actions] Get messages for conversation'
    )
    try {
      return Dispatcher.request(async () => {
        const res = await chatAPI.getMessagesForConversation(context)
        commit(types.CHAT_CONVERSATION_GET_MESSAGES, {
          messages: res.data,
          conversationId: context,
        })
        return await Promise.resolve(res)
      })
    } catch (e) {
      ConMsgs.methods.$_console_group(
        '[Vuex][Actions] Error from getting messages for conversation',
        e.response
      )
      return await Promise.reject(e.response)
    }
  },
  async incrementConversationUnreadMessageCount({ commit }, context) {
    ConMsgs.methods.$_console_log(
      "[Vuex][Actions] Increment conversation's unread chat message count"
    )

    commit(types.CHAT_CONVERSATION_UNREAD_MESSAGES_INCREMENT, context)
  },
  async addChatMessage({ commit, rootState }, context) {
    ConMsgs.methods.$_console_log('[Vuex][Actions] Add chat message')

    commit(types.CHAT_MESSAGE_ADD, {
      ...context,
      userId: rootState.auth.user.id,
    })
  },
  async addReadReceipt({ commit, rootState }, context) {
    ConMsgs.methods.$_console_log('[Vuex][Actions] Add read receipt')

    commit(types.CHAT_MESSAGE_READ_RECEIPT_ADD, {
      ...context,
      userId: rootState.auth.user.id,
    })
  },
  async deleteChatMessage({ commit }, context) {
    ConMsgs.methods.$_console_log('[Vuex][Actions] Delete chat message')
    try {
      return Dispatcher.request(async () => {
        const res = await chatAPI.deleteMessage(context.messageId)
        if (typeof res.data === 'boolean' && res.data === true) {
          commit(types.CHAT_MESSAGE_DELETE, context)
          return await Promise.resolve(res)
        } else {
          ConMsgs.methods.$_console_group(
            '[Vuex][Actions] Error from deleting message. API call succeeded but returned false.'
          )
          return await Promise.reject()
        }
      })
    } catch (e) {
      ConMsgs.methods.$_console_group(
        '[Vuex][Actions] Error from deleting chat message',
        e.response
      )
      return await Promise.reject(e.response)
    }
  },
  async readChatMessage({ commit }, context) {
    ConMsgs.methods.$_console_log('[Vuex][Actions] Read chat message')
    try {
      return Dispatcher.request(async () => {
        const res = await chatAPI.readMessage(
          context.conversationId,
          context.messageId
        )
        commit(types.CHAT_MESSAGE_READ, {
          conversationId: context.conversationId,
          receipt: res.data,
          status: res.status,
        })
      })
    } catch (e) {
      ConMsgs.methods.$_console_group(
        '[Vuex][Actions] Error from reading chat message',
        e.response
      )
      return await Promise.reject(e.response)
    }
  },
  async readChatMessageList({ commit }, context) {
    ConMsgs.methods.$_console_log('[Vuex][Actions] Read chat message list')
    try {
      return Dispatcher.request(async () => {
        const res = await chatAPI.readMessageList(
          context.conversationId,
          context.messageIds
        )
        ConMsgs.methods.$_console_log(
          '[Vuex][Actions] Read message response: ',
          res
        )
        if (Array.isArray(res.data)) {
          res.data.forEach(element => {
            commit(types.CHAT_MESSAGE_READ, {
              conversationId: context.conversationId,
              receipt: element,
              status: res.status,
            })
          })
        }
      })
    } catch (e) {
      ConMsgs.methods.$_console_group(
        '[Vuex][Actions] Error from reading chat message list',
        e.response
      )
      return await Promise.reject(e.response)
    }
  },
  async highlightMessage({ commit }, context) {
    if (context.on) commit(types.CHAT_MESSAGE_HIGHLIGHT, context)
    else commit(types.CHAT_MESSAGE_UNHIGHLIGHT, context)
  },
  async setMessageHover({ commit }, context) {
    if (context.on) commit(types.CHAT_MESSAGE_HOVER, context)
    else commit(types.CHAT_MESSAGE_UNHOVER, context)
  },
}

const mutations = {
  [types.CHAT_CLEAR](state) {
    ConMsgs.methods.$_console_log('[Vuex][Mutations] Mutating clearing chat')

    state.conversations = []
  },
  [types.CHAT_CONVERSATION_UPDATE_NEW_MESSAGE_COUNT](state, data) {
    ConMsgs.methods.$_console_log(
      '[Vuex][Mutations] Mutating update new message count for all conversations for user'
    )

    data.forEach(element => {
      const conversationIndex = state.conversations.findIndex(
        x => x.id === element.id
      )
      if (conversationIndex >= 0) {
        state.conversations[conversationIndex].unreadMessages =
          element.unreadMessages
      }
    })
  },
  [types.CHAT_CONVERSATION_GET_ALL](state, data) {
    ConMsgs.methods.$_console_log(
      '[Vuex][Mutations] Mutating get chat conversations for user'
    )

    // Clean up
    state.conversations = []

    // Add conversations to list
    if (Array.isArray(data.list)) {
      data.list.forEach(element => {
        element.title = getConversationTitle(element, data.userId)
        state.conversations.push(element)
      })
    }
  },
  [types.CHAT_CONVERSATION_START_NEW](state, data) {
    ConMsgs.methods.$_console_log(
      '[Vuex][Mutations] Mutating start new conversation'
    )

    data.obj.title = getConversationTitle(data.obj, data.userId)
    state.conversations.push(data.obj)
  },
  [types.CHAT_CONVERSATION_DELETE](state, data) {
    ConMsgs.methods.$_console_log(
      '[Vuex][Mutations] Mutating delete conversation'
    )

    const conversationIndex = state.conversations.findIndex(x => x.id === data)
    if (conversationIndex < 0) {
      ConMsgs.methods.$_console_log(
        "[Vuex][Mutations] DeleteConversation: Can't find conversation to delete"
      )
      return
    }

    state.conversations.splice(conversationIndex, 1)
  },
  [types.CHAT_CONVERSATION_UPDATE_TITLE](state, data) {
    ConMsgs.methods.$_console_log(
      '[Vuex][Mutations] Mutating update conversation title'
    )

    let conversation = state.conversations.find(
      x => x.id === data.conversationId
    )
    if (typeof conversation === 'undefined') {
      ConMsgs.methods.$_console_log(
        "[Vuex][Mutations] UpdateConversationTitle: Can't find conversation to update the title of"
      )
      return
    }

    conversation.title = data.title
  },
  [types.CHAT_CONVERSATION_UPDATE_USER_COLOR](state, data) {
    ConMsgs.methods.$_console_log(
      '[Vuex][Mutations] Mutating update conversation user color'
    )

    let conversation = state.conversations.find(
      x => x.id === data.conversationId
    )
    if (typeof conversation === 'undefined') {
      ConMsgs.methods.$_console_log(
        "[Vuex][Mutations] UpdateConversationUserColor: Can't find conversation to update the user color of"
      )
      return
    }

    let conversationUser = conversation.conversationUsers.find(
      x => x.userId === data.userId
    )
    if (typeof conversationUser === 'undefined') {
      ConMsgs.methods.$_console_log(
        "[Vuex][Mutations] UpdateConversationUserColor: Can't find conversation user to update the user color of"
      )
      return
    }

    conversationUser.color = data.color
  },
  [types.CHAT_CONVERSATION_GET_MESSAGES](state, data) {
    ConMsgs.methods.$_console_log(
      '[Vuex][Mutations] Mutating get messages for conversation'
    )

    let conversation = state.conversations.find(
      x => x.id === data.conversationId
    )
    if (typeof conversation === 'undefined') {
      ConMsgs.methods.$_console_log(
        "[Vuex][Mutations] ConversationGetMessages: Can't find conversation to get messages for"
      )
      return
    }

    conversation.messages = data.messages
  },
  [types.CHAT_CONVERSATION_UNREAD_MESSAGES_INCREMENT](state, data) {
    ConMsgs.methods.$_console_log(
      "[Vuex][Mutations] Mutating incrementing the conversation's unread message count"
    )

    let conversation = state.conversations.find(x => x.id === data)
    if (typeof conversation === 'undefined') {
      ConMsgs.methods.$_console_log(
        "[Vuex][Mutations] ConversationUnreadMessageIncrement: Can't find conversation to increment the unread message count of"
      )
      return
    }

    conversation.unreadMessages++
  },
  [types.CHAT_MESSAGE_ADD](state, data) {
    ConMsgs.methods.$_console_log('[Vuex][Mutations] Mutating add chat message')

    const conversationIndex = state.conversations.findIndex(
      x => x.id === data.conversationId
    )
    if (conversationIndex < 0) {
      ConMsgs.methods.$_console_log(
        "[Vuex][Mutations] ChatMessageAdd: Can't find conversation to add the message to"
      )
      return
    }

    if (!Array.isArray(state.conversations[conversationIndex].messages)) {
      state.conversations[conversationIndex].messages = []
    }

    state.conversations[conversationIndex].messages.push(
      Object.assign({}, data.message)
    )
  },
  [types.CHAT_MESSAGE_READ_RECEIPT_ADD](state, data) {
    ConMsgs.methods.$_console_log('[Vuex][Mutations] Mutating add read receipt')

    const conversationIndex = state.conversations.findIndex(
      x => x.id === data.conversationId
    )
    if (conversationIndex < 0) {
      ConMsgs.methods.$_console_log(
        "[Vuex][Mutations] ReadReceiptAdd: Can't find conversation to add the message to"
      )
      return
    }

    if (!Array.isArray(state.conversations[conversationIndex].messages)) {
      ConMsgs.methods.$_console_log(
        "[Vuex][Mutations] ReadReceiptAdd: Messages isn't an array"
      )
      return
    }

    let messageIndex = state.conversations[
      conversationIndex
    ].messages.findIndex(x => x.id === data.receipt.messageId)
    if (messageIndex < 0) {
      ConMsgs.methods.$_console_log(
        '[Vuex][Mutations] ReadReceiptAdd: Messages not found in list'
      )
      return
    }

    if (
      !Array.isArray(
        state.conversations[conversationIndex].messages[messageIndex]
          .readReceipts
      )
    ) {
      state.conversations[conversationIndex].messages[
        messageIndex
      ].readReceipts = []
    }

    state.conversations[conversationIndex].messages[
      messageIndex
    ].readReceipts.push(data.receipt)
  },
  [types.CHAT_MESSAGE_DELETE](state, data) {
    ConMsgs.methods.$_console_log(
      '[Vuex][Mutations] Mutating delete chat message'
    )

    const conversationIndex = state.conversations.findIndex(
      x => x.id === data.conversationId
    )
    if (conversationIndex < 0) {
      ConMsgs.methods.$_console_log(
        "[Vuex][Mutations] ChatMessageDelete: Can't find conversation to delete the message from"
      )
      return
    }

    const messageIndex = state.conversations[
      conversationIndex
    ].messages.findIndex(x => x.id === data.messageId)
    if (messageIndex < 0) {
      ConMsgs.methods.$_console_log(
        "[Vuex][Mutations] ChatMessageDelete: Can't find message to delete"
      )
      return
    }

    state.conversations[conversationIndex].messages.splice(messageIndex, 1)
  },
  [types.CHAT_MESSAGE_READ](state, data) {
    ConMsgs.methods.$_console_log(
      '[Vuex][Mutations] Mutating read chat message'
    )

    if (data.status === 204) {
      ConMsgs.methods.$_console_log(
        '[Vuex][Mutations] ChatMessageRead: Response was null'
      )
      return
    }

    const conversationIndex = state.conversations.findIndex(
      x => x.id === data.conversationId
    )
    if (conversationIndex < 0) {
      ConMsgs.methods.$_console_log(
        "[Vuex][Mutations] ChatMessageRead: Can't find conversation to read the message from"
      )
      return
    }

    const messageIndex = state.conversations[
      conversationIndex
    ].messages.findIndex(x => x.id === data.receipt.messageId)
    if (messageIndex < 0) {
      ConMsgs.methods.$_console_log(
        "[Vuex][Mutations] ChatMessageRead: Can't find message to read"
      )
      return
    }

    if (
      !Array.isArray(
        state.conversations[conversationIndex].messages[messageIndex]
          .readReceipts
      )
    ) {
      state.conversations[conversationIndex].messages[
        messageIndex
      ].readReceipts = []
    }

    state.conversations[conversationIndex].messages[
      messageIndex
    ].readReceipts.push(data.receipt)
    state.conversations[conversationIndex].unreadMessages--
  },
  [types.CHAT_MESSAGE_HIGHLIGHT](state, data) {
    const conversationIndex = state.conversations.findIndex(
      x => x.id === data.conversationId
    )
    if (conversationIndex < 0) {
      ConMsgs.methods.$_console_log(
        "[Vuex][Mutations] ChatMessageHighlight: Can't find conversation to read the message from"
      )
      return
    }

    const messageIndex = state.conversations[
      conversationIndex
    ].messages.findIndex(x => x.id === data.messageId)
    if (messageIndex < 0) {
      ConMsgs.methods.$_console_log(
        "[Vuex][Mutations] ChatMessageHighlight: Can't find message to read"
      )
      return
    }

    state.conversations[conversationIndex].messages[
      messageIndex
    ].highlighted = true
  },
  [types.CHAT_MESSAGE_UNHIGHLIGHT](state, data) {
    const conversationIndex = state.conversations.findIndex(
      x => x.id === data.conversationId
    )
    if (conversationIndex < 0) {
      ConMsgs.methods.$_console_log(
        "[Vuex][Mutations] ChatMessageUnhighlight: Can't find conversation to read the message from"
      )
      return
    }

    const messageIndex = state.conversations[
      conversationIndex
    ].messages.findIndex(x => x.id === data.messageId)
    if (messageIndex < 0) {
      ConMsgs.methods.$_console_log(
        "[Vuex][Mutations] ChatMessageUnhighlight: Can't find message to read"
      )
      return
    }

    state.conversations[conversationIndex].messages[
      messageIndex
    ].highlighted = false
  },
  [types.CHAT_MESSAGE_HOVER](state, data) {
    const conversationIndex = state.conversations.findIndex(
      x => x.id === data.conversationId
    )
    if (conversationIndex < 0) {
      ConMsgs.methods.$_console_log(
        "[Vuex][Mutations] ChatMessageHover: Can't find conversation to read the message from"
      )
      return
    }

    const messageIndex = state.conversations[
      conversationIndex
    ].messages.findIndex(x => x.id === data.messageId)
    if (messageIndex < 0) {
      ConMsgs.methods.$_console_log(
        "[Vuex][Mutations] ChatMessageHover: Can't find message to read"
      )
      return
    }

    state.conversations[conversationIndex].messages[messageIndex].hover = true
  },
  [types.CHAT_MESSAGE_UNHOVER](state, data) {
    const conversationIndex = state.conversations.findIndex(
      x => x.id === data.conversationId
    )
    if (conversationIndex < 0) {
      ConMsgs.methods.$_console_log(
        "[Vuex][Mutations] ChatMessageUnhover: Can't find conversation to read the message from"
      )
      return
    }

    const messageIndex = state.conversations[
      conversationIndex
    ].messages.findIndex(x => x.id === data.messageId)
    if (messageIndex < 0) {
      ConMsgs.methods.$_console_log(
        "[Vuex][Mutations] ChatMessageUnhover: Can't find message to read"
      )
      return
    }

    state.conversations[conversationIndex].messages[messageIndex].hover = false
  },
}

export default {
  state,
  getters,
  actions,
  mutations,
}

function getConversationTitle(conversation, userId) {
  if (conversation === null || typeof conversation !== 'object') {
    return ''
  }

  if (
    typeof conversation.title === 'undefined' ||
    conversation.title === null ||
    conversation.title === ''
  ) {
    const relevantUsers = conversation.conversationUsers.filter(
      x => x.userId != userId
    )
    if (Array.isArray(relevantUsers) && relevantUsers.length > 0) {
      let title = ''
      for (let i = 0; i < relevantUsers.length; i++) {
        title += relevantUsers[i].userDisplayName
        if (i < relevantUsers.length - 1) {
          title += ', '
        }
      }

      return title
    }
  }

  return conversation.title
}

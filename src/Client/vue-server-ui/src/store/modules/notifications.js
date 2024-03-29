import * as types from '../mutation_types'
import ConMsgs from '@/mixins/console'
import { NotificationActions, NotificationTypes } from '@/constants'

let staticId = 1

function createMessage(obj) {
  let message = {
    id: staticId++,
    text: '',
    action: NotificationActions.Info,
    group: null,
    read: false,
  }

  if (typeof obj.text !== 'undefined') message.text = obj.text

  if (typeof obj.action !== 'undefined') message.action = obj.action

  if (typeof obj.read !== 'undefined') message.read = obj.read

  if (typeof obj.group !== 'undefined' && obj.group !== null) {
    message.group = {}
    if (typeof obj.group.type !== 'undefined')
      message.group.type = obj.group.type
    if (typeof obj.group.value !== 'undefined')
      message.group.value = obj.group.value

    message.group.num = 1
  }

  return message
}

const state = {
  messages: [],
  opened: false,
}

const getters = {
  numNewMsgs: state => state.messages.filter(x => !x.read).length,
}

const actions = {
  async clearNotificationModule({ commit }) {
    ConMsgs.methods.$_console_log('[Vuex][Actions] Clearing notifications')

    commit(types.MESSAGE_CLEAR)
  },
  clearSpecificNotification({ commit }, context) {
    ConMsgs.methods.$_console_log(
      '[Vuex][Actions] Clearing a specific notification'
    )

    commit(types.MESSAGE_SPECIFIC_CLEAR, context)
  },
  async openNotifications({ commit }, context) {
    ConMsgs.methods.$_console_log(
      '[Vuex][Actions] Opening / closing notification message list'
    )

    commit(types.MESSAGE_OPEN_DRAWER, context)
  },
  async readNotification({ commit }, context) {
    ConMsgs.methods.$_console_log('[Vuex][Actions] Setting message as read')

    commit(types.MESSAGE_READ, context)
  },
  async pushNotification({ commit, state }, context) {
    ConMsgs.methods.$_console_log(
      '[Vuex][Actions] Pushing notification to message list',
      context
    )
    if (typeof context.group !== 'undefined' && context.group !== null) {
      const msg = state.messages.find(
        x =>
          x.group !== null &&
          x.group.type === context.group.type &&
          x.group.value === context.group.value
      )
      if (
        typeof msg !== 'undefined' &&
        msg.group.type !== null &&
        msg.group.value !== null
      )
        commit(types.MESSAGE_UPDATE, { newMsg: context, oldMsg: msg })
      else commit(types.MESSAGE_PUSH, context)
    } else {
      commit(types.MESSAGE_PUSH, context)
    }
  },
  async popNotification({ commit }, context) {
    ConMsgs.methods.$_console_log(
      '[Vuex][Actions] Popping notification from message list'
    )

    commit(types.MESSAGE_READ, context)
    const item = commit(types.MESSAGE_POP, context)

    return item
  },
}

const mutations = {
  [types.MESSAGE_CLEAR](state) {
    ConMsgs.methods.$_console_log('[Vuex][Mutations] Clearing notifications')

    state.messages = []
    state.opened = false
  },
  [types.MESSAGE_SPECIFIC_CLEAR](state, data) {
    ConMsgs.methods.$_console_log(
      '[Vuex][Mutations] Clearing a specific notification'
    )

    let index = state.messages.findIndex(
      x =>
        typeof x.group !== 'undefined' &&
        x.group !== null &&
        x.group.type === data.type &&
        x.group.value === data.value
    )

    ConMsgs.methods.$_console_log('[Vuex][Mutations] Index: ' + index)
    if (index > -1) {
      state.messages[index].read = true
    }
  },
  [types.MESSAGE_OPEN_DRAWER](state) {
    ConMsgs.methods.$_console_log('[Vuex][Mutations] Open / Close message list')

    state.opened = !state.opened
  },
  [types.MESSAGE_READ](state, data) {
    ConMsgs.methods.$_console_log('[Vuex][Mutations] Setting a message as read')

    if (typeof data !== 'number') {
      ConMsgs.methods.$_console_log(
        `[Vuex][Mutations] Passed in value: ${data} is invalid`
      )
      return
    }

    const index = state.messages.findIndex(x => x.id === data)
    if (index !== -1) {
      if (state.messages[index].read === false) {
        state.messages[index].read = true

        if (state.messages[index].group !== null) {
          state.messages[index].group.num = 0
        }
      }
    } else {
      ConMsgs.methods.$_console_log(
        `[Vuex][Mutations] Couldn't find message with id ${data}`
      )
    }
  },
  [types.MESSAGE_PUSH](state, data) {
    ConMsgs.methods.$_console_log(
      '[Vuex][Mutations] Pushing notification to message list'
    )

    if (typeof data.group !== 'undefined' && data.group !== null)
      state.messages.push(
        createMessage({
          text: data.text,
          action: data.action,
          group: { type: data.group.type, value: data.group.value },
        })
      )
    else
      state.messages.push(
        createMessage({ text: data.text, action: data.action, group: null })
      )
  },
  [types.MESSAGE_UPDATE](state, data) {
    ConMsgs.methods.$_console_log(
      '[Vuex][Mutations] Updating notification in message list'
    )

    let oldMsgId = state.messages.findIndex(x => x.id === data.oldMsg.id)
    if (oldMsgId > -1) {
      state.messages[oldMsgId].text = data.newMsg.text
      state.messages[oldMsgId].read = false
      state.messages[oldMsgId].group.num++
    }
  },
  [types.MESSAGE_POP](state, data) {
    ConMsgs.methods.$_console_log(
      '[Vuex][Mutations] Popping notification from message list'
    )

    if (typeof data !== 'number') {
      ConMsgs.methods.$_console_log(
        `[Vuex][Mutations] Passed in value: ${data} is invalid`
      )
      return null
    } else {
      const index = state.messages.findIndex(x => x.id === data)
      if (index !== -1) {
        const item = state.messages[index]

        state.messages.splice(index, 1)

        return item
      } else {
        ConMsgs.methods.$_console_log(
          `[Vuex][Mutations] Notification with id ${data} not found.`
        )
      }
    }
  },
}

export default {
  state,
  getters,
  actions,
  mutations,
}

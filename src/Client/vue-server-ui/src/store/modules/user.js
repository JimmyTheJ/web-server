import * as types from '../mutation_types'
import authAPI from '@/services/auth'
import chatAPI from '@/services/chat'
import ConMsgs from '@/mixins/console'
import Dispatcher from '@/services/ws-dispatcher'

const state = {
  userMap: JSON.parse(localStorage.getItem('userMap')) || {},
}

const getters = {}

const actions = {
  clearUserModule({ commit }) {
    ConMsgs.methods.$_console_log('[Vuex][Actions] Clearing users')

    commit(types.USER_CLEAR)
  },
  async getUsersMap({ commit }, context) {
    ConMsgs.methods.$_console_log('[Vuex][Actions] Get users and add to map: ')

    try {
      return Dispatcher.request(async () => {
        const res = await chatAPI.getActiveUsersFromConversations()
        commit(types.USER_ADD_USERS_TO_MAP, res.data)

        return await Promise.resolve(res.data)
      })
    } catch (e) {
      ConMsgs.methods.$_console_group(
        '[Vuex][Actions] Error from getting user list and adding to map',
        e.response
      )
      return await Promise.reject(e.response)
    }
  },
  async addUserToMap({ commit }, context) {
    ConMsgs.methods.$_console_log('[Vuex][Actions] Add user to map: ', context)
    commit(types.USER_ADD_USER_TO_MAP, context)
  },
}

const mutations = {
  [types.USER_CLEAR](state) {
    ConMsgs.methods.$_console_log('Mutating clear user')

    localStorage.removeItem('userMap')
    Object.keys(state.userMap).forEach(key => {
      delete state.userMap[key]
    })
  },
  [types.USER_ADD_USERS_TO_MAP](state, data) {
    ConMsgs.methods.$_console_log('Mutating adding user to user map')

    data.forEach(value => {
      state.userMap[value.id.toLowerCase()] = {
        displayName: value.displayName,
        avatar: value.avatar,
      }
    })
  },
  [types.USER_ADD_USER_TO_MAP](state, data) {
    ConMsgs.methods.$_console_log('Mutating adding user to user map')

    state.userMap[data.id.toLowerCase()] = {
      displayName: data.displayName,
      avatar: data.avatar,
    }
  },
}

export default {
  state,
  getters,
  actions,
  mutations,
}

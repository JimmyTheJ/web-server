import * as types from '../mutation_types'
import authAPI from '@/services/auth'
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
  async getAllOtherUsers({ commit }) {
    ConMsgs.methods.$_console_log('[Vuex][Actions] Get all other users')
    try {
      return Dispatcher.request(async () => {
        let res = await authAPI.getAllOtherUsers()
        ConMsgs.methods.$_console_log('Got all other user data:', res.data)
        commit(types.USER_GET_OTHERS, res.data)

        return res.data
      })
    } catch (e) {
      ConMsgs.methods.$_console_group(
        '[Vuex][Actions] Error from get all other users',
        e.response
      )
      return await Promise.reject(e.response)
    }
  },
  async getAllOtherUser({ commit }) {},
  async addUserToMap({ commit }, context) {
    ConMsgs.methods.$_console_log('[Vuex][Actions] Add user to map: ', context)
    commit(types.USER_ADD_TO_MAP, context)
  },
}

const mutations = {
  [types.USER_CLEAR](state) {
    ConMsgs.methods.$_console_log('Mutating clear user')

    localStorage.removeItem('userMap')
    state.userMap = []
  },
  [types.USER_GET_OTHERS](state, data) {
    ConMsgs.methods.$_console_log('Mutating get all other users')

    state.userMap = data
    localStorage.setItem('userMap', JSON.stringify(data))
  },
  [types.USER_ADD_TO_MAP](state, data) {
    ConMsgs.methods.$_console_log('Mutating adding user to user map')

    if (
      typeof state.userMap === 'undefined' ||
      state.userMap === null ||
      state.userMap.length === 0
    ) {
      return
    }

    state.userMap[data.username.toLowerCase()] = {
      displayName: data.username,
      avatar: null,
    }
  },
}

export default {
  state,
  getters,
  actions,
  mutations,
}

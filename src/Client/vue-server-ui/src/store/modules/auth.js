import * as types from '../mutation_types'
import authAPI from '@/services/auth'
import ConMsgs from '@/mixins/console'
import Dispatcher from '@/services/ws-dispatcher'
import ChatHub from '@/plugins/chat-hub'
import { getCodeChallenge } from '@/helpers/jwt'
import { Modules } from '@/constants.js'

const state = {
  isAuthorize: Boolean(localStorage.getItem('isAuthorize')) || false,
  user: JSON.parse(localStorage.getItem('user')) || {},
  role: localStorage.getItem('userRole') || '',
  accessToken: localStorage.getItem('accessToken') || '',
}

const getters = {}

const actions = {
  async refreshToken({ commit, state }) {
    try {
      ConMsgs.methods.$_console_log('Getting refresh token')

      const res = await authAPI.refreshToken(state.accessToken)
      commit(types.JWT_TOKEN_CREATE, res.data)
      return await Promise.resolve(res.data)
    } catch (e) {
      ConMsgs.methods.$_console_group(
        '[Vuex][Actions] Error from refresh token',
        e.response
      )
      return await Promise.reject(e.response)
    }
  },
  async signin({ commit }, context) {
    try {
      ConMsgs.methods.$_console_log('Signing in')

      // Get code challenge to send to server
      context.codeChallenge = getCodeChallenge()

      const res = await authAPI.signin(context)
      commit(types.LOGIN_SUCCESS, res.data)

      return await Promise.resolve(res.data)
    } catch (e) {
      ConMsgs.methods.$_console_group(
        '[Vuex][Actions] Error from signin',
        e.response
      )
      commit(types.LOGOUT, e.response)
      return await Promise.reject(e.response)
    }
  },
  async signout({ dispatch, state, rootState }) {
    try {
      // SECTION: Module - Chat
      // Turn off the chat hub if we have the chat module enabled for this user
      if (
        rootState.module.activeModules.findIndex(x => x.id === Modules.Chat) >
        -1
      ) {
        ChatHub.stop()
      }

      const res = await authAPI.signout(state.user.id)

      // Clear all store values from other modules
      ConMsgs.methods.$_console_log(
        '[Vuex][Actions] Logout success. Clearing state.'
      )

      dispatch('clearCredentials', Modules)
      return await Promise.resolve(res)
    } catch (e) {
      ConMsgs.methods.$_console_group(
        '[Vuex][Actions] Error from signout',
        e.response
      )

      dispatch('clearCredentials', Modules)
      return await Promise.reject(e.response)
    }
  },
  async clearCredentials({ commit, dispatch }, context) {
    // Clear all store values from other modules
    ConMsgs.methods.$_console_log(
      '[Vuex][Actions] Calling clearCredentials. Clearing state.'
    )

    // SECTION: Module - All
    Object.keys(context).forEach(item => {
      const name = item.charAt(0).toUpperCase() + item.slice(1)
      dispatch(`clear${name}Module`, null, { root: true })
    })
    commit(types.LOGOUT)

    return await Promise.resolve(true)
  },
  async getRoles({ commit }) {
    ConMsgs.methods.$_console_log('[Vuex][Actions] Calling get roles: ')
    try {
      const res = await authAPI.getRoles()
      commit(types.ROLES_GET, res.data)

      return Promise.resolve(res)
    } catch (e) {
      ConMsgs.methods.$_console_group(
        '[Vuex][Actions] Error from register',
        e.response
      )
      return await Promise.reject(e.response)
    }
  },
  async register({}, context) {
    ConMsgs.methods.$_console_log('[Vuex][Actions] Calling register: ', context)
    try {
      const res = await authAPI.register(context)
      return Promise.resolve(res)
    } catch (e) {
      ConMsgs.methods.$_console_group(
        '[Vuex][Actions] Error from register',
        e.response
      )
      return await Promise.reject(e.response)
    }
  },
  async passwordChanged({ commit }, context) {
    ConMsgs.methods.$_console_log('[Vuex][Actions] Password Changed: ', context)
    try {
      const res = await authAPI.changePassword(context.data, context.isAdmin)
      if (res.data === true) {
        commit(types.CHANGED_PASSWORD)
      }

      return Promise.resolve(res)
    } catch (e) {
      ConMsgs.methods.$_console_group(
        '[Vuex][Actions] Error from password changed',
        e.response
      )
      return await Promise.reject(e.response)
    }
  },
  async updateAvatarImage({ commit }, context) {
    ConMsgs.methods.$_console_log(
      '[Vuex][Actions] Update Avatar Image: ',
      context
    )
    try {
      return Dispatcher.request(async () => {
        const res = await authAPI.uploadAvatarImage(context)
        ConMsgs.methods.$_console_log(res.data)
        commit(types.USER_UPDATE_AVATAR, res.data)
        return await Promise.resolve(res)
      })
    } catch (e) {
      ConMsgs.methods.$_console_group(
        '[Vuex][Actions] Error from update avatar image',
        e.response
      )
      return await Promise.reject(e.response)
    }
  },
  async updateDisplayName({ commit }, context) {
    ConMsgs.methods.$_console_log(
      '[Vuex][Actions] Update User Display Name: ',
      context
    )
    try {
      return Dispatcher.request(async () => {
        const res = await authAPI.updateDisplayName(context)
        ConMsgs.methods.$_console_log(res.data)
        commit(types.USER_UPDATE_DISPLAY_NAME, context)
        return await Promise.resolve(res)
      })
    } catch (e) {
      ConMsgs.methods.$_console_group(
        '[Vuex][Actions] Error from update display name',
        e.response
      )
      return await Promise.reject(e.response)
    }
  },
}

const mutations = {
  [types.JWT_TOKEN_CREATE](state, data) {
    ConMsgs.methods.$_console_log('Mutating jwt token create')
    state.accessToken = data
    localStorage.setItem('accessToken', data)
  },
  [types.JWT_TOKEN_DESTROY](state) {
    ConMsgs.methods.$_console_log('Mutating jwt token destroy')
    state.accessToken = ''
    localStorage.removeItem('accessToken')
  },
  [types.LOGIN_SUCCESS](state, data) {
    ConMsgs.methods.$_console_log('Mutating login success')
    let role = data.roles[0].toString()

    state.user = data.user
    state.role = role
    state.accessToken = data.token
    state.isAuthorize = true

    localStorage.setItem('user', JSON.stringify(data.user))
    localStorage.setItem('userRole', role)
    localStorage.setItem('accessToken', data.token)
    localStorage.setItem('isAuthorize', true)
  },
  [types.LOGOUT](state) {
    ConMsgs.methods.$_console_log('Mutating logout')

    state.user = {}
    state.role = ''
    state.accessToken = ''
    state.isAuthorize = false
    delete state.admin

    localStorage.removeItem('user')
    localStorage.removeItem('userRole')
    localStorage.removeItem('accessToken')
    localStorage.removeItem('isAuthorize')
  },
  [types.ROLES_GET](state, data) {
    if (typeof state.admin === 'undefined') state.admin = {}
    state.admin.roles = data
  },
  [types.CHANGED_PASSWORD](state) {
    state.user.changePassword = false
  },
  [types.USER_UPDATE_AVATAR](state, data) {
    ConMsgs.methods.$_console_log('Mutating update user avatar')

    state.user.avatar = data

    localStorage.removeItem('user')
    localStorage.setItem('user', JSON.stringify(state.user))
  },
  [types.USER_UPDATE_DISPLAY_NAME](state, data) {
    ConMsgs.methods.$_console_log('Mutating update user display name')

    state.user.displayName = data

    localStorage.removeItem('user')
    localStorage.setItem('user', JSON.stringify(state.user))
  },
}

export default {
  state,
  getters,
  actions,
  mutations,
}

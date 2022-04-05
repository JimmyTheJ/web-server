import * as types from '../mutation_types'
import moduleAPI from '@/services/modules'
import ConMsgs from '@/mixins/console'
import Dispatcher from '@/services/ws-dispatcher'
import { Modules } from '@/constants.js'

const state = {
  enabledModules: JSON.parse(localStorage.getItem('enabledModules')) || [],
  activeModules: JSON.parse(localStorage.getItem('activeModules')) || [],
}

const getters = {
  getActiveModules: state => state.activeModules,
}

const actions = {
  async getEnabledModules({ commit }) {
    try {
      ConMsgs.methods.$_console_log('Getting enabled modules')

      const res = await moduleAPI.getEnabledModules()
      commit(types.GET_ENABLED_MODULES, res.data)
      return await Promise.resolve(res.data)
    } catch (e) {
      ConMsgs.methods.$_console_group(
        '[Vuex][Actions] Error from getting enabled modules',
        e.response
      )
      return await Promise.reject(e.response)
    }
  },
  async getModules({ commit }) {
    ConMsgs.methods.$_console_log('[Vuex][Actions] Get Modules')
    try {
      return Dispatcher.request(async () => {
        const res = await moduleAPI.getModulesForUser()
        ConMsgs.methods.$_console_log(res.data)
        commit(types.GET_MODULES, res.data)

        return await Promise.resolve(res)
      })
    } catch (e) {
      ConMsgs.methods.$_console_group(
        '[Vuex][Actions] Error from get modules',
        e.response
      )
      return await Promise.reject(e.response)
    }
  },
}

const mutations = {
  [types.GET_ENABLED_MODULES](state, data) {
    ConMsgs.methods.$_console_log('Mutating get enabled modules')

    localStorage.removeItem('enabledModules')
    state.enabledModules = []

    // Add modules to list
    if (typeof data !== 'undefined' && data !== null && data.length > 0) {
      data.forEach(element => {
        state.enabledModules.push(element)
      })
    }

    localStorage.setItem('enabledModules', JSON.stringify(state.enabledModules))
  },
  [types.GET_MODULES](state, data) {
    ConMsgs.methods.$_console_log('Mutating get modules')

    // Clean up
    localStorage.removeItem('activeModules')
    state.activeModules = []

    // Add modules to list
    if (typeof data !== 'undefined' && data !== null && data.length > 0) {
      data.forEach(element => {
        state.activeModules.push(element)
      })
    }

    localStorage.setItem('activeModules', JSON.stringify(state.activeModules))
  },
}

export default {
  state,
  getters,
  actions,
  mutations,
}

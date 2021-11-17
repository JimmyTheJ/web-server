import * as types from '../mutation_types'
import ConMsgs from '@/mixins/console'

const state = {
  currentTime: new Date().getTime() / 1000,
}

const getters = {}

const actions = {
  getCurrentTime({ commit }) {
    //ConMsgs.methods.$_console_log('[Vuex][Actions] Updating time')

    let time = Math.trunc(new Date().getTime() / 1000)
    commit(types.GENERAL_UPDATE_TIME, time)
  },
}

const mutations = {
  [types.GENERAL_UPDATE_TIME](state, data) {
    //ConMsgs.methods.$_console_log('[Vuex][Mutations] Mutating updating time')

    state.currentTime = data
  },
}

export default {
  state,
  getters,
  actions,
  mutations,
}

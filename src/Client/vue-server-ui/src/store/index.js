import Vue from 'vue'
import Vuex from 'vuex'

/**
 * Root Scope of VUEX
 */
import * as getters from './getters'

/**
 * Module Scope of VUEX
 */
import auth from './modules/auth'
import chat from './modules/chat'
import library from './modules/library'
import notifications from './modules/notifications'
import fileExplorer from './modules/file-explorer'

Vue.use(Vuex)

/**
 * Export
 */
export default new Vuex.Store({
  getters,
  modules: {
    auth,
    chat,
    library,
    notifications,
    fileExplorer,
  },
  strict: true,
})

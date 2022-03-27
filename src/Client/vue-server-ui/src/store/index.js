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
import general from './modules/general'
import notifications from './modules/notifications'

// import chat from './modules/chat'
// import library from './modules/library'
// import fileExplorer from './modules/file-explorer'

Vue.use(Vuex)

export default new Vuex.Store({
  getters,
  modules: {
    auth,
    general,
    notifications,
  },
  strict: process.env.NODE_ENV !== 'production',
})

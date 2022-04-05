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
import module from './modules/module.js'
import notifications from './modules/notifications'

// import chat from './modules/chat'
// import library from './modules/library'
// import directory from './modules/directory'

Vue.use(Vuex)

export default new Vuex.Store({
  getters,
  modules: {
    auth,
    general,
    module,
    notifications,
  },
  strict: process.env.NODE_ENV !== 'production',
})

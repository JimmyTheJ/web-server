// src/plugins/vuetify.js

import Vue from 'vue'
import Vuetify from 'vuetify/lib'

Vue.use(Vuetify)

const opts = {
  icons: {
    iconfont: 'mdi',
  },
  theme: {
    dark: true,
    // If you wanted to override the theming
    //themes: {
    //    light: {

    //    },
    //    dark: {

    //    }
    //}
  },
}

export default new Vuetify(opts)

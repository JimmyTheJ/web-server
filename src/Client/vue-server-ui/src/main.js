// Vue stuff
import Vue from 'vue'
import VueCookie from 'vue-cookie'
import { sync } from 'vuex-router-sync'

// Material Design icons
import 'material-design-icons-iconfont/dist/material-design-icons.css'

// Font Awesome
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import '@/font-awesome'

// Vuetify
import vuetify from '@/plugins/vuetify'
import 'vuetify/dist/vuetify.min.css'

// Site css
import '@/css/site.css'

// User stuff
import ChatHub from '@/plugins/chat-hub'
import router from '@/router'
import store from '@/store'
import App from '@/App.vue'
import Moment from 'vue-moment-lib'
import ConMsgs from '@/mixins/console'

/*******************
 * Use libraries
 */
Vue.use(VueCookie)
Vue.use(Moment)

/*******************
 * Use Plugins
 */
Vue.use(ChatHub)

/*******************
 * Global components
 */
Vue.component('fa-icon', FontAwesomeIcon)

/*******************
 * Global mixins
 */
Vue.mixin(ConMsgs)

// Sync router with the Vuex store
sync(store, router)

Vue.config.productionTip = false

new Vue({
  vuetify,
  store,
  router,
  render: h => h(App),
}).$mount('#app')

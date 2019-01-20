// Vue stuff
import Vue from 'vue'
import VueCookie from 'vue-cookie'
import { sync } from 'vuex-router-sync'

// Vuetify
import Vuetify from 'vuetify'
import 'material-design-icons-iconfont'

// Font Awesome
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'

// User stuff
import axios from './axios'
import router from './router'
import store from './store'
import App from 'components/app-root'
import Moment from 'vue-moment-lib'
import ConMsgs from './mixins/console'

/*******************
 * Use libraries
 */
Vue.use(VueCookie);
Vue.use(Vuetify);
Vue.use(Moment);

/*******************
 * Global components
 */
Vue.component('fa-icon', FontAwesomeIcon);

/*******************
 * Global mixins
 */
Vue.mixin(ConMsgs);

// Sync router with the Vuex store
sync(store, router);


const app = new Vue({
    store,
    router,
    ...App
})

export {
    app,
    router,
    store
}

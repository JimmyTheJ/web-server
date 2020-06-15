// Vue stuff
import Vue from 'vue'
import VueCookie from 'vue-cookie'
import { sync } from 'vuex-router-sync'

// Vuetify
import Vuetify from 'vuetify/lib';
import vuetify from './plugins/vuetify'
import 'vuetify/dist/vuetify.min.css'

export default new Vuetify({
    icons: {
        iconfont: 'mdi',
    },
});

// Font Awesome
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'

// User stuff
import ChatHub from './plugins/chat-hub'
import axios from './axios'
import router from './router'
import store from './store'
import App from 'components/app-root'
import Moment from 'vue-moment-lib'
import ConMsgs from './mixins/console'

/*******************
 * Use libraries
 */
Vue.use(Vuetify);
Vue.use(VueCookie);
Vue.use(Moment);

/*******************
 * Use Plugins
 */
Vue.use(ChatHub);

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

Vue.config.productionTip = false

const app = new Vue({
    vuetify,
    store,
    router,
    ...App
})

export {
    app,
    router,
    store
}

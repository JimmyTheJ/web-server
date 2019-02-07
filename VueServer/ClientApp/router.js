import Vue from 'vue'
import VueRouter from 'vue-router'

import { Roles } from './constants'
import { routes } from './routes'

import store from './store'
import Auth from './mixins/authentication'
import ConMsgs from './mixins/console'

Vue.use(VueRouter);

let router = new VueRouter({
    mode: 'history',
    routes
})

router.beforeEach((to, from, next) => {
    let role = store.state.auth.role;
    let level = Auth.methods.$_auth_convertRole(role);

    if (store.state.auth.isAuthorize === true && to.meta.authLevel > level) {
        ConMsgs.methods.$_console_log('[Router] beforeEach: DENIED ACCESS');
        next('/home/start');
    }

    next();
})

export default router

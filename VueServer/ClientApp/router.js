import Vue from 'vue'
import VueRouter from 'vue-router'

import { Roles } from './constants'
import { routes } from './routes'

import store from './store'

// Mixins
import Auth from './mixins/authentication'
import Module from './mixins/module'
import ConMsgs from './mixins/console'

Vue.use(VueRouter);

let router = new VueRouter({
    mode: 'history',
    routes
})

router.beforeEach((to, from, next) => {
    let role = store.state.auth.role;
    let level = Auth.methods.$_auth_convertRole(role);

    ConMsgs.methods.$_console_log('To:', to);

    // Allow administrator access to everything
    if (level === Roles.Level.Admin) {
        next();
    }
    // Also allow access to index / login
    else if (to.name === 'index' || to.name === 'login') {
        next();
    }
    // Allow access to paths that have no access level requirement
    else if (to.meta.authLevel === Roles.Level.None) {
        next();
    }
    // User is authenticated and trying to access a path with no special permission
    else if (store.state.auth.isAuthorize === true && to.meta.authLevel <= Roles.Level.Default) {
        next();
    }
    // User is authenticated but is trying to access a path beyond their access level
    else if (store.state.auth.isAuthorize === true && to.meta.authLevel > level) {
        ConMsgs.methods.$_console_log('[Router] beforeEach: DENIED. User is not authorized or does not have sufficient access level');
        next('/home/start');
    }
    // Unauthorized path. User doesn't have access to this module
    else if (!Module.methods.$_module_userHasModule(to)) {
        ConMsgs.methods.$_console_log('[Router] beforeEach: DENIED. User does not have access to this module');
        next(false);
    }
    else {
        next();
    }
})

export default router

import * as types from '../mutation_types'
import authAPI from '../../services/auth'
import moduleAPI from '../../services/modules'
import ConMsgs from '../../mixins/console';

const state = {
    isAuthorize: Boolean(localStorage.getItem('isAuthorize')) || false,
    username: localStorage.getItem('username') || '',
    role: localStorage.getItem('userRole') || '',

    accessToken: localStorage.getItem('accessToken') || '',
    refreshToken: localStorage.getItem('refreshToken') || '',
    csrfToken: localStorage.getItem('csrfToken') || '',

    activeModules: JSON.parse(localStorage.getItem('activeModules')) || []
}

const getters = {
    getActiveModules: state => state.activeModules
}

const actions = {
    async refreshToken({ commit }) {
        try {
            ConMsgs.methods.$_console_log('Getting refresh token')
            const res = await authAPI.refreshToken(state.accessToken, state.refreshToken)
            commit(types.JWT_TOKEN_CREATE, res.data.token)
            commit(types.REFRESH_TOKEN_CREATE, res.data.refreshToken)
            return await Promise.resolve(res.data)
        } catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from refresh token', e.response)
            return await Promise.reject(e.response)
        }
    },
    async getCsrfToken({ commit } ) {
        try {
            ConMsgs.methods.$_console_log('Getting csrf token')
            const res = await authAPI.getCsrfToken()
            commit(types.CSRF_CREATE, res.data)
            return await Promise.resolve(res.data)
        } catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from getCsrfToken', e.response)
            commit(types.CSRF_DESTROY, e.response)
            return await Promise.reject(e.response)
        }
    },
    async signin({ commit }, context) {
        try {
            ConMsgs.methods.$_console_log('Signing in')
            const res = await authAPI.signin(context)
            ConMsgs.methods.$_console_log(res);
            commit(types.LOGIN_SUCCESS, res.data)
            return await Promise.resolve(res.data)
        } catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from signin', e.response)
            commit(types.LOGOUT, e.response)
            return await Promise.reject(e.response)
        }
    },
    async signout({ commit } ) {
        try {
            const res = await authAPI.signout()
            commit(types.LOGOUT)
            return await Promise.resolve(res)
        } catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from signout', e.response)
            commit(types.LOGOUT)
            return await Promise.reject(e.response);
        }
    },
    async register({ commit }, context) {
        ConMsgs.methods.$_console_log(context)
        try {
            const res = await authAPI.register(context)
            return await Promise.resolve(res)
        }
        catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from register', e.response)
            return await Promise.reject(e.response);
        }
    },
    async getModules({ commit }) {
        try {
            const res = await moduleAPI.getModulesForUser()
            ConMsgs.methods.$_console_log(res.data);
            commit(types.GET_MODULES, res.data)
            return await Promise.resolve(res)
        }
        catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from get modules', e.response)
            return await Promise.reject(e.response);
        }
    }
}

const mutations = {
    [types.JWT_TOKEN_CREATE](state, data) {
        ConMsgs.methods.$_console_log("Mutating jwt token create")
        state.accessToken = data
        localStorage.setItem('accessToken', data)
    },
    [types.JWT_TOKEN_DESTROY](state) {
        ConMsgs.methods.$_console_log("Mutating jwt token destroy")
        state.accessToken = ''
        localStorage.removeItem('accessToken')
    },
    [types.REFRESH_TOKEN_CREATE](state, data) {
        ConMsgs.methods.$_console_log("Mutating refresh token create")
        state.refreshToken = data
        localStorage.setItem('refreshToken', data)
    },
    [types.REFRESH_TOKEN_DESTROY](state) {
        ConMsgs.methods.$_console_log("Mutating refresh token destroy")
        state.refreshToken = ''
        localStorage.removeItem('refreshToken')
    },
    [types.CSRF_CREATE](state, data) {
        ConMsgs.methods.$_console_log("Mutating CSRF create")
        state.csrfToken = data
        localStorage.setItem('csrfToken', data)
    },
    [types.CSRF_DESTROY](state) {
        ConMsgs.methods.$_console_log("Mutating CSRF destroy")
        state.csrfToken = ''
        localStorage.removeItem('csrfToken')
    },
    [types.LOGIN_SUCCESS](state, data) {
        ConMsgs.methods.$_console_log("Mutating login success")
        let role = data.roles[0].toString()

        state.username = data.username
        state.role = role
        state.accessToken = data.token
        state.refreshToken = data.refreshToken
        state.csrfToken = data.csrfToken
        state.isAuthorize = true

        localStorage.setItem('username', data.username)
        localStorage.setItem('userRole', role)
        localStorage.setItem('accessToken', data.token)
        localStorage.setItem('refreshToken', data.refreshToken)
        localStorage.setItem('csrfToken', data.csrfToken)
        localStorage.setItem('isAuthorize', true)
    },
    [types.LOGOUT](state) {
        ConMsgs.methods.$_console_log("Mutating logout");

        state.username = ''
        state.role = ''
        state.accessToken = ''
        state.refreshToken = ''
        state.csrfToken = ''
        state.isAuthorize = false

        localStorage.removeItem('username')
        localStorage.removeItem('userRole')
        localStorage.removeItem('accessToken')
        localStorage.removeItem('refreshToken')
        localStorage.removeItem('csrfToken')
        localStorage.removeItem('isAuthorize')
    },
    [types.GET_MODULES](state, data) {
        ConMsgs.methods.$_console_log("Mutating get modules");

        // Clean up
        localStorage.removeItem('activeModules')
        state.activeModules = [];

        // Add modules to list
        if (typeof data !== 'undefined' && data !== null && data.length > 0) {
            data.forEach(element => { state.activeModules.push(element) });
        }

        localStorage.setItem('activeModules', JSON.stringify(state.activeModules))
    }
}

export default {
    state,
    getters,
    actions,
    mutations
}

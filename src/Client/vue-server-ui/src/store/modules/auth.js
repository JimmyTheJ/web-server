import * as types from '../mutation_types'
import authAPI from '../../services/auth'
import moduleAPI from '../../services/modules'
import ConMsgs from '../../mixins/console';
import DispatchFactory from '../../factories/dispatchFactory'
import { getCodeChallenge } from '../../helpers/jwt'

const state = {
    isAuthorize: Boolean(localStorage.getItem('isAuthorize')) || false,
    user: JSON.parse(localStorage.getItem('user')) || { },
    role: localStorage.getItem('userRole') || '',

    accessToken: localStorage.getItem('accessToken') || '',
    codeChallenge: localStorage.getItem('codeChallenge') || '',

    activeModules: JSON.parse(localStorage.getItem('activeModules')) || [],
    otherUsers: JSON.parse(localStorage.getItem('otherUsers')) || [],
}

const getters = {
    getActiveModules: state => state.activeModules
}

const actions = {
    async refreshToken({ commit, state }) {
        try {
            ConMsgs.methods.$_console_log('Getting refresh token')

            const res = await authAPI.refreshToken(state.accessToken, state.codeChallenge)
            commit(types.JWT_TOKEN_CREATE, res.data)
            return await Promise.resolve(res.data)
        } catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from refresh token', e.response)
            return await Promise.reject(e.response)
        }
    },
    async signin({ commit }, context) {
        try {
            ConMsgs.methods.$_console_log('Signing in')

            // Get code challenge to send to server
            let challenge = getCodeChallenge()
            commit(types.CODE_CHALLENGE_CREATE, challenge)
            context.codeChallenge = challenge

            const res = await authAPI.signin(context)
            commit(types.LOGIN_SUCCESS, res.data)
            return await Promise.resolve(res.data)
        } catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from signin', e.response)
            commit(types.LOGOUT, e.response)
            return await Promise.reject(e.response)
        }
    },
    async signout({ commit, dispatch } ) {
        try {
            const res = await authAPI.signout(state.user.id)
            commit(types.LOGOUT)

            // Clear all store values from other modules
            ConMsgs.methods.$_console_log("[Vuex][Actions] Logout success. Clearing state.")
            dispatch('clearChat')
            dispatch('clearNotifications');
            dispatch('clearLibrary');
            dispatch('clearFileExplorer');

            return await Promise.resolve(res)
        } catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from signout', e.response)
            commit(types.LOGOUT)
            return await Promise.reject(e.response);
        }
    },
    async register(context) {
        ConMsgs.methods.$_console_log('[Vuex][Actions] Calling register: ', context)
        try {
            const res = await authAPI.register(context)
            return Promise.resolve(res)
        }
        catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from register', e.response)
            return await Promise.reject(e.response);
        }
    },
    async getModules({ commit }) {
        try {
            return DispatchFactory.request(async () => {
                const res = await moduleAPI.getModulesForUser()
                ConMsgs.methods.$_console_log(res.data);
                commit(types.GET_MODULES, res.data)
                return await Promise.resolve(res)
            })
        }
        catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from get modules', e.response)
            return await Promise.reject(e.response);
        }
    },
    async updateAvatarImage({ commit }, context) {
        try {
            return DispatchFactory.request(async () => {
                const res = await authAPI.uploadAvatarImage(context)
                ConMsgs.methods.$_console_log(res.data);
                commit(types.USER_UPDATE_AVATAR, res.data)
                return await Promise.resolve(res)
            })
        }
        catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from update avatar image', e.response)
            return await Promise.reject(e.response);
        }
    },
    async updateDisplayName({ commit }, context) {
        try {
            return DispatchFactory.request(async () => {
                const res = await authAPI.updateDisplayName(context)
                ConMsgs.methods.$_console_log(res.data);
                commit(types.USER_UPDATE_DISPLAY_NAME, context)
                return await Promise.resolve(res)
            })
        }
        catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from update display name', e.response)
            return await Promise.reject(e.response);
        }
    },
    async getAllOtherUsers({ commit }) {
        try {
            return DispatchFactory.request(async () => {
                let res = await authAPI.getAllOtherUsers();
                ConMsgs.methods.$_console_log('Got all other user data:', res.data)
                commit(types.USER_GET_OTHERS, res.data)

                return res.data;
            })
        }
        catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from get all other users', e.response)
            return await Promise.reject(e.response);
        }
    },
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
    [types.CODE_CHALLENGE_CREATE](state, data) {
        ConMsgs.methods.$_console_log("Mutating code challenge create")
        state.codeChallenge = data
        localStorage.setItem('codeChallenge', data)
    },
    [types.CODE_CHALLENGE_DESTROY](state) {
        ConMsgs.methods.$_console_log("Mutating code challenge destroy")
        state.codeChallenge = ''
        localStorage.removeItem('codeChallenge')
    },
    [types.LOGIN_SUCCESS](state, data) {
        ConMsgs.methods.$_console_log("Mutating login success")
        let role = data.roles[0].toString()

        state.user = data.user
        state.role = role
        state.accessToken = data.token
        state.isAuthorize = true

        localStorage.setItem('user', JSON.stringify(data.user))
        localStorage.setItem('userRole', role)
        localStorage.setItem('accessToken', data.token)
        localStorage.setItem('isAuthorize', true)
    },
    [types.LOGOUT](state) {
        ConMsgs.methods.$_console_log("Mutating logout");

        state.user = {}
        state.role = ''
        state.accessToken = ''
        state.codeChallenge = ''
        state.isAuthorize = false
        state.activeModules = []
        state.otherUsers = []

        localStorage.removeItem('user')
        localStorage.removeItem('userRole')
        localStorage.removeItem('accessToken')
        localStorage.removeItem('codeChallenge')
        localStorage.removeItem('isAuthorize')
        localStorage.removeItem('activeModules')
        localStorage.removeItem('otherUsers')
    },
    [types.GET_MODULES](state, data) {
        ConMsgs.methods.$_console_log("Mutating get modules");

        // Clean up
        localStorage.removeItem('activeModules')
        state.activeModules = []

        // Add modules to list
        if (typeof data !== 'undefined' && data !== null && data.length > 0) {
            data.forEach(element => { state.activeModules.push(element) });
        }

        localStorage.setItem('activeModules', JSON.stringify(state.activeModules))
    },
    [types.USER_UPDATE_AVATAR](state, data) {
        ConMsgs.methods.$_console_log("Mutating update user avatar");

        state.user.avatar = data

        localStorage.removeItem('user')
        localStorage.setItem('user', JSON.stringify(state.user))
    },
    [types.USER_UPDATE_DISPLAY_NAME](state, data) {
        ConMsgs.methods.$_console_log("Mutating update user display name");

        state.user.displayName = data

        localStorage.removeItem('user')
        localStorage.setItem('user', JSON.stringify(state.user))
    },
    [types.USER_GET_OTHERS](state, data) {
        ConMsgs.methods.$_console_log("Mutating get all other users");

        state.otherUsers = data

        localStorage.removeItem('otherUsers')
        localStorage.setItem('otherUsers', JSON.stringify(data))
    }
}

export default {
    state,
    getters,
    actions,
    mutations
}

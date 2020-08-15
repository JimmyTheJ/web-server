import * as types from '../mutation_types'
import fileAPI from '../../services/file-explorer'
import { getSubdirectoryString } from '../../helpers/browser'
import ConMsgs from '../../mixins/console';
import DispatchFactory from '../../factories/dispatchFactory'

const state = {
    contents: [],
    folders: [],
    directory: '',
    subDirectories: [],
}

const getters = {

}

const actions = {
    async clearFileExplorer({ commit }) {
        ConMsgs.methods.$_console_log('[Vuex][Actions] Clearing file-explorer')

        commit(types.BROWSER_CLEAR)
    },
    async getFolders({ commit }) {
        ConMsgs.methods.$_console_log('[Vuex][Actions] Getting folder list')
        try {
            return await DispatchFactory.request(async () => {
                const res = await fileAPI.getFolderList()
                commit(types.BROWSER_GET_FOLDERS, res.data)

                return await Promise.resolve(res.data)
            })
        }
        catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error getting folder list', e.response)
            return await Promise.reject(e.response)
        }
    },
    async loadDirectory({ commit, state }) {
        try {
            ConMsgs.methods.$_console_log('[Vuex][Actions] Getting directory contents')

            if (state.directory === '') {
                ConMsgs.methods.$_console_log('[Vuex][Actions] Directory is empty, cancelling directory load.')
                return;
            }

            let subDirString = getSubdirectoryString(state.subDirectories)

            return await DispatchFactory.request(async () => {
                const res = await fileAPI.loadDirectory(state.directory, subDirString)
                commit(types.BROWSER_LOAD_DIRECTORY, res.data)

                return await Promise.resolve(res.data)
            })
        }
        catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error getting directory contents', e.response)
            // On failure, go back one level
            commit(types.BROWSER_POP_DIRECTORY)
            return await Promise.reject(e.response)
        }
    },
    async changeDirectory({ commit }, context) {
        ConMsgs.methods.$_console_log('[Vuex][Actions] Changing directory')

        commit(types.BROWSER_CHANGE_DIRECTORY, context)
    },
    async goForwardDirectory({ commit }, context) {
        ConMsgs.methods.$_console_log('[Vuex][Actions] Going foward a directory')

        commit(types.BROWSER_PUSH_DIRECTORY, context)
    },
    async goBackDirectory({ commit }) {
        ConMsgs.methods.$_console_log('[Vuex][Actions] Going backward a directory')

        commit(types.BROWSER_POP_DIRECTORY)
    },
    async addFile({ commit }, context) {
        ConMsgs.methods.$_console_log('[Vuex][Actions] Adding a file')

        commit(types.BROWSER_FILE_ADD, context)
    },
    async deleteFile({ commit }, context) {
        ConMsgs.methods.$_console_log('[Vuex][Actions] Deleting a file')

        commit(types.BROWSER_FILE_DELETE, context)
    }
}

const mutations = {
    [types.BROWSER_CLEAR](state) {
        ConMsgs.methods.$_console_log('[Vuex][Mutations] Clearing file-explorer')

        state.contents = []
        state.folders = []
        state.directory = ''
        state.subDirectories = []
    },
    [types.BROWSER_GET_FOLDERS](state, data) {
        ConMsgs.methods.$_console_log('[Vuex][Mutations] Getting folder list')

        state.folders = data
    },
    [types.BROWSER_LOAD_DIRECTORY](state, data) {
        ConMsgs.methods.$_console_log('[Vuex][Mutations] Getting directory contents')

        state.contents = data
    },
    [types.BROWSER_CHANGE_DIRECTORY](state, data) {
        ConMsgs.methods.$_console_log('[Vuex][Mutations] Changing directory')

        state.directory = data
        state.subDirectories = []
    },
    [types.BROWSER_PUSH_DIRECTORY](state, data) {
        ConMsgs.methods.$_console_log('[Vuex][Mutations] Going forward a directory')

        state.subDirectories.push(data)
    },
    [types.BROWSER_POP_DIRECTORY](state) {
        ConMsgs.methods.$_console_log('[Vuex][Mutations] Going backward a directory')

        if (state.subDirectories.length > 0) {
            state.subDirectories.pop()
        }
    },
    [types.BROWSER_FILE_ADD](state, data) {
        ConMsgs.methods.$_console_log('[Vuex][Mutations] Adding a file')

        state.contents.push(data)
    },
    [types.BROWSER_FILE_DELETE](state, data) {
        ConMsgs.methods.$_console_log('[Vuex][Mutations] Deleting a file')

        let index = state.contents.findIndex(x => x.title === data)
        if (index > -1) {
            state.contents.splice(index, 1)
        }
    }
}

export default {
    state,
    getters,
    actions,
    mutations
}

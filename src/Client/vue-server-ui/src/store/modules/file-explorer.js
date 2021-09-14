import * as types from '../mutation_types'
import fileAPI from '@/services/file-explorer'
import { getSubdirectoryString, getSubdirectoryArray } from '@/helpers/browser'
import ConMsgs from '@/mixins/console'
import Dispatcher from '@/services/ws-dispatcher'

const state = {
  contents: [],
  filteredFiles: [],
  folders: [],
  directory: '',
  subDirectories: [],
  loadingContents: false,
  file: null,
}

const getters = {}

const actions = {
  async clearFileExplorer({ commit }) {
    ConMsgs.methods.$_console_log('[Vuex][Actions] Clearing file-explorer')

    commit(types.BROWSER_CLEAR)
  },
  async getFolders({ commit }) {
    ConMsgs.methods.$_console_log('[Vuex][Actions] Getting folder list')
    try {
      return await Dispatcher.request(async () => {
        const res = await fileAPI.getFolderList()
        commit(types.BROWSER_GET_FOLDERS, res.data)

        return await Promise.resolve(res.data)
      })
    } catch (e) {
      ConMsgs.methods.$_console_group(
        '[Vuex][Actions] Error getting folder list',
        e.response
      )
      return await Promise.reject(e.response)
    }
  },
  async loadDirectory({ commit, state }) {
    try {
      ConMsgs.methods.$_console_log(
        '[Vuex][Actions] Getting directory contents'
      )

      if (state.directory === '') {
        ConMsgs.methods.$_console_log(
          '[Vuex][Actions] Directory is empty, cancelling directory load.'
        )
        return
      }

      let subDirString = getSubdirectoryString(state.subDirectories)

      commit(types.BROWSER_LOADING_CONTENTS, true)
      return await Dispatcher.request(async () => {
        const res = await fileAPI.loadDirectory(state.directory, subDirString)
        commit(types.BROWSER_LOAD_DIRECTORY, res.data)

        commit(types.BROWSER_LOADING_CONTENTS, false)
        return await Promise.resolve(res.data)
      })
    } catch (e) {
      ConMsgs.methods.$_console_group(
        '[Vuex][Actions] Error getting directory contents',
        e.response
      )
      commit(types.BROWSER_LOADING_CONTENTS, false)
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
  async goDirectory({ commit }, context) {
    ConMsgs.methods.$_console_log(
      '[Vuex][Actions] Going to a specific directory'
    )

    const subDirArray = getSubdirectoryArray(context.subDirs)

    commit(types.BROWSER_GO_DIRECTORY, {
      directory: context.directory,
      subDirs: subDirArray,
    })
  },
  async addFile({ commit }, context) {
    ConMsgs.methods.$_console_log('[Vuex][Actions] Adding a file')

    commit(types.BROWSER_FILE_ADD, context)
  },
  async deleteFile({ commit }, context) {
    ConMsgs.methods.$_console_log('[Vuex][Actions] Deleting a file')

    commit(types.BROWSER_FILE_DELETE, context)
  },
  async loadFile({ commit }, context) {
    ConMsgs.methods.$_console_log('[Vuex][Actions] Loading file into viewer')

    commit(types.BROWSER_FILE_LOAD, context)
  },
  async setActiveFile({ commit }, context) {
    ConMsgs.methods.$_console_log(
      `[Vuex][Actions] Setting ${context.value ? 'active' : 'innactive'} file`
    )

    commit(types.BROWSER_FILE_SET_ACTIVE, context)
  },
}

const mutations = {
  [types.BROWSER_CLEAR](state) {
    ConMsgs.methods.$_console_log('[Vuex][Mutations] Clearing file-explorer')

    state.contents = []
    state.filteredFiles = []
    state.folders = []
    state.directory = ''
    state.subDirectories = []
  },
  [types.BROWSER_GET_FOLDERS](state, data) {
    ConMsgs.methods.$_console_log('[Vuex][Mutations] Getting folder list')

    state.folders = data
  },
  [types.BROWSER_LOAD_DIRECTORY](state, data) {
    ConMsgs.methods.$_console_log(
      '[Vuex][Mutations] Getting directory contents'
    )

    state.contents = data

    let copiedData = data.slice(0)
    copiedData.forEach(item => {
      item.active = false
    })
    state.filteredFiles = copiedData.filter(x => !x.isFolder)
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
  [types.BROWSER_GO_DIRECTORY](state, data) {
    ConMsgs.methods.$_console_log('[Vuex][Mutations] Going forward a directory')

    state.directory = data.directory
    state.subDirectories = data.subDirs
  },
  [types.BROWSER_POP_DIRECTORY](state) {
    ConMsgs.methods.$_console_log(
      '[Vuex][Mutations] Going backward a directory'
    )

    if (state.subDirectories.length > 0) {
      state.subDirectories.pop()
    }
  },
  [types.BROWSER_FILE_ADD](state, data) {
    ConMsgs.methods.$_console_log('[Vuex][Mutations] Adding a file')

    let mainIndex
    let compareName = data.title.toLowerCase()
    let isFolder = data.isFolder

    // Match the type (folder, or file)
    let fileTypeContents = state.contents.filter(x => x.isFolder === isFolder)

    // Get the starting index (files come after folders so this index for order would start later than 0)
    if (!Array.isArray(fileTypeContents) || fileTypeContents.length === 0) {
      mainIndex = 0
    } else {
      mainIndex = state.contents.findIndex(
        x => x.title === fileTypeContents[0].title
      )
    }

    let modifiedIndex = 0

    fileTypeContents.forEach((item, index) => {
      if (item.title.toLowerCase() < compareName) {
        modifiedIndex++
      }
    })

    state.contents.splice(mainIndex + modifiedIndex, 0, data)

    if (!data.isFolder) {
      data.active = false
      state.filteredFiles.push(data)
    }
  },
  [types.BROWSER_FILE_DELETE](state, data) {
    ConMsgs.methods.$_console_log('[Vuex][Mutations] Deleting a file')

    let index = state.contents.findIndex(x => x.title === data)
    if (index > -1) {
      state.contents.splice(index, 1)
    }

    let filteredIndex = state.filteredFiles.findIndex(x => x.title === data)
    if (filteredIndex > -1) {
      state.filteredFiles.splice(filteredIndex, 1)
    }
  },
  [types.BROWSER_LOADING_CONTENTS](state, data) {
    ConMsgs.methods.$_console_log(
      `[Vuex][Mutations] Loading contents spinner: ${data}`
    )

    state.loadingContents = data
  },
  [types.BROWSER_FILE_LOAD](state, data) {
    ConMsgs.methods.$_console_log(`[Vuex][Mutations] Load file into viewer`)

    state.file = data
  },
  [types.BROWSER_FILE_SET_ACTIVE](state, data) {
    ConMsgs.methods.$_console_log(
      `[Vuex][Mutations] Setting ${data.value ? 'active' : 'innactive'} file`
    )

    if (typeof state.filteredFiles[data.index] === 'undefined') {
      ConMsgs.methods.$_console_log(
        `[Vuex][Mutations] state.filteredFiles[data.index] is undefined. Here are all the filtered files, and the targetted index/value`,
        state.filteredFiles,
        data
      )
    } else {
      state.filteredFiles[data.index].active = data.value
    }
  },
}

export default {
  state,
  getters,
  actions,
  mutations,
}

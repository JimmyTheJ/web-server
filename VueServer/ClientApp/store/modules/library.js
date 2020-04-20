import * as types from '../mutation_types'
import libraryAPI from '../../services/library'
import ConMsgs from '../../mixins/console';


const state = {
    authors: [],
    books: [],
    bookcases: [],
    genres: [],
    series: [],
    shelves: [],
}

const getters = {

}

const actions = {
    // Author
    async getAuthors({ commit }) {
        try {
            ConMsgs.methods.$_console_log('[Vuex][Actions] Getting author list')

            const res = await libraryAPI.author.getList()
            commit(types.GET_AUTHORS, res.data)

            return await Promise.resolve(res.data)
        } catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from getting author list', e.response)
            return await Promise.reject(e.response)
        }
    },
    async addAuthor({ commit }, context) {
        try {
            ConMsgs.methods.$_console_log('[Vuex][Actions] Adding author')

            const res = await libraryAPI.author.add(context)
            commit(types.ADD_AUTHOR, res.data)

            return await Promise.resolve(res.data)
        }
        catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from adding author', e.response)
            return await Promise.reject(e.response)
        }
    },
    async editAuthor({ commit }, context) {
        try {
            ConMsgs.methods.$_console_log('[Vuex][Actions] Editing author')

            const res = await libraryAPI.author.update(context)
            commit(types.EDIT_AUTHOR, res.data)

            return await Promise.resolve(res.data)
        }
        catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from editing author', e.response)
            return await Promise.reject(e.response)
        }
    },
    async deleteAuthor({ commit }, context) {
        try {
            ConMsgs.methods.$_console_log('[Vuex][Actions] Deleting author')

            const res = await libraryAPI.author.delete(context)
            commit(types.DELETE_AUTHOR, res.data)

            return await Promise.resolve(res.data)
        }
        catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from deleting author', e.response)
            return await Promise.reject(e.response)
        }
    },

    // Book
    async getBooks({ commit }) {
        try {
            ConMsgs.methods.$_console_log('[Vuex][Actions] Getting book list')

            const res = await libraryAPI.book.getList()
            commit(types.GET_BOOKS, res.data)

            return await Promise.resolve(res.data)
        } catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from getting book list', e.response)
            return await Promise.reject(e.response)
        }
    },
    async addBook({ commit }, context) {
        try {
            ConMsgs.methods.$_console_log('[Vuex][Actions] Adding book')

            const res = await libraryAPI.book.add(context)
            commit(types.ADD_BOOK, res.data)

            return await Promise.resolve(res.data)
        }
        catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from adding book', e.response)
            return await Promise.reject(e.response)
        }
    },
    async editBook({ commit }, context) {
        try {
            ConMsgs.methods.$_console_log('[Vuex][Actions] Editing book')

            const res = await libraryAPI.book.update(context)
            commit(types.EDIT_BOOK, res.data)

            return await Promise.resolve(res.data)
        }
        catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from editing book', e.response)
            return await Promise.reject(e.response)
        }
    },
    async deleteBook({ commit }, context) {
        try {
            ConMsgs.methods.$_console_log('[Vuex][Actions] Deleting book')

            const res = await libraryAPI.book.delete(context)
            commit(types.DELETE_BOOK, res.data)

            return await Promise.resolve(res.data)
        }
        catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from deleting book', e.response)
            return await Promise.reject(e.response)
        }
    },

    // Bookcase
    async getBookcases({ commit }) {
        try {
            ConMsgs.methods.$_console_log('[Vuex][Actions] Getting bookcase list')

            const res = await libraryAPI.bookcase.getList()
            commit(types.GET_BOOKCASES, res.data)

            return await Promise.resolve(res.data)
        } catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from getting bookcase list', e.response)
            return await Promise.reject(e.response)
        }
    },
    async addBookcase({ commit }, context) {
        try {
            ConMsgs.methods.$_console_log('[Vuex][Actions] Adding bookcase')

            const res = await libraryAPI.bookcase.add(context)
            commit(types.ADD_BOOKCASES, res.data)

            return await Promise.resolve(res.data)
        } catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from adding bookcase', e.response)
            return await Promise.reject(e.response)
        }
    },
    async editBookcase({ commit }, context) {
        try {
            ConMsgs.methods.$_console_log('[Vuex][Actions] Editing bookcase')

            const res = await libraryAPI.bookcase.update(context)
            commit(types.EDIT_BOOKCASES, res.data)

            return await Promise.resolve(res.data)
        } catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from editing bookcase', e.response)
            return await Promise.reject(e.response)
        }
    },
    async deleteBookcase({ commit }, context) {
        try {
            ConMsgs.methods.$_console_log('[Vuex][Actions] Deleting bookcase')

            const res = await libraryAPI.bookcase.delete(context)
            commit(types.DELETE_BOOKCASE, res.data)

            return await Promise.resolve(res.data)
        } catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from deleting bookcase', e.response)
            return await Promise.reject(e.response)
        }
    },

    // Genre
    async getGenres({ commit }) {
        try {
            ConMsgs.methods.$_console_log('[Vuex][Actions] Getting genre list')

            const res = await libraryAPI.genre.getList()
            commit(types.GET_GENRES, res.data)

            return await Promise.resolve(res.data)
        } catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from getting genre list', e.response)
            return await Promise.reject(e.response)
        }
    },

    // Series
    async getSeries({ commit }) {
        try {
            ConMsgs.methods.$_console_log('[Vuex][Actions] Getting series list')

            const res = await libraryAPI.series.getList()
            commit(types.GET_SERIES, res.data)

            return await Promise.resolve(res.data)
        } catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from getting series list', e.response)
            return await Promise.reject(e.response)
        }
    },
    async addSeries({ commit }, context) {
        try {
            ConMsgs.methods.$_console_log('[Vuex][Actions] Adding series')

            const res = await libraryAPI.series.add(context)
            commit(types.ADD_SERIES, res.data)

            return await Promise.resolve(res.data)
        } catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from adding series', e.response)
            return await Promise.reject(e.response)
        }
    },
    async editSeries({ commit }, context) {
        try {
            ConMsgs.methods.$_console_log('[Vuex][Actions] Editing series')

            const res = await libraryAPI.series.update(context)
            commit(types.EDIT_SERIES, res.data)

            return await Promise.resolve(res.data)
        } catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from editing series', e.response)
            return await Promise.reject(e.response)
        }
    },
    async deleteSeries({ commit }, context) {
        try {
            ConMsgs.methods.$_console_log('[Vuex][Actions] Deleting series')

            const res = await libraryAPI.series.delete(context)
            commit(types.DELETE_SERIES, res.data)

            return await Promise.resolve(res.data)
        } catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from deleting bookcase', e.response)
            return await Promise.reject(e.response)
        }
    },

    // Shelf
    async getShelves({ commit }) {
        try {
            ConMsgs.methods.$_console_log('Getting shelf list')

            const res = await libraryAPI.shelf.getList()
            commit(types.GET_SHELVES, res.data)

            return await Promise.resolve(res.data)
        } catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from getting shelf list', e.response)
            return await Promise.reject(e.response)
        }
    },
    async addShelf({ commit }, context) {
        try {
            ConMsgs.methods.$_console_log('[Vuex][Actions] Adding shelf')

            const res = await libraryAPI.shelf.add(context)
            commit(types.ADD_SHELF, res.data)

            return await Promise.resolve(res.data)
        } catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from adding shelf', e.response)
            return await Promise.reject(e.response)
        }
    },
    async editShelf({ commit }, context) {
        try {
            ConMsgs.methods.$_console_log('[Vuex][Actions] Updating shelf')

            const res = await libraryAPI.shelf.update(context)
            commit(types.EDIT_SHELF, res.data)

            return await Promise.resolve(res.data)
        } catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from adding shelf', e.response)
            return await Promise.reject(e.response)
        }
    },
    async deleteShelf({ commit }, context) {
        try {
            ConMsgs.methods.$_console_log('[Vuex][Actions] Deleting shelf')

            const res = await libraryAPI.shelf.delete(context)
            commit(types.DELETE_SHELF, res.data)

            return await Promise.resolve(res.data)
        } catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from deleting shelf', e.response)
            return await Promise.reject(e.response)
        }
    },
}

const mutations = {
    // Author
    [types.GET_AUTHORS](state, data) {
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Get authors")
        state.authors = data
    },
    [types.ADD_AUTHOR](state, data) {
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Add author")
        state.authors.push(data)
    },
    [types.EDIT_AUTHOR](state, data) {
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Edit author")
    },
    [types.DELETE_AUTHOR](state, data) {
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Delete author")
    },

    // Book
    [types.GET_BOOKS](state, data) {
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Get books")
        state.books = data
    },
    [types.ADD_BOOK](state, data) {
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Add book")
        state.books.push(data)
    },
    [types.EDIT_BOOK](state, data) {
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Edit book")
    },
    [types.DELETE_BOOK](state, data) {
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Delete book")
    },

    // Bookcase
    [types.GET_BOOKCASES](state, data) {
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Get bookcases")
        state.bookcases = data
    },
    [types.ADD_BOOKCASE](state, data) {
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Add bookcase")
        state.bookcases.push(data)
    },
    [types.EDIT_BOOKCASE](state, data) {
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Edit bookcase")
    },
    [types.DELETE_BOOKCASE](state, data) {
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Delete bookcase")
    },

    // Genre
    [types.GET_GENRES](state, data) {
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Get genres")
        state.genres = data
    },


    // Series
    [types.GET_SERIES](state, data) {
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Get series")
        state.series = data
    },
    [types.ADD_SERIES](state, data) {
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Add series")
        state.series.push(data)
    },
    [types.EDIT_SERIES](state, data) {
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Edit series")
    },
    [types.DELETE_SERIES](state, data) {
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Delete series")
    },

    // Shelf
    [types.GET_SHELVES](state, data) {
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Get shelves")
        state.shelves = data
    },
    [types.ADD_SHELF](state, data) {
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Add shelf")
        state.shelves.push(data)
    },
    [types.EDIT_SHELF](state, data) {
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Edit shelf")
    },
    [types.DELETE_SHELF](state, data) {
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Delete shelf")
    },
}


export default {
    state,
    getters,
    actions,
    mutations
}
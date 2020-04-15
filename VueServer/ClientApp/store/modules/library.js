import * as types from '../mutation_types'
import libraryAPI from '../../services/library'
import ConMsgs from '../../mixins/console';


const state = {
    authors: [],
    books: [],
    bookshelves: [],
    series: [],
}

const getters = {

}

const actions = {
    // Author
    async getAuthors({ commit }) {
        try {
            ConMsgs.methods.$_console_log('Getting author list')

            const res = await libraryAPI.author.getList()
            commit(types.GET_AUTHORS, res.data)

            return await Promise.resolve(res.data)
        } catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from getting author list', e.response)
            return await Promise.reject(e.response)
        }
    },
    async addAuthor({ commit }, context) {

    },
    async editAuthor({ commit }, context) {

    },
    async deleteAuthor({ commit }, context) {

    },

    // Book
    async getBooks({ commit }) {
        try {
            ConMsgs.methods.$_console_log('Getting book list')

            const res = await libraryAPI.book.getList()
            commit(types.GET_BOOKS, res.data)

            return await Promise.resolve(res.data)
        } catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from getting book list', e.response)
            return await Promise.reject(e.response)
        }
    },
    async addBook({ commit }, context) {

    },
    async editBook({ commit }, context) {

    },
    async deleteBook({ commit }, context) {

    },

    // Bookshelf
    async getBookshelves({ commit }) {
        try {
            ConMsgs.methods.$_console_log('Getting bookshelf list')

            const res = await libraryAPI.bookshelf.getList()
            commit(types.GET_BOOKSHELVES, res.data)

            return await Promise.resolve(res.data)
        } catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from getting bookshelf list', e.response)
            return await Promise.reject(e.response)
        }
    },
    async addBookshelf({ commit }, context) {

    },
    async editBookshelf({ commit }, context) {

    },
    async deleteBookshelf({ commit }, context) {

    },

    // Genres
    async getGenres({ commit }) {
        try {
            ConMsgs.methods.$_console_log('Getting genre list')

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
            ConMsgs.methods.$_console_log('Getting series list')

            const res = await libraryAPI.series.getList()
            commit(types.GET_SERIES, res.data)

            return await Promise.resolve(res.data)
        } catch (e) {
            ConMsgs.methods.$_console_group('[Vuex][Actions] Error from getting series list', e.response)
            return await Promise.reject(e.response)
        }
    },
    async addSeries({ commit }, context) {

    },
    async editSeries({ commit }, context) {

    },
    async deleteSeries({ commit }, context) {

    },
}

const mutations = {
    [types.GET_AUTHORS](state, data) {
        ConMsgs.methods.$_console_log("Mutating get authors")
        state.authors = data
    },
    [types.GET_BOOKS](state, data) {
        ConMsgs.methods.$_console_log("Mutating get books")
        state.books = data
    },
    [types.GET_BOOKSHELVES](state, data) {
        ConMsgs.methods.$_console_log("Mutating get bookshelves")
        state.bookshelves = data
    },
    [types.GET_GENRES](state, data) {
        ConMsgs.methods.$_console_log("Mutating get genres")
        state.genres = data

        // TODO: Error check here length
        state.genres.splice(0, 0, { id: -1, name: '' });
    },
    [types.GET_SERIES](state, data) {
        ConMsgs.methods.$_console_log("Mutating get series")
        state.series = data
    },
}


export default {
    state,
    getters,
    actions,
    mutations
}

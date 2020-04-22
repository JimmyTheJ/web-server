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

            // Add book
            commit(types.ADD_BOOK, res.data)

            // Add objects
            commit(types.ADD_BOOKCASE, res.data.bookcase)
            commit(types.ADD_SERIES, res.data.series)
            commit(types.ADD_SHELF, res.data.shelf)

            if (Array.isArray(res.data.bookAuthors)) {
                res.data.bookAuthors.forEach(element => {
                    commit(types.ADD_AUTHOR, element.author)
                })
            }

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

            // Add objects
            commit(types.ADD_BOOKCASE, res.data.bookcase)
            commit(types.ADD_SERIES, res.data.series)
            commit(types.ADD_SHELF, res.data.shelf)

            if (Array.isArray(res.data.bookAuthors)) {
                res.data.bookAuthors.forEach(element => {
                    commit(types.ADD_AUTHOR, element.author)
                })
            }

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
            commit(types.ADD_BOOKCASE, res.data)

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
            commit(types.EDIT_BOOKCASE, res.data)

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

        // Check if it's a valid object with a valid id
        if (data !== null && typeof data === 'object' && data.id > 0) {
            const index = state.authors.findIndex(x => x.id === data.id)
            // Author not found in list
            if (index < 0) {
                state.authors.push(data)
                ConMsgs.methods.$_console_log("[Vuex][Mutations] Author added")
            }
            else {
                ConMsgs.methods.$_console_log("[Vuex][Mutations] Author already exists in list!")
            }
        }
    },
    [types.EDIT_AUTHOR](state, data) {
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Edit author")

        // Check if it's a valid object with a valid id
        if (data !== null && typeof data === 'object' && data.id > 0) {
            const index = state.authors.findIndex(x => x.id === data.id)
            // Author not found in list
            if (index < 0) {
                ConMsgs.methods.$_console_log("[Vuex][Mutations] Author doesn't exists in list. Can't update it")                
            }
            else {
                state.authors.splice(index, 1, data)
                ConMsgs.methods.$_console_log("[Vuex][Mutations] Author updated")
            }
        }
    },
    [types.DELETE_AUTHOR](state, data) {
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Delete author")
        // Error state, failed to delete author
        if (typeof data !== 'number' || data <= 0) {
            ConMsgs.methods.$_console_log("[Vuex][Mutations] Invalid id passed in. Author was not deleted")
            return
        }

        const index = state.authors.findIndex(x => x.id === data)
        // Author not found in list
        if (index < 0) {
            ConMsgs.methods.$_console_log("[Vuex][Mutations] Index was not found in the list. Author not deleted")
            return
        }

        // Remove author from list
        state.authors.splice(index, 1)
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Author deleted")
    },

    // Book
    [types.GET_BOOKS](state, data) {
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Get books")
        state.books = data
    },
    [types.ADD_BOOK](state, data) {
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Add book")

        // Check if it's a valid object with a valid id
        if (data !== null && typeof data === 'object' && data.id > 0) {
            const index = state.books.findIndex(x => x.id === data.id)
            // Book not found in list
            if (index < 0) {
                state.books.push(data)
                ConMsgs.methods.$_console_log("[Vuex][Mutations] Book added")
            }
            else {
                ConMsgs.methods.$_console_log("[Vuex][Mutations] Book already exists in list!")
            }
        }
    },
    [types.EDIT_BOOK](state, data) {
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Edit book")

        // Check if it's a valid object with a valid id
        if (typeof data === 'object' && data.id > 0) {
            const index = state.books.findIndex(x => x.id === data.id)
            // Book not found in list
            if (index < 0) {
                ConMsgs.methods.$_console_log("[Vuex][Mutations] Book doesn't exist in list. Can't update it")
            }
            else {
                state.books.splice(index, 1, data)
                ConMsgs.methods.$_console_log("[Vuex][Mutations] Book updated")
            }
        }
    },
    [types.DELETE_BOOK](state, data) {
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Delete book")
        // Error state, failed to delete book
        if (typeof data !== 'number' || data <= 0) {
            ConMsgs.methods.$_console_log("[Vuex][Mutations] Invalid id passed in. Book was not deleted")
            return
        }

        const index = state.books.findIndex(x => x.id === data)
        // Book not found in list
        if (index < 0) {
            ConMsgs.methods.$_console_log("[Vuex][Mutations] Index was not found in the list. Book was not deleted")
            return
        }

        // Remove book from list
        state.books.splice(index, 1)
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Book deleted")
    },

    // Bookcase
    [types.GET_BOOKCASES](state, data) {
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Get bookcases")
        state.bookcases = data
    },
    [types.ADD_BOOKCASE](state, data) {
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Add bookcase")

        // Check if it's a valid object with a valid id
        if (data !== null && typeof data === 'object' && data.id > 0) {
            const index = state.bookcases.findIndex(x => x.id === data.id)
            // Bookcase not found in list
            if (index < 0) {
                state.bookcases.push(data)
                ConMsgs.methods.$_console_log("[Vuex][Mutations] Bookcase added")
            }
            else {
                ConMsgs.methods.$_console_log("[Vuex][Mutations] Bookcase already exists in list!")
            }
        }
    },
    [types.EDIT_BOOKCASE](state, data) {
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Edit bookcase")

        // Check if it's a valid object with a valid id
        if (data !== null && typeof data === 'object' && data.id > 0) {
            const index = state.bookcases.findIndex(x => x.id === data.id)
            // Bookcase not found in list
            if (index < 0) {
                ConMsgs.methods.$_console_log("[Vuex][Mutations] Bookcase doesn't exists in list. Can't update it")
            }
            else {
                state.bookcases.splice(index, 1, data)
                ConMsgs.methods.$_console_log("[Vuex][Mutations] Bookcase updated")
            }
        }
    },
    [types.DELETE_BOOKCASE](state, data) {
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Delete bookcase")
        // Error state, failed to delete bookcase
        if (typeof data !== 'number' || data <= 0) {
            ConMsgs.methods.$_console_log("[Vuex][Mutations] Invalid id passed in. Bookcase was not deleted")
            return
        }

        const index = state.bookcases.findIndex(x => x.id === data)
        // Bookcase not found in list
        if (index < 0) {
            ConMsgs.methods.$_console_log("[Vuex][Mutations] Index was not found in the list. Bookcase not deleted")
            return
        }

        // Remove author from list
        state.bookcases.splice(index, 1)
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Bookcase deleted")
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

        // Check if it's a valid object with a valid id
        if (data !== null && typeof data === 'object' && data.id > 0) {
            const index = state.series.findIndex(x => x.id === data.id)
            // Series not found in list
            if (index < 0) {
                state.series.push(data)
                ConMsgs.methods.$_console_log("[Vuex][Mutations] Series added")
            }
            else {
                ConMsgs.methods.$_console_log("[Vuex][Mutations] Series already exists in list!")
            }
        }
    },
    [types.EDIT_SERIES](state, data) {
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Edit series")

        // Check if it's a valid object with a valid id
        if (data !== null && typeof data === 'object' && data.id > 0) {
            const index = state.series.findIndex(x => x.id === data.id)
            // Series not found in list
            if (index < 0) {
                ConMsgs.methods.$_console_log("[Vuex][Mutations] Series doesn't exists in list. Can't update it")
            }
            else {
                state.series.splice(index, 1, data)
                ConMsgs.methods.$_console_log("[Vuex][Mutations] Series updated")
            }
        }
    },
    [types.DELETE_SERIES](state, data) {
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Delete series")
        // Error state, failed to delete series
        if (typeof data !== 'number' || data <= 0) {
            ConMsgs.methods.$_console_log("[Vuex][Mutations] Invalid id passed in. Series was not deleted")
            return
        }

        const index = state.series.findIndex(x => x.id === data)
        // Series not found in list
        if (index < 0) {
            ConMsgs.methods.$_console_log("[Vuex][Mutations] Index was not found in the list. Series not deleted")
            return
        }

        // Remove series from list
        state.series.splice(index, 1)
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Series deleted")
    },

    // Shelf
    [types.GET_SHELVES](state, data) {
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Get shelves")
        state.shelves = data
    },
    [types.ADD_SHELF](state, data) {
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Add shelf")

        // Check if it's a valid object with a valid id
        if (data !== null && typeof data === 'object' && data.id > 0) {
            const index = state.shelves.findIndex(x => x.id === data.id)
            // Shelf not found in list
            if (index < 0) {
                state.shelves.push(data)
                ConMsgs.methods.$_console_log("[Vuex][Mutations] Shelf added")
            }
            else {
                ConMsgs.methods.$_console_log("[Vuex][Mutations] Shelf already exists in list!")
            }
        }
    },
    [types.EDIT_SHELF](state, data) {
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Edit shelf")

        // Check if it's a valid object with a valid id
        if (data !== null && typeof data === 'object' && data.id > 0) {
            const index = state.shelves.findIndex(x => x.id === data.id)
            // Shelf not found in list
            if (index < 0) {
                ConMsgs.methods.$_console_log("[Vuex][Mutations] Shelf doesn't exists in list. Can't update it")
            }
            else {
                state.shelves.splice(index, 1, data)
                ConMsgs.methods.$_console_log("[Vuex][Mutations] Shelf updated")
            }
        }
    },
    [types.DELETE_SHELF](state, data) {
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Delete shelf")
        // Error state, failed to delete shelf
        if (typeof data !== 'number' || data <= 0) {
            ConMsgs.methods.$_console_log("[Vuex][Mutations] Invalid id passed in. Shelf was not deleted")
            return
        }

        const index = state.shelves.findIndex(x => x.id === data)
        // Shelf not found in list
        if (index < 0) {
            ConMsgs.methods.$_console_log("[Vuex][Mutations] Index was not found in the list. Shelf not deleted")
            return
        }

        // Remove shelf from list
        state.shelves.splice(index, 1)
        ConMsgs.methods.$_console_log("[Vuex][Mutations] Shelf deleted")
    },
}


export default {
    state,
    getters,
    actions,
    mutations
}

import axios from '../axios'
import qs from 'qs'

const GetBookListUrl = `api/library/book/list`
const GetBookUrl = `api/library/book/get`
const AddBookUrl = `api/library/book/add`
const DeleteBookUrl = `api/library/book/delete`
const UpdateBookUrl = `api/library/book/update`

const GetAuthorListUrl = `api/library/author/list`
const AddAuthorUrl = `api/library/author/add`
const DeleteAuthorUrl = `api/library/author/delete`
const UpdateAuthorUrl = `api/library/author/update`

const GetBookcaseListUrl = `api/library/bookcase/list`
const AddBookcaseUrl = `api/library/bookcase/add`
const DeleteBookcaseUrl = `api/library/bookcase/delete`
const UpdateBookcaseUrl = `api/library/bookcase/update`

const GetGenreListUrl = `api/library/genre/list`

const GetSeriesListUrl = `api/library/series/list`
const AddSeriesUrl = `api/library/series/add`
const DeleteSeriesUrl = `api/library/series/delete`
const UpdateSeriesUrl = `api/library/series/update`

const GetShelfListUrl = `api/library/shelf/list`
const AddShelfUrl = `api/library/shelf/add`
const DeleteShelfUrl = `api/library/shelf/delete`
const UpdateShelfUrl = `api/library/shelf/update`

export default {
  author: {
    getList() {
      return axios.get(GetAuthorListUrl)
    },
    delete(id) {
      return axios.request({
        url: DeleteAuthorUrl,
        method: 'delete',
        params: {
          id: id,
        },
      })
    },
    add(data) {
      return axios.post(AddAuthorUrl, data)
    },
    update(data) {
      return axios.request({
        url: UpdateAuthorUrl,
        method: 'put',
        data: data,
        headers: {
          'Content-Type': 'application/json',
        },
      })
    },
  },
  book: {
    getList() {
      return axios.get(GetBookListUrl)
    },
    get(id) {
      return axios.get(GetBookUrl, {
        params: {
          id: id,
        },
      })
    },
    delete(id) {
      return axios.request({
        url: DeleteBookUrl,
        method: 'delete',
        params: {
          id: id,
        },
      })
    },
    add(data) {
      return axios.post(AddBookUrl, data)
    },
    update(data) {
      return axios.request({
        url: UpdateBookUrl,
        method: 'put',
        data: data,
        headers: {
          'Content-Type': 'application/json',
        },
      })
    },
  },
  bookcase: {
    getList() {
      return axios.get(GetBookcaseListUrl)
    },
    delete(id) {
      return axios.request({
        url: DeleteBookcaseUrl,
        method: 'delete',
        params: {
          id: id,
        },
      })
    },
    add(data) {
      return axios.post(AddBookcaseUrl, data)
    },
    update(data) {
      return axios.request({
        url: UpdateBookcaseUrl,
        method: 'put',
        data: data,
        headers: {
          'Content-Type': 'application/json',
        },
      })
    },
  },
  genre: {
    getList() {
      return axios.get(GetGenreListUrl)
    },
  },
  series: {
    getList() {
      return axios.get(GetSeriesListUrl)
    },
    delete(id) {
      return axios.request({
        url: DeleteSeriesUrl,
        method: 'delete',
        params: {
          id: id,
        },
      })
    },
    add(data) {
      return axios.post(AddSeriesUrl, data)
    },
    update(data) {
      return axios.request({
        url: UpdateSeriesUrl,
        method: 'put',
        data: data,
        headers: {
          'Content-Type': 'application/json',
        },
      })
    },
  },
  shelf: {
    getList() {
      return axios.get(GetShelfListUrl)
    },
    delete(id) {
      return axios.request({
        url: DeleteShelfUrl,
        method: 'delete',
        params: {
          id: id,
        },
      })
    },
    add(data) {
      return axios.post(AddShelfUrl, data)
    },
    update(data) {
      return axios.request({
        url: UpdateShelfUrl,
        method: 'put',
        data: data,
        headers: {
          'Content-Type': 'application/json',
        },
      })
    },
  },
}

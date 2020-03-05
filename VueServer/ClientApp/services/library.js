import axios from '../axios'

const GetBookListUrl = `api/library/book/list`;
const GetBookUrl = `api/library/book/get`;
const AddBookUrl = `api/library/book/add`;
const DeleteBookUrl = `api/library/book/delete`;
const UpdateBookUrl = `api/library/book/update`;

const GetGenreListUrl = `api/library/genre/list`;
const GetSeriesListUrl = `api/library/series/list`;
const GetAuthorListUrl = `api/library/author/list`;
const GetBookshelfListUrl = `api/library/bookshelf/list`;

export default {
    book: {
        getList() {
            return axios.get(GetBookListUrl);
        },
        get(id) {
            return axios.get(GetBookUrl, {
                params: {
                    id: id
                }
            });
        },
        delete(id) {
            return axios.request({
                url: DeleteBookUrl,
                method: 'delete',
                params: {
                    id: id
                }
            });
        },
        add(data) {
            return axios.post(AddBookUrl, data);
        },
        update(data) {
            return axios.request({
                url: UpdateBookUrl,
                method: 'put',
                data: data,
                headers: {
                    "Content-Type": "application/json"
                }
            });
        },  
    },
    genre: {
        getList() {
            return axios.get(GetGenreListUrl);
        },
    },
    series: {
        getList() {
            return axios.get(GetSeriesListUrl);
        },
    },
    author: {
        getList() {
            return axios.get(GetAuthorListUrl);
        },
    },
    bookshelf: {
        getList() {
            return axios.get(GetBookshelfListUrl);
        },
    },
}

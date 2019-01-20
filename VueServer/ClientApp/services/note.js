import axios from '../axios'

const GetAllNotesUrl = `api/note/getall`;
const GetNotesUrl = `api/note/get`;
const CreateNotesUrl = `api/note/create`;
const UpdateNotesUrl = `api/note/update`;
const DeleteNotesUrl = `api/note/delete`;

export default {
    getAllNotes() {
        return axios.get(GetAllNotesUrl)
    },
    getNotes() {
        return axios.get(GetNotesUrl);
    },
    deleteNote(id) {
        return axios.request({
            url: DeleteNotesUrl,
            method: 'delete',
            params: {
                id: id
            }
        })
    },
    createNote(data) {
        return axios.post(CreateNotesUrl, data);
    },
    updateNote(data) {
        return axios.request({
            url: UpdateNotesUrl,
            method: 'put',
            data: data,
            headers: {
                "Content-Type": "application/json"
            }
        });
    }
}

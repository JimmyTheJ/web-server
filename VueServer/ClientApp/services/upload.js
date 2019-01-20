import axios from '../axios'

const FolderListUrl = `api/upload/folders`;
const UploadListUrl = `api/upload/list`;
const DeleteUploadUrl = `api/upload/delete`;
const UploadFilesUrl = `api/upload/upload`;

export default {
    getFolders() {
        return axios.get(FolderListUrl);
    },
    getList() {
        return axios.get(UploadListUrl);
    },
    deleteFile(file, folder) {
        return axios.request({
            url: DeleteUploadUrl,
            method: 'POST',
            data: {
                fileName: file,
                folder: folder
            }
        })
    },
    uploadFile(data) {
        return axios.request({
            url: UploadFilesUrl,
            method: 'POST',
            data: data,
            headers: {
                'Content-Type': false,
                'Process-Data': false
            },
            timeout: 60000,
        });
    }
}

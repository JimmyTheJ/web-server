import axios from '../axios'

const LoadDirectoryUrl = `api/directory/folder`;
const FileListUrl = `api/directory/list`;
const DeleteUploadUrl = `api/directory/delete`;
const UploadFilesUrl = `api/directory/upload`;
const GetFileUrl = `api/directory/download/file/`;

export default {
    loadDirectory(dir, subDir) {
        if (!subDir)
            return axios.get(`${LoadDirectoryUrl}/${dir}`);
        else
            return axios.get(`${LoadDirectoryUrl}/${dir}/${subDir}`);
    },
    getFolderList() {
        return axios.get(FileListUrl);
    },
    deleteFile(file, folder, subFolder) {
        return axios.request({
            url: DeleteUploadUrl,
            method: 'POST',
            data: {
                Name: file,
                Directory: folder,
                SubDirectory: subFolder
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
    },
    getFile(url) {
        return axios.get(GetFileUrl + url);
    }
}

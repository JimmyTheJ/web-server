import axios from '@/axios'
import store from '@/store/index'

const LoadDirectoryUrl = `api/directory/folder`
const FileListUrl = `api/directory/list`
const GetFileUrl = `api/directory/download/file`
const DeleteUploadUrl = `api/directory/delete`
const UploadFilesUrl = `api/directory/upload`
const CreateFolderUrl = `api/directory/create-folder`
const RenameFileUrl = `api/directory/rename-file`

export default {
  loadDirectory(dir, subDir) {
    if (
      typeof subDir === 'undefined' ||
      subDir === null ||
      subDir === '' ||
      subDir === false
    )
      return axios.get(`${LoadDirectoryUrl}/${dir}`)
    else return axios.get(`${LoadDirectoryUrl}/${dir}/${subDir}`)
  },
  getFile(file) {
    return axios.get(`${GetFileUrl}/${file}`, {
      params: { token: store.state.auth.accessToken },
    })
  },
  getFolderList() {
    return axios.get(FileListUrl)
  },
  createFolder(name, folder, subFolder) {
    return axios.request({
      url: CreateFolderUrl,
      method: 'POST',
      data: {
        Name: name,
        Directory: folder,
        SubDirectory: subFolder,
      },
    })
  },
  renameFile(oldName, newName, folder, subFolder) {
    return axios.request({
      url: RenameFileUrl,
      method: 'PUT',
      data: {
        Name: oldName,
        NewName: newName,
        Directory: folder,
        SubDirectory: subFolder,
      },
    })
  },
  deleteFile(file, folder, subFolder) {
    return axios.request({
      url: DeleteUploadUrl,
      method: 'POST',
      data: {
        Name: file,
        Directory: folder,
        SubDirectory: subFolder,
      },
    })
  },
  uploadFile(data) {
    return axios.request({
      url: UploadFilesUrl,
      method: 'POST',
      data: data,
      headers: {
        'Content-Type': false,
        'Process-Data': false,
      },
      timeout: 60000,
    })
  },
}

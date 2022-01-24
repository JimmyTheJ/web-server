import axios from '@/axios'
import store from '@/store/index'

const LoadDirectoryUrl = `api/directory/folder`
const FileListUrl = `api/directory/list`
const GetFileUrl = `api/directory/download/file`
const DeleteUploadUrl = `api/directory/delete`
const UploadFilesUrl = `api/directory/upload`
const CreateFolderUrl = `api/directory/create-folder`
const RenameFileUrl = `api/directory/rename-file`
const RenameFolderurl = `api/directory/rename-folder`
const MoveFileUrl = `api/directory/move-file`
const MoveFolderurl = `api/directory/move-folder`
const CopyFileUrl = `api/directory/copy-file`
const CopyFolderurl = `api/directory/copy-folder`

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
  renameFile(oldName, newName, folder, subFolder, isFolder) {
    return axios.request({
      url: isFolder === true ? RenameFolderurl : RenameFileUrl,
      method: 'POST',
      data: {
        Name: oldName,
        NewName: newName,
        Directory: folder,
        SubDirectory: subFolder,
      },
    })
  },
  moveFile(source, destination, isFolder) {
    return axios.request({
      url: isFolder === true ? MoveFolderurl : MoveFileUrl,
      method: 'POST',
      data: {
        Source: {
          Name: source.name,
          Directory: source.folder,
          SubDirectory: source.subFolder,
        },
        Destination: {
          Name: destination.name,
          Directory: destination.folder,
          SubDirectory: destination.subFolder,
        },
      },
    })
  },
  copyFile(source, destination) {
    return axios.request({
      url: source.isFolder === true ? CopyFolderurl : CopyFileUrl,
      method: 'POST',
      data: {
        Source: {
          Name: source.name,
          Directory: source.directory,
          SubDirectory: source.subDirectory,
        },
        Destination: {
          Name: destination.name,
          Directory: destination.directory,
          SubDirectory: destination.subDirectory,
        },
      },
    })
  },
  deleteFile(file, folder, subFolder) {
    return axios.request({
      url: DeleteUploadUrl,
      method: 'DELETE',
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
      timeout: 600000,
    })
  },
}

/**
 * Import Dependency
 */
import axios from '@/axios'

/**
 * Declare Variable
 */
const getDirectorySettingsUrl = `api/directory/admin/settings/get`
const getGroupDirectoriesUrl = `api/directory/admin/group/get`
const getUserDirectoriesUrl = `api/directory/admin/user/get`
const setServerSettingUrl = `api/admin/settings/set`
const deleteServerSettingUrl = `api/admin/settings/delete`
const addGroupDirectoryUrl = `api/directory/admin/group/add`
const addUserDirectoryUrl = `api/directory/admin/user/add`
const deleteGroupDirectoryUrl = `api/directory/admin/group/delete`
const deleteUserDirectoryUrl = `api/directory/admin/user/delete`
const createDefaultUserDirectoryUrl = `api/directory/admin/user/create-default-directory`

/**
 * Export
 */
export default {
  getDirectorySettings() {
    return axios.get(getDirectorySettingsUrl)
  },
  getGroupDirectories() {
    return axios.get(getGroupDirectoriesUrl)
  },
  getUserDirectories() {
    return axios.get(getUserDirectoriesUrl)
  },
  setServerSetting(setting) {
    return axios.post(setServerSettingUrl, setting)
  },
  deleteServerSetting(key) {
    return axios.request({
      url: `${deleteServerSettingUrl}/${key}`,
      method: 'delete',
    })
  },
  addGroupDirectory(obj) {
    return axios.post(addGroupDirectoryUrl, obj)
  },
  addUserDirectory(obj) {
    return axios.post(addUserDirectoryUrl, obj)
  },
  deleteGroupDirectory(id) {
    return axios.request({
      url: `${deleteGroupDirectoryUrl}/${id}`,
      method: 'delete',
    })
  },
  deleteUserDirectory(id) {
    return axios.request({
      url: `${deleteUserDirectoryUrl}/${id}`,
      method: 'delete',
    })
  },
  createDefaultUserDirectory(id) {
    return axios.post(`${createDefaultUserDirectoryUrl}/${id}`)
  },
}

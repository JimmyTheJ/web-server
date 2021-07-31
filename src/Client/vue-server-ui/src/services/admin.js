/**
 * Import Dependency
 */
import axios from '@/axios'

/**
 * Declare Variable
 */
const getDirectorySettingsUrl = `api/admin/directory/settings/get`
const getGroupDirectoriesUrl = `api/admin/directory/group/get`
const getUserDirectoriesUrl = `api/admin/directory/user/get`
const setServerSettingUrl = `api/admin/settings/set`
const deleteServerSettingUrl = `api/admin/settings/delete`
const addGroupDirectoryUrl = `api/admin/directory/group/add`
const addUserDirectoryUrl = `api/admin/directory/user/add`
const deleteGroupDirectoryUrl = `api/admin/directory/group/delete`
const deleteUserDirectoryUrl = `api/admin/directory/user/delete`

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
}

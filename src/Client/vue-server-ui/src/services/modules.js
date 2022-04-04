/**
 * Import Dependency
 */
import axios from '@/axios'

const getEnabledModulesUrl = `api/modules/get-enabled-modules`
const getAllModulesUrl = `api/modules/get-all-modules`
const getModulesForUserUrl = `api/modules/get-modules-for-user`
const getAllModulesForAllUserUrl = `api/modules/get-modules-for-all-users`
const getUserModulesAndFeaturesUrl = `get-user-modules-and-features`
const addModuleToUserUrl = `api/modules/add-module-to-user`
const deleteModuleFromUserUrl = `api/modules/delete-module-from-user`
const addFeatureToUserUrl = `api/modules/add-feature-to-user`
const deleteFeatureFromUserUrl = `api/modules/delete-feature-from-user`

/**
 * Export
 */
export default {
  getEnabledModules() {
    return axios.get(getEnabledModulesUrl)
  },
  getModulesForUser() {
    return axios.get(getModulesForUserUrl)
  },
  getAllModules() {
    return axios.get(getAllModulesUrl)
  },
  getAllModulesForAllUser() {
    return axios.get(getAllModulesForAllUserUrl)
  },
  getModulesAndFeaturesForUser(user) {
    return axios.get(getUserModulesAndFeaturesUrl, { user: user })
  },
  getAllFeaturesForAllUser() {
    return axios.get(getAllFeaturesForAllUserUrl)
  },
  addModuleToUser(obj) {
    return axios.post(addModuleToUserUrl, obj)
  },
  deleteModuleFromUser(obj) {
    return axios.post(deleteModuleFromUserUrl, obj)
  },
  addFeatureToUser(obj) {
    return axios.post(addFeatureToUserUrl, obj)
  },
  deleteFeatureFromUser(obj) {
    return axios.post(deleteFeatureFromUserUrl, obj)
  },
}

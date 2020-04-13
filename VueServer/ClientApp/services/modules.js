/**
 * Import Dependency
 */
import axios from '../axios'

const getAllModulesUrl = `api/modules/get-all-modules`
const getModulesForUserUrl = `api/modules/get-modules-for-user`
const getAllModulesForAllUserUrl = `api/modules/get-modules-for-all-users`
const addModuleToUserUrl = `api/modules/add-module-to-user`
const deleteModuleFromUserUrl = `api/modules/delete-module-from-user`

/**
 * Export
 */
export default {
    getModulesForUser() {
        return axios.get(getModulesForUserUrl)
    },
    getAllModules() {
        return axios.get(getAllModulesUrl)
    },
    getAllModulesForAllUser() {
        return axios.get(getAllModulesForAllUserUrl)
    },
    addModuleToUser(obj) {
        return axios.post(addModuleToUserUrl, obj)
    },
    deleteModuleFromUser(obj) {
        return axios.post(deleteModuleFromUserUrl, obj)
    }
}

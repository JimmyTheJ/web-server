/**
 * Import Dependency
 */
import axios from '../axios'

/**
 * Declare Variable
 */
const signinUrl = `api/account/login`
const signoutUrl = `api/account/logout`
const getCsrfTokenUrl = `api/account/get-csrf-token`
const refreshTokenUrl = `api/account/refresh-jwt`
const registerUrl = `api/account/register`
const getUsersUrl = `api/account/user/get-all`
const getUserIdsUrl = `api/account/user/get-all-ids`
const updateAvatarImageUrl = `api/account/user/update-avatar`
const updateDisplayNameUrl = `api/account/user/update-display-name`

/**
 * Export
 */
export default {
    getCsrfToken(data) {
        return axios.get(getCsrfTokenUrl)
    },
    signin(data) {
        return axios.post(signinUrl, {
            'username': data.username,
            'password': data.password
        })
    },
    signout() {
        return axios.post(signoutUrl)
    },
    refreshToken(token, refreshToken) {
        return axios.post(`${refreshTokenUrl}`, {
            'token': token,
            'refreshToken': refreshToken,
        })
    },
    register(data) {
        return axios.post(registerUrl, {
            'Username': data.username,
            'Password': data.password,
            'ConfirmPassword': data.confirmPassword,
            'Role': data.role
        })
    },
    getUsers() {
        return axios.get(getUsersUrl)
    },
    getUserIds() {
        return axios.get(getUserIdsUrl)
    },
    uploadAvatarImage(file) {
        return axios.post(updateAvatarImageUrl, file)
    },
    updateDisplayName(name) {
        return axios.post(updateDisplayNameUrl, name)
    }
}

/**
 * Import Dependency
 */
import axios from '../axios'

/**
 * Declare Variable
 */
const signinUrl = `api/account/login`
const signoutUrl = `api/account/logout`
const refreshTokenUrl = `api/account/refresh-jwt`
const registerUrl = `api/account/register`
const getUsersUrl = `api/account/user/get-all`
const getAllOtherUsersUrl = `api/account/user/get-all-others`
const updateAvatarImageUrl = `api/account/user/update-avatar`
const updateDisplayNameUrl = `api/account/user/update-display-name`

/**
 * Export
 */
export default {
    signin(data) {
        return axios.post(signinUrl, {
            'username': data.username,
            'password': data.password,
            'codeChallenge': data.codeChallenge
        })
    },
    signout(username) {
        return axios.post(signoutUrl, `"${username}"`)
    },
    refreshToken(token, codeChallenge) {
        return axios.post(`${refreshTokenUrl}`, {
            token: token,
            codeChallenge: codeChallenge
        })
    },
    register(data) {
        return axios.post(registerUrl, {
            'username': data.username,
            'password': data.password,
            'confirmPassword': data.confirmPassword,
            'role': data.role
        })
    },
    getUsers() {
        return axios.get(getUsersUrl)
    },
    getAllOtherUsers() {
        return axios.get(getAllOtherUsersUrl)
    },
    uploadAvatarImage(file) {
        return axios.post(updateAvatarImageUrl, file)
    },
    updateDisplayName(name) {
        return axios.post(updateDisplayNameUrl, `"${name}"`)
    }
}

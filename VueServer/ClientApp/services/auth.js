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
    }
}

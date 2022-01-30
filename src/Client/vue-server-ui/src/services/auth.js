/**
 * Import Dependency
 */
import axios from '@/axios'

/**
 * Declare Variable
 */
const signinUrl = `api/account/login`
const signoutUrl = `api/account/logout`
const refreshTokenUrl = `api/account/refresh-jwt`
const validateTokenUrl = `api/account/validate-tokens`
const registerUrl = `api/account/register`
const userChangePasswordUrl = `api/account/user/change-password`
const getUserProfileUrl = `api/account/user/profile/get`
const getUsersUrl = `api/account/user/get-all`
const getAllOtherUsersUrl = `api/account/user/get-all-others`
const getUsersFuzzySearchUrl = `api/account/user/get-users-fuzzy`
const updateAvatarImageUrl = `api/account/user/update-avatar`
const updateDisplayNameUrl = `api/account/user/update-display-name`
const getGuestLoginListUrl = `api/account/guest/logins`
const unblockGuestIPUrl = `api/account/guest/unblock`

const adminChangePasswordUrl = `api/admin/change-password`
const adminGetRolesUrl = `api/admin/roles/get-all`

/**
 * Export
 */
export default {
  signin(data) {
    return axios.post(
      signinUrl,
      {
        username: data.username,
        password: data.password,
        codeChallenge: data.codeChallenge,
      },
      { withCredentials: true }
    )
  },
  signout(username) {
    return axios.post(signoutUrl, `"${username}"`)
  },
  changePassword(data, isAdmin) {
    if (isAdmin) {
      return axios.post(adminChangePasswordUrl, {
        oldPassword: data.oldPassword,
        newPassword: data.newPassword,
        confirmNewPassword: data.confirmNewPassword,
      })
    } else {
      return axios.post(userChangePasswordUrl, {
        oldPassword: data.oldPassword,
        newPassword: data.newPassword,
        confirmNewPassword: data.confirmNewPassword,
      })
    }
  },
  refreshToken(token) {
    return axios.post(`${refreshTokenUrl}`, `'${token}'`, {
      withCredentials: true,
    })
  },
  validateToken(token) {
    return axios.post(`${validateTokenUrl}`, `'${token}'`, {
      withCredentials: true,
    })
  },
  register(data) {
    return axios.post(registerUrl, {
      username: data.username,
      password: data.password,
      confirmPassword: data.confirmPassword,
      role: data.role,
    })
  },
  getRoles() {
    return axios.get(adminGetRolesUrl)
  },
  getUsers() {
    return axios.get(getUsersUrl)
  },
  getUserProfile(id) {
    return axios.get(`${getUserProfileUrl}/${id}`)
  },
  getAllOtherUsers() {
    return axios.get(getAllOtherUsersUrl)
  },
  getUsersFuzzy(query) {
    return axios.get(getUsersFuzzySearchUrl, { params: { query: query } })
  },
  uploadAvatarImage(file) {
    return axios.post(updateAvatarImageUrl, file)
  },
  updateDisplayName(name) {
    return axios.post(updateDisplayNameUrl, `"${name}"`)
  },
  getGuestLogins() {
    return axios.get(getGuestLoginListUrl)
  },
  unblockGuest(ip) {
    return axios.post(unblockGuestIPUrl, `"${ip}"`)
  },
}

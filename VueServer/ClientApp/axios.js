import axios from 'axios'
import store from './store'
import router from './router'

import ConMsgs from './mixins/console'

const API_URL = process.env.API_URL

const ax = axios.create({
    baseURL: API_URL,
    headers: {
        'Content-Type': 'application/json',
    }
})

// 10s timeout in production - 1 minute timeout in development
ax.defaults.timeout = process.env.NODE_ENV === 'production' ? 10000 : 60000

ax.interceptors.request.use(config => {
    let credential = store.getters.getAccessToken
    let isAuthorize = store.getters.getIsAuthorize
    let csrf = store.getters.getCsrfToken

    if (credential && isAuthorize)
        config.headers.common['Authorization'] = 'Bearer ' + credential
    config.headers.common['X-CSRF-TOKEN'] = csrf

    return config
}, error => {
    ConMsgs.methods.$_console_group('[Axios][Interceptor] Request Error', error)
    return Promise.reject(error)
})

ax.interceptors.response.use(data => {
    return data
}, error => {
    let headerKeys = Object.keys(error.response.headers)
    ConMsgs.methods.$_console_log(error.response, headerKeys)

    if (headerKeys.includes('token-expired')) {
        store.dispatch('signout').then(resp => { }).catch(() => { }).then(() => {
            ConMsgs.methods.$_console_log('[Axios][Interceptor] Signed user out as the token has expired')
        });
    }
    //if (headerKeys.includes('token-expired')) {
    //    ConMsgs.methods.$_console_log('expired token, refreshing tokens')
    //    store.dispatch('refreshToken')
    //        .then(resp => {
    //            ConMsgs.methods.$_console_log('Successfully gathered refresh token')
    //            store.dispatch('getCsrfToken').then(resp2 => {
    //                ConMsgs.methods.$_console_log('Successfully gathered csrf token')
    //            }).catch(() => ConMsgs.methods.$_console_log('Failed to get csrf token'))
    //        }).catch(() => ConMsgs.methods.$_console_log('Failed to refresh the tokens'))
    //}
    else {
        ConMsgs.methods.$_console_group('[Axios][Interceptor] Response Error', error)
    }
    return Promise.reject(error)
})

export default ax;

import axios from 'axios'
import store from './store'
import router from './router'

import ConMsgs from './mixins/console'
import { ApiEndpoints, RouterEndpoints } from './constants'

const API_URL = process.env.API_URL

const ax = axios.create({
    baseURL: API_URL,
    headers: getHeaders()
})

// 10s timeout in production - 1 minute timeout in development
ax.defaults.timeout = process.env.NODE_ENV === 'production' ? 10000 : 60000

ax.interceptors.request.use(async config => {
    let credential = store.state.auth.accessToken
    let isAuthorize = store.state.auth.isAuthorize

    if (credential && isAuthorize)
        config.headers.common['Authorization'] = 'Bearer ' + credential
    return config
}, error => {
    ConMsgs.methods.$_console_group('[Axios][Interceptor] Request Error', error)
    return Promise.reject(error)
});

ax.interceptors.response.use(data => {
    return data;
}, error => {
    ConMsgs.methods.$_console_log('[Axios][Interceptor] Error request, response, then header keys, route fullpath', error.request, error.response, router.app.$route.fullPath);

    // If we are trying to get a refresh token and we get an unauthorized response log us out because we will now fail all requests anyways
    if (router.app.$route.path === RouterEndpoints.RefreshToken && error.status === 401) {
        logout()
    }

    return Promise.reject(error);
});

export default ax;

function getHeaders() {
    var headers = { 'Content-Type': 'application/json; charset=utf-8', }
    if (process.env.NODE_ENV === 'development') {
      headers['Access-Control-Allow-Origin'] = '*'
    }
    
    return headers;
  } 

function logout() {
    store.dispatch("signout")
        .then(() => {}).catch(() => ConMsgs.methods.$_console_log("logout fail"))
        .then(() => router.push({ name: 'login' }) )
}

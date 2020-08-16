import axios from 'axios'
import store from './store'
import router from './router'

import ConMsgs from './mixins/console'

const API_URL = process.env.API_URL

const ax = axios.create({
    baseURL: API_URL,
    headers: {
        'Content-Type': 'application/json; charset=utf-8'
    }
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
    return Promise.reject(error);
});

ax.interceptors.response.use(data => {
    return data;
}, error => {
    ConMsgs.methods.$_console_log('[Axios][Interceptor] Error request, response, then header keys, route fullpath', error.request, error.response, router.app.$route.fullPath);

    return Promise.reject(error);
});

export default ax;

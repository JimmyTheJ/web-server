import { ApiEndpoints } from './constants'
import authService from './services/auth'
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
    let csrf = store.state.auth.csrfToken

    if (credential && isAuthorize)
        config.headers.common['Authorization'] = 'Bearer ' + credential
    config.headers.common['X-CSRF-TOKEN'] = csrf

    return config
}, error => {
    ConMsgs.methods.$_console_group('[Axios][Interceptor] Request Error', error)
    return Promise.reject(error);
});

ax.interceptors.response.use(data => {
    return data;
}, error => {
    //let headerKeys = Object.keys(error.response.headers);
    //let r = router.app.$route.name;
    ConMsgs.methods.$_console_log('[Axios][Interceptor] Error request, response, then header keys, route fullpath', error.request, error.response, router.app.$route.fullPath);

    //if (error.response.status != 401) {
    //    return new Promise((resolve, reject) => {
    //        reject(error);
    //    });
    //}

    //if (error.config.url === ApiEndpoints.RefreshToken) {
    //    return new Promise((resolve, reject) => {
    //        reject(error);
    //    });
    //}
    
        //return authService.refreshToken(store.state.auth.accessToken).then(obj => {
        //    console.log('Recalling the caller');
        //    console.log(error.config);
        //    console.log(obj);
        //    let config = error.config;
        //    config.headers['Authorization'] = 'Bearer ' + obj.token;
        //    config.headers['X-CSRF-TOKEN'] = store.state.auth.csrfToken;

        //    console.log(config.headers);

        //return new Promise((resolve, reject) => {
        //    console.log('New Promise');
        //    ax.request(config).then(resp => {
        //        resolve(resp);
        //    }).catch((err) => {
        //        reject(err);
        //    });
        //});
    //}).catch((err) => {
    //    return Promise.reject(err);
    //});

    //if (error.response.status === 401 && headerKeys.includes('token-expired') && r !== '/' && r !== 'login' && r !== 'register') {
    //    ConMsgs.methods.$_console_log('[Axios][Interceptor] 401 error with token expired')

    //    store.dispatch('signout').then(resp => { }).catch(() => { }).then(() => {
    //        ConMsgs.methods.$_console_log('[Axios][Interceptor] Signed user out as the token has expired')
    //        router.push({ name: 'login' })
    //    });
    //}
    //else {
    //    ConMsgs.methods.$_console_group('[Axios][Interceptor] Response Error', error)
    //}
    return Promise.reject(error);
});

export default ax;

import ConMsgs from '../mixins/console'

export const requiresRefresh = function (token) {
    ConMsgs.methods.$_console_log('[RequiresRefresh] Checking if refresh token is required.');

    let parsedJwt = parse(token);
    let time = new Date().getTime() / 1000;

    if (time > parsedJwt.exp) {
        ConMsgs.methods.$_console_log('[RequiresRefresh] JWT is expired!');

        return true;
    }

    return false;
}

export const parse = function (token) {
    return JSON.parse(atob(token.split('.')[1]));
};

export const getCodeChallenge = function (len) {
    let arr = new Uint8Array((len || 40) / 2);
    crypto.getRandomValues(arr);

    return Array.from(arr, dec2hex).join('');
}

function dec2hex(dec) {
    return dec < 10 ? '0' + String(dec) : dec.toString(16);
}

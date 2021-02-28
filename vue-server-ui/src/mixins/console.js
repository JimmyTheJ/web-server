import * as CONST from '../constants'

export default {
    methods: {
        /**
         * Group multiple console.log messages together into one call and display them only in development
         * @param {...String} messages - Spread array of message strings
         */
        $_console_log(...messages) {
            if (typeof messages === 'undefined' || messages === null) {
                return false;
            }

            if (process.env.NODE_ENV === 'development') {
                for (let i = 0; i < messages.length; i++) {
                    console.log(messages[i]);
                }
                return true;
            }
            else {
                return false;
            }
        },

        /**
         * Group multiple console.log messages together into one call and display them only in development
         * @param {String} header - Group header
         * @param {...String} messages - Spread array of message strings
         */
        $_console_group(header, ...messages) {
            if (typeof messages === 'undefined' || messages === null) {
                return false;
            }

            if (process.env.NODE_ENV === 'development') {
                console.group(header);
                for (let i = 0; i < messages.length; i++) {
                    console.log(messages[i]);
                }
                console.groupEnd();
                return true;
            }
            else {
                return false;
            }
        }
    }
}

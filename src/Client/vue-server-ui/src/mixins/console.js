export default {
  methods: {
    /**
     * Group multiple console.log messages together into one call and display them only in development
     * @param {...String} messages - Spread array of message strings
     */
    $_console_log(...messages) {
      if (typeof messages === 'undefined' || messages == null) {
        return false
      }

      if (process.env.NODE_ENV !== 'production') {
        for (let i = 0; i < messages.length; i++) {
          console.log(messages[i])
        }
        return true
      } else {
        return false
      }
    },

    /**
     * Group multiple console.error messages together into one call and display them only in development
     * @param {...String} messages - Spread array of message strings
     */
    $_console_error(...messages) {
      if (typeof messages === 'undefined' || messages == null) {
        return false
      }

      if (process.env.NODE_ENV !== 'production') {
        for (let i = 0; i < messages.length; i++) {
          console.error(messages[i])
        }
        return true
      } else {
        return false
      }
    },

    /**
     * Group multiple console.log messages together into one call and display them only in development
     * @param {String} header - Group header
     * @param {...String} messages - Spread array of message strings
     */
    $_console_group(header, ...messages) {
      if (typeof messages === 'undefined' || messages == null) {
        return false
      }

      if (process.env.NODE_ENV !== 'production') {
        console.group(header)
        for (let i = 0; i < messages.length; i++) {
          console.log(messages[i])
        }
        console.groupEnd()
        return true
      } else {
        return false
      }
    },
  },
}

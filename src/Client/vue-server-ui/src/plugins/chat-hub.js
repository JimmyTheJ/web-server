'use strict'

import { HubConnectionBuilder, LogLevel } from '@aspnet/signalr'
import store from '../store/index.js'
import { Modules } from '../constants.js'
const DELAY = 5000

export default {
  install(Vue) {
    const connection = new HubConnectionBuilder()
      .withUrl(`${process.env.VUE_APP_API_URL}/chat-hub`)
      .configureLogging(LogLevel.Information)
      .build()

    // use new Vue instance as an event bus
    const chatHub = new Vue()
    // every component will use this.$chatHub to access the event bus
    Vue.prototype.$chatHub = chatHub
    // Forward server side SignalR events through $chatHub, where components will listen to them
    connection.on('SendMessage', message => {
      chatHub.$emit('message-received', message)
    })
    connection.on('ReadMessage', receipt => {
      chatHub.$emit('read-receipt-received', receipt)
    })

    let startedPromise = null
    function start() {
      let isAuthorize = store.state.auth.isAuthorize
      let modules = store.state.auth.activeModules
      // Only try to connect if we are authorized and have the chat module is our list of features
      if (isAuthorize && modules.findIndex(x => x.id === Modules.Chat) > -1) {
        startedPromise = connection.start().catch(err => {
          console.error('Failed to connect with hub', err)
          return new Promise((resolve, reject) =>
            setTimeout(
              () =>
                start()
                  .then(resolve)
                  .catch(reject),
              DELAY
            )
          )
        })
        return startedPromise
      } else {
        console.log('Chat Hub State')
        console.log(connection.state)
        connection.onclose(() => console.log('Closed')) // eat it
        connection.stop().catch(err => console.error(err))
        return new Promise((resolve, reject) =>
          setTimeout(
            () =>
              start()
                .then(resolve)
                .catch(reject),
            DELAY
          )
        )
      }
    }
    connection.onclose(() => {
      if (connection.status === 0) start()
    })

    start()

    return connection
  },
}

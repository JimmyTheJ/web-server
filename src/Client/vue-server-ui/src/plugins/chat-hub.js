'use strict'

import * as signalR from '@microsoft/signalr'
import ConMsgs from '@/mixins/console.js'
import Vue from 'vue'
import store from '../store/index.js'
import { Modules } from '../constants.js'
const DELAY = 5000

let FN = 'Chat Hub'

let instance = null

class ChatHub {
  constructor() {
    if (instance == null) {
      instance = this
      this.connection = null
    }

    return instance
  }

  setup() {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${process.env.VUE_APP_API_URL}/chat-hub`, {
        accessTokenFactory: () => store.state.auth.accessToken,
      })
      .withAutomaticReconnect([100, 2500, 15000, 60000])
      .configureLogging(signalR.LogLevel.Debug)
      .build()

    // use new Vue instance as an event bus
    const chatHub = new Vue()
    // every component will use this.$chatHub to access the event bus
    Vue.prototype.$chatHub = chatHub
    // Forward server side SignalR events through $chatHub, where components will listen to them
    this.connection.on('SendMessage', message => {
      chatHub.$emit('message-received', message)
    })
    this.connection.on('ReadMessage', receipt => {
      chatHub.$emit('read-receipt-received', receipt)
    })
  }

  async start() {
    if (this.connection == null) {
      ConMsgs.methods.$_console_log(`[${FN}]: Connection is null`)
      return
    }
    if (this.connection.connection.connectionState == 1) {
      ConMsgs.methods.$_console_log(`[${FN}]: Connection is already connected`)
      return
    }
    try {
      let startedPromise = await this.connection.start()
      ConMsgs.methods.$_console_log(`[${FN}]: Connection Started`)
      return startedPromise
    } catch (e) {
      ConMsgs.methods.$_console_error(`[${FN}]: Failed to connect with hub`, e)
      return await new Promise((resolve, reject) =>
        setTimeout(
          async () =>
            await this.start()
              .then(resolve)
              .catch(reject),
          DELAY
        )
      )
    }
  }

  stop() {
    this.connection.off('SendMessage')
    this.connection.off('ReadMessage')
    if (this.connection != null) {
      this.connection
        .stop()
        .then(() => {
          ConMsgs.methods.$_console_log(
            `[${FN}]: Succesfully stopped Chat Hub connection`
          )
          this.connection = null
        })
        .catch(err => ConMsgs.methods.$_console_error(err))
    }
  }
}

export default new ChatHub()

// export default {
//   install(Vue) {
//     const connection = new HubConnectionBuilder()
//       .withUrl(`${process.env.VUE_APP_API_URL}/chat-hub`)
//       .configureLogging(LogLevel.Information)
//       .build()

//     // use new Vue instance as an event bus
//     const chatHub = new Vue()
//     // every component will use this.$chatHub to access the event bus
//     Vue.prototype.$chatHub = chatHub
//     // Forward server side SignalR events through $chatHub, where components will listen to them
//     connection.on('SendMessage', message => {
//       chatHub.$emit('message-received', message)
//     })
//     connection.on('ReadMessage', receipt => {
//       chatHub.$emit('read-receipt-received', receipt)
//     })

//     let startedPromise = null
//     function start() {
//       let isAuthorize = store.state.auth.isAuthorize
//       let modules = store.state.auth.activeModules
//       // Only try to connect if we are authorized and have the chat module is our list of features
//       if (isAuthorize && modules.findIndex(x => x.id === Modules.Chat) > -1) {
//         startedPromise = connection.start().catch(err => {
//           console.error('Failed to connect with hub', err)
//           return new Promise((resolve, reject) =>
//             setTimeout(
//               () =>
//                 start()
//                   .then(resolve)
//                   .catch(reject),
//               DELAY
//             )
//           )
//         })
//         return startedPromise
//       } else {
//         console.log('Chat Hub State')
//         console.log(connection.state)
//         connection.onclose(() => console.log('Closed')) // eat it
//         connection.stop().catch(err => console.error(err))
//         return new Promise((resolve, reject) =>
//           setTimeout(
//             () =>
//               start()
//                 .then(resolve)
//                 .catch(reject),
//             DELAY
//           )
//         )
//       }
//     }
//     connection.onclose(() => {
//       if (connection.status === 0) start()
//     })

//     start()

//     return connection
//   },
// }

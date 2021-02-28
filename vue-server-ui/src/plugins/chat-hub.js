"use strict"

import { HubConnectionBuilder, LogLevel } from '@aspnet/signalr'

export default {
    install(Vue) {
        const connection = new HubConnectionBuilder()
            .withUrl(`${process.env.API_URL}/chat-hub`)
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
            startedPromise = connection.start().catch(err => {
                console.error('Failed to connect with hub', err)
                return new Promise((resolve, reject) =>
                    setTimeout(() => start().then(resolve).catch(reject), 5000))
            })
            return startedPromise
        }
        connection.onclose(() => start())

        start()

        return connection;
    }
}

import { requiresRefresh } from '@/helpers/jwt'
import store from '@/store/index'
import ConMsgs from '@/mixins/console'

const FN = 'WSDispatcher'

let instance = null

class WSDispatcher {
  constructor() {
    if (!instance) {
      instance = this
    }

    // TODO: See if we need to add checks here to not re-initialize
    // these values to their defaults if an instance already exists
    this.queue = []
    this.gettingToken = false

    return instance
  }

  async request(request) {
    ConMsgs.methods.$_console_log(`[${FN}}] request call`, request)
    if (!requiresRefresh(store.state.auth.accessToken)) {
      ConMsgs.methods.$_console_log(`[${FN}] Running request`)
      return await request()
    } else {
      this.queue.push(request)
      return await this.getRefreshToken()
    }
  }

  async processQueue() {
    ConMsgs.methods.$_console_log(`[${FN}] Processing queue...`)
    let promiseList = []
    for (let i = 0; i < this.queue.length; i++) {
      promiseList.push(this.request(this.queue[i]))
    }

    Promise.all(promiseList)
      .then(resp => {})
      .catch(() => {})
      .then(() => {
        // Once all promises are handled clear the promises out of the list
        ConMsgs.methods.$_console_log(`[${FN}}] Emptying out promise queue.`)
        for (let i = 0; i < promiseList.length; i++) {
          this.queue.shift()
        }
        ConMsgs.methods.$_console_log(
          `[${FN}] Removed ${promiseList.length} promises from the queue`
        )
      })
  }

  async getRefreshToken() {
    ConMsgs.methods.$_console_log(`[${FN}] ${this.gettingToken}`)
    if (this.gettingToken === false) {
      this.gettingToken = true
      await store.dispatch('refreshToken')

      this.gettingToken = false
      return await this.processQueue()
    } else {
      ConMsgs.methods.$_console_log(
        `[${FN}] We are already getting a refresh token.`
      )
    }
  }
}

export default new WSDispatcher()

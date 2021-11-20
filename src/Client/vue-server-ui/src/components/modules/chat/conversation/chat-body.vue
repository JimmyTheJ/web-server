<template>
  <div>
    <generic-dialog
      :open="moreInfoDialog"
      title="Message Info"
      :maxWidth="640"
      @dialog-close="moreInfoDialog = false"
    >
      <v-card>
        <v-card-text>
          <v-layout row>
            <v-flex xs4 class="body-1 py-2">Message:</v-flex
            ><v-flex xs8 class="caption py-2">{{ moreInfo.text }}</v-flex>
            <v-flex xs4 class="body-1 py-2">Timestamp:</v-flex
            ><v-flex xs8 class="caption py-2">{{ moreInfo.timestamp }}</v-flex>
            <v-flex xs4 class="body-1 py-2">User:</v-flex
            ><v-flex xs8 class="caption py-2">{{ moreInfo.username }}</v-flex>
          </v-layout>
        </v-card-text>
      </v-card>
    </generic-dialog>

    <div class="chat-body-container" ref="chatBodyContainer">
      <v-row class="px-0">
        <v-col
          cols="12"
          class="py-1"
          v-for="(message, index) in conversation.messages"
          :key="index"
          @mouseover="setMessageHover(message, true)"
          @mouseleave="setMessageHover(message, false)"
        >
          <chat-bubble
            :colorMap="colorMap"
            :message="message"
            :isGroup="isGroupConversation"
            @moreInfo="openMoreInfo"
            @deleteMessage="deleteMessage"
          />
        </v-col>
      </v-row>
    </div>
  </div>
</template>

<script>
import ChatBubble from '../chat-bubble.vue'
import GenericDialog from '@/components/modules/generic-dialog.vue'

import { mapState } from 'vuex'

export default {
  name: 'chat-body',
  components: {
    ChatBubble,
    GenericDialog,
  },
  data() {
    return {
      moreInfoDialog: false,
      moreInfo: {},
      isGroupConversation: false,
      chatWindow: null,
      scrollHeight: 0,
      friend: null,
    }
  },
  props: {
    conversation: {
      type: Object,
      required: true,
    },
    colorMap: {
      type: Object,
      required: true,
    },
    show: {
      type: Boolean,
      required: true,
    },
  },
  computed: {
    ...mapState({
      user: state => state.auth.user,
    }),
  },
  watch: {
    scrollHeight(newValue) {
      if (this.show) {
        // This here is a way to make sure we trigger a read of all messages when we get to the bottom of the scrollbar
        // The plus one at the end is a way to make it compatible with most browsers. For some reason Edge as an example
        // has the scroll height over by about 0.4 pixels when you add the scroll value to the client height of the chat window.
        // It seems like the scroll value can't necessarily get to the maximum value it's supposed to. Look into a safer way to
        // handle this in the future
        if (
          this.chatWindow.scrollHeight <=
          newValue + this.chatWindow.clientHeight + 1
        ) {
          this.$_console_log(
            "Max scroll reached. You're at the bottom, reading all messages."
          )
          this.readAllMessages()
        }
      }
    },
  },
  created() {
    this.$chatHub.$on('message-received', this.onMessageReceived)
    this.$chatHub.$on('read-receipt-received', this.onReadReceiptReceived)
  },
  mounted() {
    this.isGroupConversation = this.conversation.conversationUsers.length > 2
    this.chatWindow = this.$refs.chatBodyContainer
    this.chatWindow.addEventListener('scroll', this.windowScroll)

    this.friend =
      this.conversation.conversationUsers.length == 2
        ? this.conversation.conversationUsers.find(
            x => x.userId !== this.user.id
          )
        : null
  },
  beforeDestroy() {
    this.$chatHub.$off('message-received', this.onMessageReceived)
    this.$chatHub.$off('read-receipt-received', this.onReadReceiptReceived)
    this.chatWindow.removeEventListener('scroll', this.windowScroll)
  },
  methods: {
    windowScroll(event) {
      this.$_console_log('Scroll event', event)
      this.scrollHeight = event.target.scrollTop
    },
    onMessageReceived(message) {
      if (typeof message !== 'object' || message === null) {
        this.$_console_log('OnMessageReceived: Null object returned')
        return
      }

      if (this.conversation.id !== message.conversationId) {
        return
      }

      this.$store
        .dispatch('addChatMessage', {
          conversationId: this.conversation.id,
          message: message,
        })
        .then(() => {
          // Scroll to bottom when active user sends a message. This will ultimately cause all messages to be read,
          // Also scroll to the bottom when there is no scrollbar yet.
          if (
            message.userId === this.user.id ||
            this.chatWindow.clientHeight === this.chatWindow.scrollHeight
          ) {
            this.scrollToBottom()
          }
        })
    },
    onReadReceiptReceived(receipt) {
      this.$_console_log('Read receipt received: ', receipt)

      if (typeof receipt !== 'object' || receipt === null) {
        this.$_console_log('OnReadReceiptReceived: Null object returned')
        return
      }

      if (this.conversation.id !== receipt.message.conversationId) {
        return
      }

      this.$store
        .dispatch('addReadReceipt', {
          conversationId: this.conversation.id,
          receipt: receipt,
        })
        .then(() => {
          this.$_console_log('Receipt successfully added to message. Show')
        })
    },
    setMessageHover(message, on) {
      if (on) {
        this.$store.dispatch('setMessageHover', {
          messageId: message.id,
          conversationId: message.conversationId,
          on: true,
        })
      } else {
        this.$store.dispatch('setMessageHover', {
          messageId: message.id,
          conversationId: message.conversationId,
          on: false,
        })
      }
    },
    openMoreInfo(message) {
      this.moreInfo.text = message.text
      this.moreInfo.timestamp = message.timestamp
      this.moreInfo.username = message.userId

      this.moreInfoDialog = true
    },
    deleteMessage(id) {
      this.$store.dispatch('deleteChatMessage', {
        conversationId: this.conversation.id,
        messageId: id,
      })
    },
    canDeleteMessage(message) {
      if (this.isMessageDeletable) {
        if (message.userId === this.user.id) {
          return true
        }
      }

      return false
    },
    scrollToBottom() {
      this.$nextTick(() => {
        this.chatWindow.scrollTo(0, this.chatWindow.scrollHeight)
        this.readAllMessages()
      })
    },
    // scrollToLastReadMessage() {
    //   if (!this.show) {
    //     return
    //   }

    //   let lastMessage = this.conversation.messages.find(
    //     x =>
    //       this.user.id !== x.userId &&
    //       (x.readReceipts.length === 0 ||
    //         typeof x.readReceipts.find(y => y.userId !== this.user.id) !==
    //           'undefined')
    //   )

    //   if (typeof lastMessage !== 'undefined') {
    //     this.$nextTick(() => {
    //       const ids = document.getElementsByClassName('bubble-id')

    //       let correctElement = null
    //       let height = 0
    //       for (let i = 0; i < ids.length; i++) {
    //         height += ids[i].parentNode.offsetHeight
    //         if (parseInt(ids[i].innerText, 10) === lastMessage.id) {
    //           correctElement = ids[i]
    //           break
    //         }
    //       }

    //       if (correctElement !== null) {
    //         this.chatWindow.scrollTo(0, height - this.chatWindow.offsetTop)

    //         this.$store.dispatch('highlightMessage', {
    //           messageId: lastMessage.id,
    //           conversationId: lastMessage.conversationId,
    //           on: true,
    //         })
    //         setTimeout(() => {
    //           this.$store.dispatch('highlightMessage', {
    //             messageId: lastMessage.id,
    //             conversationId: lastMessage.conversationId,
    //             on: false,
    //           })
    //         }, 3000)
    //       }
    //     })
    //   } else {
    //     this.$_console_log('ScrollToLastReadMessage: All messages are read.')
    //     this.scrollToBottom()
    //   }
    // },
    readAllMessages() {
      if (!Array.isArray(this.conversation.messages)) {
        return
      }

      const newMessages = this.conversation.messages.filter(
        x =>
          (!Array.isArray(x.readReceipts) || x.readReceipts.length === 0) &&
          x.userId !== this.user.id
      )
      if (!Array.isArray(newMessages) || newMessages.length === 0) {
        this.$_console_log('ReadAllMessages: NewMessages is null or empty')
        return
      }

      this.$_console_log(newMessages)
      this.$store.dispatch('readChatMessageList', {
        conversationId: this.conversation.id,
        messageIds: newMessages.map(x => x.id),
      })
    },
    updateReadReceiptMarker() {
      // If 2 person converation, show where the user last read to
      if (this.conversation.conversationUsers.length === 2) {
        let i
        let foundMessage = false

        if (this.othersLastReadMessage > -1) i = this.othersLastReadMessage
        else i = 0

        for (i; i < this.conversation.messages.length; i++) {
          if (this.conversation.messages[i].userId !== this.user.id) continue

          if (
            this.conversation.messages[i].readReceipts !== null &&
            this.conversation.messages[i].readReceipts.length === 0
          ) {
            break
          } else {
            foundMessage = true
          }
        }
        //let lastMessage = this.conversation.messages.find(x => this.user.id === x.userId &&
        //    (x.readReceipts.length === 0 || typeof x.readReceipts.find(y => y.userId !== this.user.id) === 'undefined'));

        if (foundMessage === true) {
          while (i > 1) {
            if (this.conversation.messages[--i].userId === this.user.id) {
              break
            }
          }
          this.othersLastReadMessage = i
        }
      } else {
        // If user count is > 2 than we need to find a solution on how to show the read receipt icon of the user
      }
    },
  },
}
</script>

<style>
.chat-body-container {
  overflow-y: scroll;
  overflow-x: hidden;
  height: 70vh;
  max-height: 80vh;
}
</style>

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
            <v-flex xs4 class="body-1 py-2">Read By:</v-flex
            ><v-flex xs8 class="caption py-2">{{ moreInfo.readByList }}</v-flex>
          </v-layout>
        </v-card-text>
      </v-card>
    </generic-dialog>

    <div id="chat-body-container" ref="chatBodyContainer">
      <v-row class="px-0">
        <v-col
          cols="12"
          class="py-1 chat-bubble-object"
          v-for="(message, index) in conversation.messages"
          :key="index"
        >
          <chat-bubble
            :message="message"
            :isGroup="isGroupConversation"
            :color="getColor(message.userId)"
            @moreInfo="openMoreInfo"
            @deleteMessage="deleteMessage"
            :id="'bubble-' + message.id"
          />
        </v-col>
      </v-row>
    </div>
  </div>
</template>

<script>
import ChatBubble from '../chat-bubble.vue'
import GenericDialog from '@/components/modules/generic-dialog.vue'
import ChatHub from '@/plugins/chat-hub'

import { mapState } from 'vuex'
import { Modules, NotificationTypes } from '@/constants'

const heightPrefix = 'height: '

function isInViewport(element) {
  const rect = element.getBoundingClientRect()
  return (
    rect.top >= 0 &&
    rect.left >= 0 &&
    rect.bottom <=
      (window.innerHeight || document.documentElement.clientHeight) &&
    rect.right <= (window.innerWidth || document.documentElement.clientWidth)
  )
}

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
      readingInProgress: false,
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
    textFieldHeight: {
      type: Number,
      required: false,
    },
  },
  computed: {
    ...mapState({
      modules: state => state.auth.activeModules,
      user: state => state.auth.user,
    }),
  },
  watch: {
    scrollHeight(newValue) {
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
    },
    'conversation.loaded': {
      handler(newValue) {
        if (newValue) {
          this.$nextTick(() => {
            this.scrollToLastReadMessage()
            this.$nextTick(() => {
              // Trick to get the conversation to read all messages if there is no scroll bar
              if (this.chatWindow.scrollTop === 0) {
                this.$_console_log(
                  'Reading all chat message messages on Chat Body Mount because there is no scrollbar'
                )
                this.readAllMessages()
              }
            })
          })
        }
      },
      deep: true,
    },
    textFieldHeight(newValue) {
      this.$_console_log('Text Field Height: ' + newValue)
      document
        .getElementById('chat-body-container')
        .style.setProperty(
          'height',
          `calc(100vh - 64px - 64px - 54px - ${newValue}px - 5px)`
        )
    },
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

    if (this.modules.findIndex(x => x.id === Modules.Chat) > -1) {
      if (ChatHub.connection != null) {
        this.$chatHub.$on('message-received', this.onMessageReceived)
        this.$chatHub.$on('read-receipt-received', this.onReadReceiptReceived)
      }
    }
  },
  beforeDestroy() {
    if (ChatHub.connection != null) {
      this.$chatHub.$off('message-received', this.onMessageReceived)
      this.$chatHub.$off('read-receipt-received', this.onReadReceiptReceived)
    }
  },
  methods: {
    windowScroll(event) {
      //this.$_console_log('Scroll event', event)
      this.scrollHeight = event.target.scrollTop

      if (event.target.scrollTop < 1) {
        this.$nextTick(() => {
          if (this.conversation.allMsgs === true) {
            this.$_console_log(
              'No more messages in this conversation. No need to call API to get more'
            )
          } else {
            const chatContainer = document.getElementById('chat-body-container')
            const bubbles = document.getElementsByClassName(
              'chat-bubble-object'
            )

            // If no messages exist, get the last X, if we have messages already, then get the next X from the list
            const msgId =
              this.conversation.messages.length === 0
                ? -2
                : this.conversation.messages[0].id
            this.$store
              .dispatch('getMessagesForConversation', {
                conversationId: this.conversation.id,
                msgId: msgId,
              })
              .then(() => {
                document.getElementById(`bubble-${msgId}`).scrollIntoView()
              })
          }
        })
      }
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
          // Scroll to bottom when active user sends a message or when the last message
          // is in the viewport when a message from another user comes in
          if (message.userId === this.user.id) {
            this.scrollToBottom()
          } else {
            let lastBubble = this.conversation.messages[
              this.conversation.messages.length - 1
            ]
            let msgInViewport = isInViewport(
              document.getElementById(`bubble-${lastBubble.id}`)
            )

            if (msgInViewport) {
              this.scrollToBottom()
            }
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
    openMoreInfo(message) {
      this.moreInfo.text = message.text
      this.moreInfo.timestamp = message.timestamp
      this.moreInfo.username = message.userId
      this.moreInfo.readByList = ''
      if (Array.isArray(message.readReceipts)) {
        for (let i = 0; i < message.readReceipts.length; i++) {
          this.moreInfo.readByList += message.readReceipts[i].userId
          if (i + 1 < message.readReceipts.length) {
            this.moreInfo.readByList += ', '
          }
        }
      }

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
      this.$_console_log('Scroll to Bottom activated')
      this.$nextTick(() => {
        this.chatWindow.scrollTo(0, this.chatWindow.scrollHeight)
        this.readAllMessages()
      })
    },
    scrollToLastReadMessage() {
      let lastMessage = this.conversation.messages.find(
        x =>
          this.user.id !== x.userId &&
          (x.readReceipts.length === 0 ||
            typeof x.readReceipts.find(y => y.userId !== this.user.id) !==
              'undefined')
      )

      if (typeof lastMessage !== 'undefined') {
        this.$nextTick(() => {
          document.getElementById(`bubble-${lastMessage.id}`).scrollIntoView()
        })
      } else {
        this.$_console_log('ScrollToLastReadMessage: All messages are read.')
        this.scrollToBottom()
      }
    },
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
      if (!this.readingInProgress) {
        this.readingInProgress = true
        this.$store
          .dispatch('readChatMessageList', {
            conversationId: this.conversation.id,
            messageIds: newMessages.map(x => x.id),
          })
          .then(() => {
            this.$store.dispatch('clearSpecificNotification', {
              type: NotificationTypes.Chat,
              value: this.conversation.id,
            })
          })
          .finally(() => {
            this.readingInProgress = false
          })
      }
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
    getColor(id) {
      if (id === this.user.id) {
        return 'grey'
      } else {
        // const color = this.$store.getters.getConversationUserColor(
        //   this.message.conversation.id,
        //   this.message.userId
        // )

        let color = this.colorMap[id]
        if (typeof color === 'undefined' || color === null || color === '') {
          return 'indigo'
        }

        return color
      }
    },
  },
}
</script>

<style>
#chat-body-container {
  overflow-y: scroll;
  overflow-x: hidden;
  height: calc(100vh - 64px - 64px - 86px - 5px);
}
</style>

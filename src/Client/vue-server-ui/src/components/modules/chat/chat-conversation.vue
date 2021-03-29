<template>
  <div v-show="show">
    <generic-dialog
      :open="deleteConversationDialog"
      title="Delete Conversation"
      :maxWidth="400"
      :hideClose="true"
      @dialog-close="deleteConversationDialog = false"
    >
      <v-card>
        <v-card-text>
          Are you sure you want to delete this conversation ?
        </v-card-text>
        <v-card-actions>
          <v-btn @click="deleteConversation">Yes</v-btn>
          <v-btn @click="deleteConversationDialog = false">No</v-btn>
        </v-card-actions>
      </v-card>
    </generic-dialog>

    <generic-dialog
      :open="changeUserColorDialog"
      title="Change User Color"
      :maxWidth="260"
      :hideClose="true"
      @dialog-close="deleteConversationDialog = false"
    >
      <v-card>
        <v-card-text class="text-center">
          <v-avatar
            v-for="(color, i) in colorList"
            :key="i"
            :color="color"
            size="48"
            class="ma-2"
            @click="changeUserColor(i)"
          ></v-avatar>
        </v-card-text>
      </v-card>
    </generic-dialog>

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

    <div id="chat-container">
      <v-toolbar>
        <v-btn text @click="goBack" v-show="mobile">
          <fa-icon icon="reply"></fa-icon>
        </v-btn>
        <v-btn text @click="editingTitle = !editingTitle">
          <v-icon>mdi-account-edit</v-icon>
        </v-btn>
        <v-toolbar-title v-if="!editingTitle">
          {{ conversation.title }}
        </v-toolbar-title>
        <v-toolbar-title v-else style="width: 100%" class="ml-3 pl-2">
          <div style="display: flex; flex-direction: row">
            <v-text-field
              v-model="newTitle"
              @keyup.enter.prevent="updateTitle"
              autofocus
            ></v-text-field>
            <v-btn v-if="!mobile" @click="updateTitle">SAVE</v-btn>
          </div>
        </v-toolbar-title>
        <v-spacer></v-spacer>
        <v-menu
          :close-on-content-click="true"
          :nudge-width="200"
          v-model="menu"
          offset-x
        >
          <template v-slot:activator="{ on }">
            <v-btn v-on="on" icon
              ><fa-icon icon="ellipsis-v" size="2x"></fa-icon
            ></v-btn>
          </template>
          <v-card>
            <v-list>
              <v-list-item
                v-if="canChangeUserColor"
                @click="changeUserColorDialog = true"
              >
                <v-list-item-title class="ml-2">
                  Change user color
                </v-list-item-title>
              </v-list-item>
              <v-list-item
                v-if="isConversationDeletable"
                @click="deleteConversationDialog = true"
              >
                <v-icon>mdi-delete</v-icon>
                <v-list-item-title class="ml-2"
                  >Delete Conversation</v-list-item-title
                >
              </v-list-item>
            </v-list>
          </v-card>
        </v-menu>
      </v-toolbar>

      <div
        class="chat-body-container"
        :style="{ height: bodyContainerHeight + 'px' }"
        ref="chatBodyContainer"
      >
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
              :currentTime="time"
              :owner="isOwner(message)"
              :isGroup="isGroupConversation"
              @moreInfo="openMoreInfo"
              @deleteMessage="deleteMessage"
            >
              <template v-slot:lastReadIcon v-if="isLastReadMessage(message)">
                <chat-avatar
                  :conversation="conversation"
                  size="16"
                ></chat-avatar>
              </template>
            </chat-bubble>
          </v-col>
        </v-row>
      </div>
      <div class="chat-message-textfield" ref="chatTextfield">
        <v-text-field
          v-model="newMessage.text"
          autofocus
          label="Message"
          ref="newMessage"
          @keyup.enter.prevent="sendMessage"
          class="pa-0"
        >
          <template v-slot:append-outer>
            <v-btn icon text @click="sendMessage"
              ><fa-icon size="lg" icon="paper-plane"></fa-icon
            ></v-btn>
          </template>
        </v-text-field>
      </div>
    </div>
  </div>
</template>

<script>
import ChatBubble from './chat-bubble.vue'
import ChatAvatar from './chat-avatar.vue'
import GenericDialog from '../generic-dialog.vue'

import service from '../../../services/chat'

import { mapState } from 'vuex'

export default {
  name: 'chat-conversation',
  data() {
    return {
      newMessage: null,
      changeUserColorDialog: false,
      deleteConversationDialog: false,
      moreInfoDialog: false,
      editingTitle: false,
      menu: false,
      newTitle: null,
      moreInfo: {},
      chatWindow: null,
      scrollHeight: 0,
      bodyContainerHeight: 0,
      othersLastReadMessage: -1,
      colorList: [
        'indigo',
        'purple',
        'teal',
        'cyan',
        'pink',
        'green',
        'orange',
        'blue-grey',
        'brown',
      ],
    }
  },
  components: {
    ChatBubble,
    ChatAvatar,
    GenericDialog,
  },
  props: {
    conversation: {
      type: Object,
      required: true,
    },
    time: {
      type: Number,
      required: true,
    },
    show: {
      type: Boolean,
      required: true,
    },
    mobile: {
      type: Boolean,
      required: true,
    },
  },
  created() {
    this.newMessage = { text: '' }
    this.$chatHub.$on('message-received', this.onMessageReceived)
    this.$chatHub.$on('read-receipt-received', this.onReadReceiptReceived)
    window.addEventListener('resize', this.resizeWindow)
  },
  mounted() {
    this.chatWindow = this.$refs.chatBodyContainer
    this.chatWindow.addEventListener('scroll', this.windowScroll)
  },
  beforeDestroy() {
    this.$chatHub.$off('message-received', this.onMessageReceived)
    this.$chatHub.$off('read-receipt-received', this.onReadReceiptReceived)
    this.chatWindow.removeEventListener('scroll', this.windowScroll)
    window.removeEventListener('resize', this.resizeWindow)
  },
  computed: {
    ...mapState({
      user: (state) => state.auth.user,
      activeModules: (state) => state.auth.activeModules,
    }),
    isConversationDeletable() {
      const chatModule = this.activeModules.find((x) => x.id === 'chat')
      if (typeof chatModule !== 'undefined') {
        const feature = chatModule.userModuleFeatures.find(
          (x) => x.moduleFeatureId === 'chat-delete-conversation'
        )
        if (typeof feature !== 'undefined') {
          return true
        }
      }

      return false
    },
    canChangeUserColor() {
      return this.conversation.conversationUsers.length == 2
    },
    colorMap() {
      let map = {}
      this.conversation.conversationUsers.forEach((element, index) => {
        map[element.userId] = element.color
      })

      return map
    },
    isGroupConversation() {
      return this.conversation.conversationUsers.length > 2
    }
  },
  watch: {
    editingTitle(newValue) {
      if (newValue === false) {
        this.newTitle = null
      } else {
        this.newTitle = this.conversation.title
      }
    },
    show(newValue) {
      if (newValue === true) {
        this.$store
          .dispatch('getMessagesForConversation', this.conversation.id)
          .then((resp) => {
            this.updateContainerHeight(true)
            this.updateReadReceiptMarker()
          })
      }
    },
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
  methods: {
    windowScroll(event) {
      this.$_console_log('Scroll event', event)
      this.scrollHeight = event.target.scrollTop
    },
    resizeWindow(event) {
      this.updateContainerHeight()
    },
    async updateContainerHeight(attemptScroll) {
      this.$nextTick(() => {
        let bodyContainer = this.$refs.chatBodyContainer
        let chatMessageTextfield = this.$refs.chatTextfield
        this.bodyContainerHeight =
          window.innerHeight -
          bodyContainer.offsetTop -
          chatMessageTextfield.clientHeight -
          4

        this.$_console_log(
          'Containers',
          bodyContainer,
          chatMessageTextfield,
          'Window Height',
          window.innerHeight,
          this.bodyContainerHeight
        )

        if (attemptScroll === true) {
          if (
            Array.isArray(this.conversation.messages) &&
            this.conversation.messages.length > 0
          ) {
            this.$_console_log(
              'Heights: ',
              this.bodyContainerHeight,
              this.chatWindow.scrollHeight
            )
            if (this.bodyContainerHeight >= this.chatWindow.scrollHeight) {
              this.$_console_log(
                'Show watcher: No scrollbar exists. Reading all messages'
              )
              this.scrollToBottom()
            } else {
              this.$_console_log(
                'Show watcher: Message length is greater than 0. Scrolling to last read message'
              )
              this.scrollToLastReadMessage()
            }
          }
        }
      })
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
          if (this.show) {
            // Scroll to bottom when active user sends a message. This will ultimately cause all messages to be read,
            // Also scroll to the bottom when there is no scrollbar yet.
            if (
              message.userId === this.user.id ||
              this.chatWindow.clientHeight === this.chatWindow.scrollHeight
            ) {
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
          this.$_console_log(
            'Receipt successfully added to message. Show',
            this.show
          )
          if (this.show) {
            this.updateReadReceiptMarker()
          }
        })
    },
    goBack() {
      this.$emit('goBack', true)
    },
    async sendMessage() {
      this.newMessage.id = 0
      this.newMessage.userId = this.user.id
      this.newMessage.conversationId = this.conversation.id

      await service.sendMessage(this.newMessage)

      this.newMessage = { text: '' }
      this.$refs.newMessage.focus()
    },
    isOwner(message) {
      if (this.user.id === message.userId) {
        return true
      } else {
        return false
      }
    },
    getTextPosition(message) {
      if (message.userId === this.user.id) {
        return 'text-right'
      } else {
        return 'text-left'
      }
    },
    deleteConversation() {
      this.$store
        .dispatch('deleteConversation', this.conversation.id)
        .then(() => {})
        .catch(() => {})
        .then(() => (this.deleteConversationDialog = false))
    },
    updateTitle() {
      const title = this.newTitle

      if (/^\s*$/.test(title)) {
        this.editingTitle = false
        return
      }

      this.$store
        .dispatch('updateConversationTitle', {
          conversationId: this.conversation.id,
          title: title,
        })
        .then(() => {})
        .catch(() => {})
        .then(() => (this.editingTitle = false))
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
    scrollToLastReadMessage() {
      if (!this.show) {
        return
      }

      let lastMessage = this.conversation.messages.find(
        (x) =>
          this.user.id !== x.userId &&
          (x.readReceipts.length === 0 ||
            typeof x.readReceipts.find((y) => y.userId !== this.user.id) !==
              'undefined')
      )

      if (typeof lastMessage !== 'undefined') {
        this.$nextTick(() => {
          const ids = document.getElementsByClassName('bubble-id')

          let correctElement = null
          let height = 0
          for (let i = 0; i < ids.length; i++) {
            height += ids[i].parentNode.offsetHeight
            if (parseInt(ids[i].innerText, 10) === lastMessage.id) {
              correctElement = ids[i]
              break
            }
          }

          if (correctElement !== null) {
            this.chatWindow.scrollTo(0, height - this.chatWindow.offsetTop)

            this.$store.dispatch('highlightMessage', {
              messageId: lastMessage.id,
              conversationId: lastMessage.conversationId,
              on: true,
            })
            setTimeout(() => {
              this.$store.dispatch('highlightMessage', {
                messageId: lastMessage.id,
                conversationId: lastMessage.conversationId,
                on: false,
              })
            }, 3000)
          }
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
        (x) =>
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
        messageIds: newMessages.map((x) => x.id),
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
    isLastReadMessage(message) {
      if (
        this.othersLastReadMessage > -1 &&
        message.id === this.conversation.messages[this.othersLastReadMessage].id
      )
        return true

      return false
    },
    updateReadReceiptMarker() {
      // If 2 person converation, show where the usre last read to
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
    changeUserColor(color) {
      this.$_console_log(`Selected color is: ${color}`)
      const otherUserIndex = this.conversation.conversationUsers.findIndex(
        (x) => x.userId !== this.user.id
      )
      if (otherUserIndex < 0) {
        return this.$_console_log("Couldn't find other user")
      }

      this.$store
        .dispatch('changeConversationUserColor', {
          conversationId: this.conversation.id,
          userId: this.conversation.conversationUsers[otherUserIndex].userId,
          colorId: color,
        })
        .then(() => {})
        .catch(() => {})
        .then(() => {
          this.changeUserColorDialog = false
        })
    },
  },
}

function getPosition(element) {
  var xPosition = 0
  var yPosition = 0

  while (element) {
    xPosition += element.offsetLeft - element.scrollLeft + element.clientLeft
    yPosition += element.offsetTop - element.scrollTop + element.clientTop
    element = element.offsetParent
  }

  return { x: xPosition, y: yPosition }
}
</script>

<style>
.chat-body-container {
  overflow-y: scroll;
  overflow-x: hidden;
}
</style>

<template>
  <div class="chat-container">
    <v-hover v-slot="{ hover }" open-delay="200">
      <div :class="['bubble-container', getFlexDirection]">
        <v-menu absolute offset-y>
          <template v-slot:activator="{ on }">
            <div v-on="on" class="order-2">
              <span v-if="hover" class="px-1">...</span>
            </div>
          </template>
          <v-list>
            <v-list-item @click="moreInfo">
              <v-list-item-title> More info </v-list-item-title>
            </v-list-item>
            <v-list-item v-if="isMessageDeletable" @click="deleteMessage">
              <v-list-item-title> Delete Message </v-list-item-title>
            </v-list-item>
          </v-list>
        </v-menu>

        <div
          v-if="isGroup && user.id !== message.userId"
          style="align-self: end;"
        >
          <chat-avatar
            :avatar="
              typeof userMap[message.userId].avatar !== 'undefined'
                ? userMap[message.userId].avatar
                : null
            "
            :text="userMap[message.userId].displayName.charAt(0)"
            :color="color"
            size="32"
          />
        </div>

        <div
          :class="['bubble-body-container', color, 'pa-2', 'order-1']"
          @click="readMessage()"
        >
          <div
            class="text-header-1 font-weight-bold"
            v-if="isGroup && user.id !== message.userId"
          >
            {{ userMap[message.userId].displayName }}
          </div>
          <template v-for="(line, i) in textLines">
            <div class="text-body-1 bubble-wrap" :key="i">{{ line }}</div>
          </template>
          <div class="text-caption text-right">{{ time }}</div>
          <div class="bubble-id" style="display: none">{{ message.id }}</div>
        </div>
      </div>
    </v-hover>

    <div :class="['bubble-last-read', getFlexDirection, 'pr-2']">
      <chat-avatar
        v-if="isLastReadMessage(message)"
        :avatar="getUserAvatarFromMessage(message)"
        :text="getUserAvatarTextFromMessage(message)"
        :color="color"
        size="16"
      ></chat-avatar>
    </div>
  </div>
</template>

<script>
import { mapState } from 'vuex'
import ChatAvatar from './chat-avatar.vue'

export default {
  name: 'chat-bubble',
  components: {
    'chat-avatar': ChatAvatar,
  },
  data() {
    return {
      time: '0s',
      textLines: [],
    }
  },
  props: {
    color: {
      type: String,
      required: true,
    },
    message: {
      type: Object,
      required: true,
    },
    lastMessage: {
      type: Boolean,
      default: false,
    },
    isGroup: {
      type: Boolean,
      default: false,
    },
  },
  computed: {
    ...mapState({
      user: state => state.auth.user,
      userMap: state => state.user.userMap,
      activeModules: state => state.module.activeModules,
      currentTime: state => state.general.currentTime,
    }),
    isMessageDeletable() {
      if (this.owner === true) {
        const chatModule = this.activeModules.find(x => x.id === 'chat')
        if (typeof chatModule !== 'undefined') {
          const feature = chatModule.userModuleFeatures.find(
            x => x.moduleFeatureId === 'chat-delete-message'
          )
          if (typeof feature !== 'undefined') {
            return true
          }
        }
      }

      return false
    },
    getFlexDirection() {
      if (this.owner) {
        return 'flex-right'
      } else {
        return 'flex-left'
      }
    },
    owner() {
      if (this.user.id === this.message.userId) {
        return true
      } else {
        return false
      }
    },
  },
  watch: {
    // message: {
    //   handler(newValue) {
    //     if (typeof newValue !== 'undefined' && newValue.highlighted === true) {
    //       this.$_console_log('Message.Highlighted watcher: highlight = true')
    //       setTimeout(() => {
    //         this.$store.dispatch('highlightMessage', {
    //           messageId: newValue.id,
    //           conversationId: newValue.conversationId,
    //           on: false,
    //         })
    //       }, 6000)
    //     }
    //   },
    //   deep: true,
    // },
    currentTime: {
      handler(newValue) {
        let t = this.timeSince(newValue)

        if (this.time !== t) {
          this.time = t
        }
      },
    },
  },
  created() {
    this.time = this.timeSince(this.currentTime)

    this.setLines()
  },
  methods: {
    deleteMessage() {
      this.$emit('deleteMessage', this.message.id)
      this.optionDialog = false
    },
    moreInfo() {
      this.$emit('moreInfo', this.message)
      this.optionDialog = false
    },
    setLines() {
      let line = ''
      for (let i = 0; i < this.message.text.length; i++) {
        if (
          this.message.text[i] === '\r' &&
          i + 1 < this.message.text.length &&
          this.message.text[i + 1] === '\n'
        ) {
          if (line === '') this.textLines.push(' ')
          else this.textLines.push(line)
          line = ''
          i++
          continue
        } else if (this.message.text[i] === '\n') {
          if (line === '') this.textLines.push(' ')
          else this.textLines.push(line)

          line = ''
          continue
        }

        line += this.message.text[i]
      }

      if (line !== '') {
        this.textLines.push(line)
      }
    },
    isMessageReadable() {
      // Not self
      if (this.user.id !== this.message.userId) {
        // No array or array length is 0
        if (
          !Array.isArray(this.message.readReceipts) ||
          (Array.isArray(this.message.readReceipts) &&
            this.message.readReceipts.length === 0)
        ) {
          return true
        }
        // No read receipts from self
        else if (
          typeof this.message.readReceipts.find(
            x => x.userId === this.user.id
          ) === 'undefined'
        ) {
          return true
        }
      }

      return false
    },
    readMessage() {
      if (!this.isMessageReadable()) {
        this.$_console_log('ReadMessage: Message is not readable')
        return
      }

      this.$store.dispatch('readChatMessage', {
        conversationId: this.message.conversationId,
        messageId: this.message.id,
      })
    },
    isLastReadMessage() {
      if (
        this.othersLastReadMessage > -1 &&
        message.id === this.conversation.messages[this.othersLastReadMessage].id
      )
        return true

      return false
    },
    getUserAvatarFromMessage() {
      if (this.isGroupConversation) {
        // TODO: Handle how to show icons at bottom of messages for group chats
      } else if (this.friend !== null) {
        if (
          typeof this.userMap[this.friend.userId] !== 'undefined' &&
          typeof this.userMap[this.friend.userId].avatar !== 'undefined'
        )
          return this.userMap[this.friend.userId].avatar
      }

      return null
    },
    getUserAvatarTextFromMessage() {
      if (this.isGroupConversation) {
        // TODO: Handle how to show icons at bottom of messages for group chats
      } else if (this.friend !== null) {
        if (
          typeof this.userMap[this.friend.userId] !== 'undefined' &&
          typeof this.userMap[this.friend.userId].avatar === 'undefined'
        )
          return this.userMap[this.friend.userId].displayName.charAt(0)
      }

      return null
    },
    timeSince(t) {
      if (typeof t !== 'number' || typeof this.message.timestamp !== 'number') {
        return '0s'
      }

      let seconds = t - this.message.timestamp
      if (seconds < 0) {
        return '0s'
      }
      if (seconds < 60) {
        return `${Math.trunc(seconds)}s`
      }

      let minutes = seconds / 60
      if (minutes < 60) {
        return `${Math.trunc(minutes)}m`
      }

      let hours = minutes / 60
      if (hours < 24) {
        return `${Math.trunc(hours)}h`
      }

      let days = hours / 24
      return `${Math.trunc(days)}d`
    },
  },
}
</script>

<style scoped>
.chat-container {
  display: flex;
  flex-direction: column;
}

.bubble-container {
  display: flex;
  flex-direction: row;
  max-width: 100%;
}

.bubble-body-container {
  border: 1px solid gray;
  border-radius: 15px;
  display: flex;
  flex-direction: column;
  max-width: 70%;
}

.bubble-last-read {
  display: flex;
  flex-direction: row;
}

.flex-left {
  flex-direction: row;
}

.flex-right {
  flex-direction: row-reverse;
}

.order-1 {
  order: 1;
}

.order-2 {
  order: 2;
}

.green {
  background-color: green;
}

.blue {
  background-color: blue;
}

.highlight {
  /* Get this to shift the color somehow */
  background-color: aquamarine;
}
</style>

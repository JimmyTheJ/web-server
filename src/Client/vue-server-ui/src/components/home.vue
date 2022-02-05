<template>
  <div>
    <main-menu :source="`home`"></main-menu>
    <router-view :key="$route.fullPath"></router-view>
    <notification-bar></notification-bar>
  </div>
</template>

<script>
import Menu from '@/components/modules/menu'
import NotificationBar from '@/components/modules/notification-bar'

import { mapState } from 'vuex'
import { NotificationActions, NotificationTypes } from '@/constants'

import ChatHub from '@/plugins/chat-hub'
import { Modules } from '@/constants'

export default {
  data() {
    return {}
  },
  components: {
    'main-menu': Menu,
    'notification-bar': NotificationBar,
  },
  computed: {
    ...mapState({
      modules: state => state.auth.activeModules,
      user: state => state.auth.user,
      conversations: state => state.chat.conversations,
    }),
  },
  created() {
    if (!this.$store.state.auth.isAuthorize) this.$router.push('/')
    if (this.user.changePassword) this.$router.push({ name: 'profile' })
    if (this.$route.fullPath === '/home') this.$router.push({ name: 'start' })

    this.getUserConversations()
  },
  async mounted() {
    if (this.modules.findIndex(x => x.id === Modules.Chat) > -1) {
      ChatHub.setup()
      await ChatHub.start()

      this.$chatHub.$on('message-received', this.onMessageReceived)
    }
  },
  beforeDestroy() {
    if (ChatHub.connection != null)
      this.$chatHub.$off('message-received', this.onMessageReceived)
  },
  methods: {
    onMessageReceived(message) {
      this.$_console_log('Ding ding ding. Message received', message)
      if (typeof message !== 'object' || message === null) {
        return
      }

      let conversation = this.conversations.find(
        x => x.id === message.conversationId
      )
      if (
        message.userId === this.user.id ||
        typeof conversation === 'undefined' ||
        !Array.isArray(conversation.conversationUsers) ||
        typeof conversation.conversationUsers.find(
          x => x.userId === this.user.id
        ) === 'undefined'
      ) {
        return
      }

      // Increment unread messages for this conversation as the message came from another user that was a member of your conversation
      this.$store.dispatch(
        'incrementConversationUnreadMessageCount',
        conversation.id
      )
      this.$store.dispatch('pushNotification', {
        text: `You have new messages from ${conversation.title}`,
        action: NotificationActions.Info,
        group: {
          type: NotificationTypes.Chat,
          value: conversation.id,
        },
      })
    },
    async getUserConversations() {
      // Get all conversations for this user
      await this.$store.dispatch('getAllConversationsForUser').then(resp1 => {
        // Get all new messages for these conversations
        this.$store
          .dispatch('getNewConversationNotifications')
          .then(resp2 => {
            this.$_console_log('New message notifications:', resp2.data)
            this.buildNotificationList(resp2.data)
          })
          .catch(() => this.$_console_log('No new message notifications'))
      })
    },
    buildNotificationList(data) {
      if (!Array.isArray(data) || data.length === 0) {
        return
      }

      data.forEach(value => {
        if (Array.isArray(value.messages) && value.messages.length > 0) {
          // There are new messages
          const obj = {
            text: `You have new messages from ${value.title}`,
            action: NotificationActions.Info,
            group: {
              type: NotificationTypes.Chat,
              value: value.id,
            },
          }

          this.$store.dispatch('pushNotification', obj)
        }
      })
    },
  },
}
</script>

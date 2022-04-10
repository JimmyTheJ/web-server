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

import Auth from '@/mixins/authentication'

export default {
  data() {
    return {}
  },
  components: {
    'main-menu': Menu,
    'notification-bar': NotificationBar,
  },
  mixins: [Auth],
  computed: {
    ...mapState({
      modules: state => state.module.activeModules,
      user: state => state.auth.user,
      conversations: state => state.chat.conversations,
      userMap: state => state.user.userMap,
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
      let userMapKeys = Object.keys(this.userMap)
      if (userMapKeys.length === 0) {
        this.$store.dispatch('getUsersMap')
      }

      if (ChatHub.connection == null) {
        ChatHub.setup()
        await ChatHub.start()

        this.$chatHub.$on('message-received', this.onMessageReceived)
        this.$chatHub.$on('conversation-created', this.onCreateConvoReceived)
        this.$chatHub.$on('conversation-deleted', this.onDeleteConvoReceived)
      }
    }
  },
  beforeDestroy() {
    if (ChatHub.connection != null) {
      this.$chatHub.$off('message-received', this.onMessageReceived)
      this.$chatHub.$off('conversation-created', this.onCreateConvoReceived)
      this.$chatHub.$off('conversation-deleted', this.onDeleteConvoReceived)
    }
  },
  methods: {
    onCreateConvoReceived(conversation) {
      this.$_console_log('New Conversation received: ', conversation)
      this.$store
        .dispatch('receiveNewConversation', conversation)
        .then(resp => {})
    },
    onDeleteConvoReceived(conversationId) {
      this.$_console_log('Delete Conversation received: ', conversationId)
      this.$store
        .dispatch('receiveDeleteConversation', conversationId)
        .then(resp => {})
    },
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

<template>
  <div>
    <v-badge
      :content="conversation.unreadMessages"
      v-if="conversation.unreadMessages > 0"
    >
      <chat-avatar :conversation="conversation" />
    </v-badge>
    <chat-avatar :conversation="conversation" v-else />
  </div>
</template>

<script>
import { mapState } from 'vuex'
import ChatAvatar from './chat-avatar.vue'

export default {
  name: 'chat-badge',
  data() {
    return {}
  },
  components: {
    'chat-avatar': ChatAvatar,
  },
  props: {
    conversation: {
      type: Object,
      required: true,
    },
  },
  computed: {
    ...mapState({
      user: (state) => state.auth.user,
      userMap: (state) => state.auth.userMap,
    }),
  },
  methods: {
    friendHasAvatar(conversation) {
      if (
        typeof this.userMap !== 'object' ||
        typeof conversation !== 'object' ||
        conversation === null ||
        !Array.isArray(conversation.conversationUsers)
      ) {
        return false
      }

      // For one on one conversation use the non-active users image
      var friend = conversation.conversationUsers.find(
        (x) => x.userId !== this.user.id
      )
      if (typeof friend === 'undefined') {
        return false
      }

      var user = this.userMap[friend.userId]
      if (typeof user === 'undefined' || typeof user.avatar === 'undefined') {
        return false
      }

      return `${process.env.VUE_APP_API_URL}/public/${user.avatar}`
    },
    getFriendColor(conversation) {
      const defaultColor = 'blue'
      if (
        typeof conversation !== 'object' ||
        conversation === null ||
        !Array.isArray(conversation.conversationUsers)
      ) {
        return defaultColor
      }

      let friend = {}
      if (conversation.conversationUsers.length <= 2) {
        friend = conversation.conversationUsers.find(
          (x) => x.userId !== this.user.id)

        if (typeof friend === 'undefined') {
          return defaultColor
        }
      }
      else {
        if (typeof message !== 'undefined') {
          friend = conversation.conversationUsers.find(
            (x) => x.userId === message.userId)
        }
      }

      if (friend.color === null) {
        return defaultColor
      }

      return friend.color
    },
    getFriendAvatarText(conversation) {
      var friend = conversation.conversationUsers.find(
        (x) => x.userId !== this.user.id
      )
      if (typeof friend === 'undefined') {
        return false
      }

      var user = this.userMap[friend.userId]
      if (typeof user === 'undefined') {
        return false
      }

      return user.displayName.charAt(0)
    },  
  },
}
</script>

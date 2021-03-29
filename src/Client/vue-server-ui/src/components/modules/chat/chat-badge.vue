<template>
  <div>
    <v-badge
      :content="conversation.unreadMessages"
      v-if="conversation.unreadMessages > 0"
    >
      <template v-if="conversation.conversationUsers.length > 2">
        <fa-icon icon="users" size="2x" />
      </template>
      <template v-else>
        <chat-avatar :avatar="avatar" :text="text" :color="color" size="48" />
      </template>
    </v-badge>

    <template v-else>
      <template v-if="conversation.conversationUsers.length > 2">
        <fa-icon icon="users" size="2x" />
      </template>
      <template v-else>
        <chat-avatar :avatar="avatar" :text="text" :color="color" size="48" />
      </template>
    </template>
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
    avatar() {
      if (!Array.isArray(this.conversation.conversationUsers)) {
        return null
      }

      if (this.conversation.conversationUsers.length > 2) {
        return null
      }
      else {
        // For one on one conversation use the non-active users image
        var friend = this.conversation.conversationUsers.find(
          (x) => x.userId !== this.user.id)

        if (typeof friend === 'undefined' || typeof friend.userId == 'undefined'
            || typeof this.userMap[friend.userId].avatar === 'undefined') {
          return null
        }

        return this.userMap[friend.userId].avatar
      }
    },
    text() {
      if (!Array.isArray(this.conversation.conversationUsers)) {
        return null
      }
      
      if (this.conversation.conversationUsers.length > 2) {
        return null
      }
      else {
        // For one on one conversation use the non-active users image
        var friend = this.conversation.conversationUsers.find(
          (x) => x.userId !== this.user.id)

        if (typeof friend === 'undefined' || typeof friend.userId == 'undefined'
            || typeof this.userMap[friend.userId].displayName === 'undefined') {
          return null
        }

        return this.userMap[friend.userId].displayName.charAt(0)
      }
    },
    color() {
      if (!Array.isArray(this.conversation.conversationUsers)) {
        return null
      }

      if (this.conversation.conversationUsers.length > 2) {
        return null
      }
      else {
        // For one on one conversation use the non-active users image
        var friend = this.conversation.conversationUsers.find(
          (x) => x.userId !== this.user.id)

        if (typeof friend === 'undefined' || typeof friend.color == 'undefined') {
          return null
        }

        return friend.color
      }
    },
  },
}
</script>

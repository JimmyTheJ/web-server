<template>
  <div>
    <template v-if="usePersonal === false && conversation.conversationUsers.length > 2">
      <fa-icon icon="users" size="2x"></fa-icon>
    </template>
    <template v-else-if="friendHasAvatar(conversation) !== false">
      <v-avatar :size="maxSize">
        <v-img :src="friendHasAvatar(conversation)"></v-img>
      </v-avatar>
    </template>
    <template v-else>
      <v-avatar :color="getFriendColor(conversation)" :size="maxSize">
        <span :class="['white--text', getTextSize()]">{{
          getFriendAvatarText(conversation)
        }}</span>
      </v-avatar>
    </template>
  </div>
</template>

<script>
import { mapState } from 'vuex'

export default {
  name: 'chat-avatar',
  data() {
    return {
      maxSize: 0,
    }
  },
  props: {
    conversation: {
      type: Object,
      required: true,
    },
    size: {
      type: String,
      required: false,
    },
    usePersonal: {
      type: Boolean,
      required: false,
    },
    message: {
      type: Object,
      required: false,
    },
  },
  computed: {
    ...mapState({
      user: (state) => state.auth.user,
      userMap: (state) => state.auth.userMap,
    }),
  },
  mounted() {
    if (typeof this.size !== 'undefined') this.maxSize = this.size
    else {
      this.maxSize = '48'
    }
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
    getTextSize() {
      if (this.maxSize >= 40) {
        return 'headline'
      } else if (this.maxSize < 40) {
        return 'caption'
      }
    },
  },
}
</script>

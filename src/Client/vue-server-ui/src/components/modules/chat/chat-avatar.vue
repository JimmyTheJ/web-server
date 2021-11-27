<template>
  <div>
    <template v-if="typeof avatar !== undefined && avatar !== null">
      <v-avatar :size="maxSize">
        <v-img :src="avatarPath"></v-img>
      </v-avatar>
    </template>
    <template v-else>
      <v-avatar :color="color" :size="maxSize">
        <span :class="['white--text', getTextSize()]">
          {{ text }}
        </span>
      </v-avatar>
    </template>
  </div>
</template>

<script>
const FN = 'chat-avatar'

import { mapState } from 'vuex'

export default {
  name: 'chat-avatar',
  data() {
    return {}
  },
  props: {
    size: {
      type: String,
      required: false,
    },
    avatar: {
      type: String,
      required: false,
      default: null,
    },
    text: {
      type: String,
      required: false,
      default: null,
    },
    color: {
      type: String,
      required: false,
      default: null,
    },
  },
  computed: {
    ...mapState({
      user: state => state.auth.user,
    }),
    maxSize() {
      if (typeof this.size !== 'undefined' && this.size !== null)
        return this.size
      else return '48'
    },
    avatarPath() {
      return `${process.env.VUE_APP_API_URL}/public/${this.avatar}`
    },
  },
  methods: {
    friendHasAvatar(conversation) {
      if (
        typeof conversation !== 'object' ||
        conversation === null ||
        !Array.isArray(conversation.conversationUsers)
      ) {
        return false
      }

      // For one on one conversation use the non-active users image
      var friend = conversation.conversationUsers.find(
        x => x.userId !== this.user.id
      )
      if (typeof friend === 'undefined') {
        return false
      }

      this.$_console_log(`[${FN} friendHasAvatar]:`, friend)

      return `${process.env.VUE_APP_API_URL}/public/${friend.user.avatar}`
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
          x => x.userId !== this.user.id
        )

        if (typeof friend === 'undefined') {
          return defaultColor
        }
      } else {
        if (typeof message !== 'undefined') {
          friend = conversation.conversationUsers.find(
            x => x.userId === message.userId
          )
        }
      }

      if (friend.color === null) {
        return defaultColor
      }

      return friend.color
    },
    getFriendAvatarText(conversation) {
      var friend = conversation.conversationUsers.find(
        x => x.userId !== this.user.id
      )
      if (typeof friend === 'undefined') {
        return false
      }

      return friend.user.displayName.charAt(0)
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

<template>
  <div>
    <v-badge :content="unreadMessages" v-if="unreadMessages > 0">
      <template v-if="!Array.isArray(users)">
        <fa-icon icon="plus" size="2x" />
      </template>
      <template v-else-if="Array.isArray(users) && users.length > 2">
        <fa-icon icon="users" size="2x" />
      </template>
      <template v-else>
        <chat-avatar :avatar="avatar" :text="text" :color="color" size="48" />
      </template>
    </v-badge>

    <template v-else>
      <template v-if="!Array.isArray(users)">
        <fa-icon icon="plus" size="2x" />
      </template>
      <template v-else-if="Array.isArray(users) && users.length > 2">
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
    unreadMessages: {
      type: Number,
      required: false,
    },
    users: {
      type: Array,
      required: false,
    },
    avatar: {
      type: String,
      required: false,
    },
  },
  computed: {
    ...mapState({
      user: state => state.auth.user,
    }),
    text() {
      if (!Array.isArray(this.users)) {
        return null
      }

      if (this.users.length > 2) {
        return null
      } else {
        // For one on one conversation use the non-active users image
        var friend = this.users.find(x => x.userId !== this.user.id)

        if (
          typeof friend === 'undefined' ||
          typeof friend.userId == 'undefined'
        ) {
          return null
        }

        if (friend.userDisplayName !== null)
          return friend.userDisplayName.charAt(0)
        else return friend.userId.charAt(0)
      }
    },
    color() {
      if (!Array.isArray(this.users)) {
        return null
      }

      if (this.users.length > 2) {
        return null
      } else {
        // For one on one conversation use the non-active users image
        var friend = this.users.find(x => x.userId !== this.user.id)

        if (
          typeof friend === 'undefined' ||
          typeof friend.color == 'undefined'
        ) {
          return null
        }

        return friend.color
      }
    },
  },
}
</script>

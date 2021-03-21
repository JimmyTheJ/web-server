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
}
</script>

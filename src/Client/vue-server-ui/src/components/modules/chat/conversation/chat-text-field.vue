<template>
  <div class="chat-message-textfield" ref="chatTextfield">
    <v-text-field
      v-model="message.text"
      autofocus
      label="Message"
      ref="msgField"
      @keyup.shift.enter.stop.prevent="message.text += '\r\n'"
      @keyup.enter.exact.prevent="sendMessage"
      class="py-0 pr-0 pl-2"
    >
      <template v-slot:append-outer>
        <v-btn icon text @click="sendMessage"
          ><fa-icon size="lg" icon="paper-plane"></fa-icon
        ></v-btn>
      </template>
    </v-text-field>
  </div>
</template>

<script>
import service from '@/services/chat'

import { mapState } from 'vuex'

export default {
  name: 'chat-text-field',
  data() {
    return {
      message: {
        text: '',
      },
    }
  },
  props: {
    conversationId: {
      type: Number,
      required: true,
    },
  },
  computed: {
    ...mapState({
      user: state => state.auth.user,
    }),
  },
  methods: {
    async sendMessage() {
      this.message.id = 0
      this.message.userId = this.user.id
      this.message.conversationId = this.conversationId

      await service.sendMessage(this.message)

      this.message = { text: '' }
      this.$refs.msgField.focus()
    },
  },
}
</script>

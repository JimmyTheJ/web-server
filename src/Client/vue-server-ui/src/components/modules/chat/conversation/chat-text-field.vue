<template>
  <div>
    <v-textarea
      v-model="message.text"
      ref="msgTextArea"
      id="msgTextArea"
      counter
      filled
      auto-grow
      label="Message"
      rows="1"
      @keyup.enter.exact.prevent="sendMessage"
      @keyup.shift.enter.exact.prevent="getChatAreaHeight()"
      @input="getChatAreaHeight()"
      class="py-0 pr-0 pl-2"
    >
      <template v-slot:append-outer>
        <v-btn icon text @click="sendMessage"
          ><fa-icon size="lg" icon="paper-plane"></fa-icon
        ></v-btn>
      </template>
    </v-textarea>
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
      chatMsgField: null,
      chatAreaHeight: 0,
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
  mounted() {
    this.chatMsgField = document.getElementById('msgTextArea')
  },
  methods: {
    async sendMessage() {
      this.message.id = 0
      this.message.userId = this.user.id
      this.message.conversationId = this.conversationId

      await service.sendMessage(this.message)

      this.message = { text: '' }
      this.$nextTick(() => {
        this.$refs.msgTextArea.focus()
        setTimeout(() => {
          this.getChatAreaHeight()
        }, 100)
      })
    },
    getChatAreaHeight() {
      this.chatAreaHeight = document.getElementById('msgTextArea').clientHeight
      this.$nextTick(() => this.$emit('updateHeight', this.chatAreaHeight))
    },
  },
}
</script>

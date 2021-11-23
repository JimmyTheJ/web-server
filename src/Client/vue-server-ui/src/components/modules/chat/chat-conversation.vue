<template>
  <div v-if="show">
    <div id="chat-container">
      <chat-toolbar :conversation="conversation" @goBack="goBack" />
      <chat-body
        :conversation="conversation"
        :colorMap="colorMap"
        :textFieldHeight="chatFieldHeight"
      />
      <chat-text-field
        :conversationId="conversation.id"
        @updateHeight="updateHeight"
      />
    </div>
  </div>
</template>

<script>
import ChatBubble from './chat-bubble.vue'
import ChatAvatar from './chat-avatar.vue'
import ChatToolbar from './conversation/chat-toolbar.vue'
import ChatBody from './conversation/chat-body.vue'
import ChatTextField from './conversation/chat-text-field.vue'
import GenericDialog from '../generic-dialog.vue'

export default {
  name: 'chat-conversation',
  data() {
    return {
      bodyContainerHeight: 0,
      othersLastReadMessage: -1,
      colorMap: {},
      chatFieldHeight: -1,
    }
  },
  components: {
    ChatBubble,
    ChatAvatar,
    'chat-toolbar': ChatToolbar,
    'chat-body': ChatBody,
    'chat-text-field': ChatTextField,
    GenericDialog,
  },
  props: {
    conversation: {
      type: Object,
      required: true,
    },
    show: {
      type: Boolean,
      required: true,
    },
  },
  watch: {
    show(newValue) {
      if (newValue === true) {
        this.$store.dispatch('getMessagesForConversation', this.conversation.id)
      } else {
        this.$store.dispatch(
          'clearMessagesForConversation',
          this.conversation.id
        )
      }
    },
  },
  mounted() {
    this.getColorMap()
  },
  methods: {
    getColorMap() {
      this.conversation.conversationUsers.forEach((element, index) => {
        this.colorMap[element.userId] = element.color
      })
    },
    goBack() {
      this.$emit('goBack', true)
    },
    updateHeight(height) {
      this.chatFieldHeight = height
    },
  },
}

function getPosition(element) {
  var xPosition = 0
  var yPosition = 0

  while (element) {
    xPosition += element.offsetLeft - element.scrollLeft + element.clientLeft
    yPosition += element.offsetTop - element.scrollTop + element.clientTop
    element = element.offsetParent
  }

  return { x: xPosition, y: yPosition }
}
</script>

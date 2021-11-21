<template>
  <div v-if="show">
    <div id="chat-container">
      <chat-toolbar :conversation="conversation" @goBack="goBack" />
      <chat-body
        :conversation="conversation"
        :show="show"
        :colorMap="colorMap"
      />
      <chat-text-field :conversationId="conversation.id" />
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
    // resizeWindow(event) {
    //   this.updateContainerHeight()
    // },
    // async updateContainerHeight(attemptScroll) {
    //   this.$nextTick(() => {
    //     let bodyContainer = this.$refs.chatBodyContainer
    //     let chatMessageTextfield = this.$refs.chatTextfield
    //     this.bodyContainerHeight =
    //       window.innerHeight -
    //       bodyContainer.offsetTop -
    //       chatMessageTextfield.clientHeight -
    //       4

    //     this.$_console_log(
    //       'Containers',
    //       bodyContainer,
    //       chatMessageTextfield,
    //       'Window Height',
    //       window.innerHeight,
    //       this.bodyContainerHeight
    //     )

    //     if (attemptScroll === true) {
    //       if (
    //         Array.isArray(this.conversation.messages) &&
    //         this.conversation.messages.length > 0
    //       ) {
    //         this.$_console_log(
    //           'Heights: ',
    //           this.bodyContainerHeight,
    //           this.chatWindow.scrollHeight
    //         )
    //         if (this.bodyContainerHeight >= this.chatWindow.scrollHeight) {
    //           this.$_console_log(
    //             'Show watcher: No scrollbar exists. Reading all messages'
    //           )
    //           this.scrollToBottom()
    //         } else {
    //           this.$_console_log(
    //             'Show watcher: Message length is greater than 0. Scrolling to last read message'
    //           )
    //           this.scrollToLastReadMessage()
    //         }
    //       }
    //     }
    //   })
    // },
    goBack() {
      this.$emit('goBack', true)
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

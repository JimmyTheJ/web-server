<template>
  <v-container class="pa-0">
    <generic-dialog
      :title="dialogTitle"
      :open="dialogOpen"
      @dialog-close="dialogOpen = false"
      :maxWidth="960"
    >
      <v-card>
        <chat-starter @createConversation="startConversation" />
      </v-card>
    </generic-dialog>

    <v-row no-gutters>
      <v-col
        cols="12"
        md="3"
        lg="2"
        class="px-0"
        v-show="(isMobile && !hideMobile) || !isMobile"
      >
        <v-list shaped>
          <v-list-item @click="dialogOpen = true">
            <v-list-item-icon>
              <chat-badge :unreadMessages="0" />
            </v-list-item-icon>
            <v-list-item-content>
              Start new
            </v-list-item-content>
          </v-list-item>
        </v-list>

        <template
          v-if="Array.isArray(conversations) && conversations.length > 0"
        >
          <v-list shaped>
            <v-list-item-group v-model="selectedConversationId" color="primary">
              <v-list-item
                v-for="(convo, i) in conversations"
                :key="i"
                @click="selectedConversation = convo"
              >
                <v-list-item-icon>
                  <chat-badge
                    :avatar="convo.avatar"
                    :unreadMessages="convo.unreadMessages"
                    :users="convo.conversationUsers"
                  />
                </v-list-item-icon>
                <v-list-item-content>
                  {{ convo.title }}
                </v-list-item-content>
              </v-list-item>
            </v-list-item-group>
          </v-list>
        </template>
      </v-col>

      <v-col
        cols="12"
        md="9"
        lg="10"
        class="chat-conversation-window pb-0 px-0 pr-2"
        v-show="(isMobile && hideMobile) || !isMobile"
      >
        <template v-for="(conversation, i) in conversations">
          <chat-conversation
            :key="i"
            :conversation="conversation"
            :show="shouldShowConversation(conversation)"
            @goBack="closeConversation"
          />
        </template>
      </v-col>
    </v-row>
  </v-container>
</template>

<script>
import ChatConversation from '../modules/chat/chat-conversation.vue'
import ChatBadge from '../modules/chat/chat-badge.vue'
import ChatStarter from '../modules/chat/chat-starter.vue'
import ChatHub from '@/plugins/chat-hub'

import { mapState } from 'vuex'
import GenericDialog from '../modules/generic-dialog.vue'

export default {
  name: 'chat-messaging',
  data() {
    return {
      startNewConversation: {
        avatar: null,
        conversationUsers: null,
        id: -1,
        messages: null,
        title: 'Start New',
        unreadMessages: 0,
      },
      selectedConversationId: -1,
      selectedConversation: null,
      search: null,
      isLoading: false,
      hideMobile: false,
      dialogOpen: false,
      dialogTitle: 'Conversation Starter',
      timer: null,
    }
  },
  components: {
    ChatConversation,
    ChatBadge,
    ChatStarter,
    GenericDialog,
  },
  computed: {
    ...mapState({
      user: state => state.auth.user,
      userMap: state => state.auth.userMap,
      conversations: state => state.chat.conversations,
    }),
    isMobile() {
      return this.$vuetify.breakpoint.mobile
    },
  },
  watch: {
    selectedConversation: {
      handler(newValue, oldValue) {
        this.$_console_log('SelectedConversation', newValue)
        if (newValue === null) {
          this.hideMobile = false
        } else {
          this.hideMobile = true
        }
      },
      deep: true,
    },
  },
  mounted() {
    this.timer = setInterval(() => {
      this.$store.dispatch('getCurrentTime')
    }, 15000)
  },
  beforeDestroy() {
    this.timer = null
  },
  methods: {
    closeConversation() {
      this.selectedConversation = null
    },
    async getAllConversations() {
      this.$store.dispatch('getAllConversationsForUser').then(resp => {
        const newMessages = this.conversation.messages.filter(
          x =>
            (!Array.isArray(x.readReceipts) || x.readReceipts.length === 0) &&
            x.userId !== this.user.id
        )
        if (!Array.isArray(newMessages) || newMessages.length === 0) {
          this.$_console_log('ReadAllMessages: NewMessages is null or empty')
          return
        }
      })
    },
    async startConversation(conversation) {
      this.$store
        .dispatch('startNewConversation', conversation)
        .then(resp => {
          ChatHub.connection.invoke('createConversation', resp.data)
        })
        .finally(() => (this.dialogOpen = false))
    },
    shouldShowConversation(conversation) {
      if (typeof conversation === 'object' && conversation !== null) {
        if (
          typeof this.selectedConversation === 'object' &&
          this.selectedConversation !== null
        ) {
          if (conversation.id === this.selectedConversation.id) {
            return true
          }
        }
      }

      return false
    },
  },
}
</script>

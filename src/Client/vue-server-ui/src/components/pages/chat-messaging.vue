<template>
  <v-container class="py-0">
    <div class="headline">Chat System</div>

    <div v-show="(isMobile && !hideMobile) || !isMobile">
      <v-row no-gutters align="center">
        <v-col cols="12" sm="8" md="9">
          <v-autocomplete
            v-model="newConversation.users"
            :items="userList"
            item-value="id"
            item-text="displayName"
            prepend-icon="mdi-database-search"
            label="Select user(s)"
            :loading="isLoading"
            :search-input="search"
            multiple
          >
          </v-autocomplete>
        </v-col>
        <v-col cols="12" sm="4" md="3">
          <v-btn @click="startConversation" class="mb-2"
            >Start Conversation</v-btn
          >
        </v-col>
      </v-row>
    </div>

    <v-row no-gutters>
      <v-col
        cols="12"
        md="3"
        lg="2"
        class="px-0"
        v-show="(isMobile && !hideMobile) || !isMobile"
      >
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
                  <chat-badge :conversation="convo" />
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
            :time="currentTime"
            :show="shouldShowConversation(conversation)"
            :mobile="isMobile"
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

import { mapState } from 'vuex'

export default {
  name: 'chat-messaging',
  data() {
    return {
      selectedConversationId: -1,
      selectedConversation: null,
      newConversation: {
        users: [],
      },
      search: null,
      isLoading: false,
      currentTime: 0,
      hideMobile: false,
    }
  },
  components: {
    ChatConversation,
    ChatBadge,
  },
  mounted() {
    this.countTime()
  },
  computed: {
    ...mapState({
      user: (state) => state.auth.user,
      userList: (state) => state.auth.otherUsers,
      conversations: (state) => state.chat.conversations,
    }),
    isMobile() {
      return this.$vuetify.breakpoint.mobile
    },
  },
  watch: {
    selectedConversation: {
      handler(newValue, oldValue) {
        //if (typeof newValue === 'object' && newValue !== null) {
        //    if ((typeof oldValue === 'object' && oldValue !== null && newValue.id !== oldValue.id) || typeof oldValue !== 'object' || oldValue === null) {
        //        this.$store.dispatch('getMessagesForConversation', newValue.id);
        //    }
        //}

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
  methods: {
    countTime() {
      this.currentTime = Math.trunc(new Date().getTime() / 1000)

      setTimeout(() => {
        this.countTime()
      }, 1000)
    },
    closeConversation() {
      this.selectedConversation = null
    },
    async getAllConversations() {
      this.$store.dispatch('getAllConversationsForUser').then((resp) => {
        const newMessages = this.conversation.messages.filter(
          (x) =>
            (!Array.isArray(x.readReceipts) || x.readReceipts.length === 0) &&
            x.userId !== this.user.id
        )
        if (!Array.isArray(newMessages) || newMessages.length === 0) {
          this.$_console_log('ReadAllMessages: NewMessages is null or empty')
          return
        }
      })
    },
    async startConversation() {
      this.$store
        .dispatch('startNewConversation', this.newConversation)
        .then(() => {})
        .catch(() => {})
        .then(() => (this.newConversation.users = []))
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

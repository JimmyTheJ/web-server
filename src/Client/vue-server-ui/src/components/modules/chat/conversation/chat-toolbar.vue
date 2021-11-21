<template>
  <div>
    <generic-dialog
      :open="deleteConversationDialog"
      title="Delete Conversation"
      :maxWidth="400"
      :hideClose="true"
      @dialog-close="deleteConversationDialog = false"
    >
      <v-card>
        <v-card-text>
          Are you sure you want to delete this conversation ?
        </v-card-text>
        <v-card-actions>
          <v-btn @click="deleteConversation">Yes</v-btn>
          <v-btn @click="deleteConversationDialog = false">No</v-btn>
        </v-card-actions>
      </v-card>
    </generic-dialog>

    <generic-dialog
      :open="changeUserColorDialog"
      title="Change User Color"
      :maxWidth="260"
      :hideClose="true"
      @dialog-close="deleteConversationDialog = false"
    >
      <v-card>
        <v-card-text class="text-center">
          <v-avatar
            v-for="(color, i) in colorList"
            :key="i"
            :color="color"
            size="48"
            class="ma-2"
            @click="changeUserColor(i)"
          ></v-avatar>
        </v-card-text>
      </v-card>
    </generic-dialog>

    <v-toolbar>
      <v-btn text @click="goBack" v-show="mobile" class="small-btn pa-0 ma-0">
        <fa-icon icon="reply"></fa-icon>
      </v-btn>
      <v-btn
        text
        @click="editingTitle = !editingTitle"
        class="small-btn pa-0 my-0 ml-0 mr-2"
      >
        <v-icon>mdi-account-edit</v-icon>
      </v-btn>
      <v-toolbar-title v-if="!editingTitle">
        {{ conversation.title }}
      </v-toolbar-title>
      <v-toolbar-title v-else style="width: 100%" class="ml-3 pl-2">
        <div style="display: flex; flex-direction: row">
          <v-text-field
            v-model="newTitle"
            @keyup.enter.prevent="updateTitle"
            autofocus
          ></v-text-field>
          <v-btn v-if="!mobile" @click="updateTitle">SAVE</v-btn>
        </div>
      </v-toolbar-title>
      <v-spacer></v-spacer>
      <v-menu
        :close-on-content-click="true"
        :nudge-width="200"
        v-model="menu"
        offset-x
      >
        <template v-slot:activator="{ on }">
          <v-btn v-on="on" icon
            ><fa-icon icon="ellipsis-v" size="2x"></fa-icon
          ></v-btn>
        </template>
        <v-card>
          <v-list>
            <v-list-item
              v-if="canChangeUserColor"
              @click="changeUserColorDialog = true"
            >
              <v-list-item-title class="ml-2">
                Change user color
              </v-list-item-title>
            </v-list-item>
            <v-list-item
              v-if="isConversationDeletable"
              @click="deleteConversationDialog = true"
            >
              <v-icon>mdi-delete</v-icon>
              <v-list-item-title class="ml-2"
                >Delete Conversation</v-list-item-title
              >
            </v-list-item>
          </v-list>
        </v-card>
      </v-menu>
    </v-toolbar>
  </div>
</template>

<script>
import GenericDialog from '@/components/modules/generic-dialog.vue'

import { mapState } from 'vuex'

export default {
  name: 'chat-toolbar',
  components: {
    GenericDialog,
  },
  data() {
    return {
      changeUserColorDialog: false,
      deleteConversationDialog: false,
      editingTitle: false,
      menu: false,
      newTitle: null,
      chatWindow: null,
      colorList: [
        'indigo',
        'purple',
        'teal',
        'cyan',
        'pink',
        'green',
        'orange',
        'blue-grey',
        'brown',
      ],
    }
  },
  props: {
    conversation: {
      type: Object,
      required: true,
    },
  },
  computed: {
    ...mapState({
      user: state => state.auth.user,
      activeModules: state => state.auth.activeModules,
    }),
    isConversationDeletable() {
      const chatModule = this.activeModules.find(x => x.id === 'chat')
      if (typeof chatModule !== 'undefined') {
        const feature = chatModule.userModuleFeatures.find(
          x => x.moduleFeatureId === 'chat-delete-conversation'
        )
        if (typeof feature !== 'undefined') {
          return true
        }
      }

      return false
    },
    canChangeUserColor() {
      return this.conversation.conversationUsers.length == 2
    },
    mobile() {
      return this.$vuetify.breakpoint.mobile
    },
  },
  watch: {
    editingTitle(newValue) {
      if (newValue === false) {
        this.newTitle = null
      } else {
        this.newTitle = this.conversation.title
      }
    },
  },
  methods: {
    goBack() {
      this.$emit('goBack', true)
    },
    deleteConversation() {
      this.$store
        .dispatch('deleteConversation', this.conversation.id)
        .then(() => {})
        .catch(() => {})
        .then(() => (this.deleteConversationDialog = false))
    },
    changeUserColor(color) {
      this.$_console_log(`Selected color is: ${color}`)
      const otherUserIndex = this.conversation.conversationUsers.findIndex(
        x => x.userId !== this.user.id
      )
      if (otherUserIndex < 0) {
        return this.$_console_log("Couldn't find other user")
      }

      this.$store
        .dispatch('changeConversationUserColor', {
          conversationId: this.conversation.id,
          userId: this.conversation.conversationUsers[otherUserIndex].userId,
          colorId: color,
        })
        .then(() => {})
        .catch(() => {})
        .then(() => {
          this.changeUserColorDialog = false
        })
    },
    updateTitle() {
      const title = this.newTitle

      if (/^\s*$/.test(title)) {
        this.editingTitle = false
        return
      }

      this.$store
        .dispatch('updateConversationTitle', {
          conversationId: this.conversation.id,
          title: title,
        })
        .then(() => {})
        .catch(() => {})
        .then(() => (this.editingTitle = false))
    },
  },
}
</script>

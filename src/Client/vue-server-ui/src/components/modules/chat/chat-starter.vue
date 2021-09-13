<template>
  <div>
    <template v-for="(user, i) in selectedUsers">
      <v-chip :key="i" close @click:close="removeUser(user)" class="my-2 mx-1">
        {{ user.displayName }}
      </v-chip>
    </template>
    <v-text-field
      v-model="search"
      label="Search users to Start Conversation"
      clearable
      class="mx-2"
    ></v-text-field>

    <v-list v-show="users.length > 0">
      <v-list-item v-for="(user, i) in users" :key="i" @click="addUser(user)">
        <v-list-item-icon v-if="user.avatar !== undefined">
          <v-img :src="user.avatar" />
        </v-list-item-icon>
        <v-list-item-icon v-else>
          <v-avatar :color="getColor(i)" size="48">
            <span class="white--text">
              {{ user.displayName[0] }}
            </span>
          </v-avatar>
        </v-list-item-icon>
        <v-list-item-content>{{ user.displayName }}</v-list-item-content>
      </v-list-item>
    </v-list>
    <div class="text-center pb-3">
      <v-btn @click="startConversation()">
        Start the Conversation!
      </v-btn>
    </div>
  </div>
</template>

<script>
const FN = 'ChatStarter'

import Dispatcher from '@/services/ws-dispatcher'
import service from '@/services/auth'

export default {
  name: 'chat-starter',
  data() {
    return {
      isLoading: false,
      search: '',
      users: [],
      selectedUsers: [],
      colorMap: [
        'red',
        'blue',
        'green',
        'gray',
        'purple',
        'orange',
        'pink',
        'yellow',
      ],
    }
  },
  watch: {
    search(newValue) {
      this.getUsers(newValue)
    },
  },
  methods: {
    getUsers(value) {
      Dispatcher.request(() => {
        service
          .getUsersFuzzy(value)
          .then(resp => {
            // Data returned
            if (!Array.isArray(resp.data) || resp.data.length === 0) {
              this.users = []
            } else {
              if (
                !Array.isArray(this.selectedUsers) ||
                this.selectedUsers.length === 0
              ) {
                this.users = resp.data
              } else {
                let newUsers = []
                resp.data.forEach(item => {
                  if (
                    this.selectedUsers.findIndex(x => x.id === item.id) === -1
                  ) {
                    newUsers.push(item)
                  }
                })

                this.users = newUsers.slice(0)
              }
            }
          })
          .catch(() => {
            this.$_console_log(
              `[${FN}] getUsers: Failed to get users with query ${value}`
            )
          })
      })
    },
    addUser(user) {
      if (this.selectedUsers.findIndex(x => x.id === user.id) === -1) {
        this.selectedUsers.push(user)

        let spliceIndex = this.users.findIndex(x => x.id === user.id)
        if (spliceIndex > -1) {
          this.users.splice(spliceIndex, 1)
        }
      }
    },
    removeUser(user) {
      let index = this.selectedUsers.findIndex(x => x.id === user.id)
      if (index > -1) {
        this.selectedUsers.splice(index, 1)
      }
    },
    getColor(index) {
      return this.colorMap[index % this.colorMap.length]
    },
    startConversation() {
      const userIds = this.selectedUsers.map(x => x.id)
      this.$emit('createConversation', { users: userIds })

      this.users = []
      this.selectedUsers = []
      this.search = ''
      this.isLoading = false
    },
  },
}
</script>

<template>
  <div>
    <generic-dialog
      :title="
        newDirectoryType === dirTypes.Group
          ? 'New Group Directory Rule'
          : 'New User Directory Rule'
      "
      :open="createDirectoryDialog"
      :maxWidth="1280"
      @dialog-close="createDirectoryDialog = false"
    >
      <v-card v-if="newDirectoryObject != null">
        <v-card-text>
          <v-row>
            <v-col cols="12">
              <v-text-field
                v-model="newDirectoryObject.name"
                label="Folder Display Name"
              />
            </v-col>
            <v-col cols="12">
              <v-select
                v-if="newDirectoryType === dirTypes.Group"
                v-model="newDirectoryObject.role"
                label="Role"
                :items="roles"
                item-value="id"
                item-text="displayName"
              />
              <v-text-field
                v-else
                v-model="newDirectoryObject.userId"
                label="Username"
              />
            </v-col>
            <v-col cols="12">
              <v-text-field v-model="newDirectoryObject.path" label="Path" />
            </v-col>

            <v-col cols="12">
              <v-checkbox
                v-model="newDirectoryObject.default"
                label="Default"
              />
            </v-col>
            <v-col cols="12">
              <v-select
                v-model="newDirectoryObject.selectedPermissions"
                :items="permissions"
                label="Permissions"
                multiple
                item-text="key"
                item-value="value"
              />
            </v-col>

            <v-col cols="12">
              <v-btn
                @click="
                  newDirectoryType === dirTypes.Group
                    ? saveNewGroupDirectory(newDirectoryObject)
                    : saveNewUserDirectory(newDirectoryObject)
                "
                ><fa-icon icon="save" size="2x"
              /></v-btn>
            </v-col>
          </v-row>
        </v-card-text>
      </v-card>
    </generic-dialog>

    <v-card>
      <v-card-title
        >Directory Settings<v-btn icon @click="createNewSetting()"
          ><fa-icon icon="plus" color="green"/></v-btn
      ></v-card-title>
      <v-card-text v-for="(setting, i) in directorySettings" :key="i">
        <v-layout row class="no-gutters">
          <v-col cols="4" class="px-1">
            <v-text-field v-model="setting.key" label="Key:" />
          </v-col>
          <v-col cols="6" class="px-1">
            <v-text-field v-model="setting.value" label="Value:" />
          </v-col>
          <v-col cols="1" class="px-1">
            <v-btn @click="saveSetting(setting)"
              ><fa-icon icon="save" size="2x"
            /></v-btn>
          </v-col>
          <v-col cols="1" class="px-1">
            <v-btn @click="deleteSetting(setting.key)"
              ><fa-icon icon="window-close" size="2x"
            /></v-btn>
          </v-col>
        </v-layout>
      </v-card-text>
    </v-card>

    <v-card>
      <v-card-title
        >Group Directories
        <v-btn icon @click="createNewGroupDirectory()"
          ><fa-icon icon="plus" color="green"/></v-btn
        ><v-spacer />
        <v-text-field
          v-model="groupDirectorySearch"
          label="Search"
          v-if="groupDirectories.length > 0"
      /></v-card-title>
      <v-data-table
        v-if="groupDirectories.length > 0"
        :headers="groupDirectoryHeaders"
        :items="groupDirectories"
        item-key="id"
        class="elevation-1"
        :search="groupDirectorySearch"
      >
        <template v-slot:[`item.actions`]="{ item }">
          <!-- <v-icon small class="mr-2" @click="editItem(item)">
            mdi-pencil
          </v-icon> -->
          <v-icon small @click="deleteGroupDirectory(item.id)">
            mdi-delete
          </v-icon>
        </template>
      </v-data-table>
    </v-card>

    <v-card>
      <v-card-title>
        User Directories
        <v-btn icon @click="createNewUserDirectory()"
          ><fa-icon icon="plus" color="green"/></v-btn
        ><v-spacer />

        <v-text-field
          v-model="userDirectorySearch"
          label="Search"
          v-if="userDirectories.length > 0"
      /></v-card-title>
      <v-data-table
        v-if="userDirectories.length > 0"
        :headers="userDirectoryHeaders"
        :items="userDirectories"
        item-key="id"
        class="elevation-1"
        :search="userDirectorySearch"
      >
        <template v-slot:[`item.actions`]="props">
          <v-icon small @click="deleteUserDirectory(props.item.id)">
            mdi-delete
          </v-icon>
        </template>
      </v-data-table>
    </v-card>
  </div>
</template>

<script>
const FN = 'directory-management'

import service from '@/services/admin'
import Dispatcher from '@/services/ws-dispatcher'
import {
  convertFlagsToPermissions,
  convertPemissionsToFlags,
} from '@/helpers.js'

import { mapState } from 'vuex'
import genericDialog from '@/components/modules/generic-dialog.vue'

const DirectoryTypes = {
  Group: 0,
  User: 1,
}

function newGroupDirectory() {
  return {
    id: 0,
    name: null,
    path: null,
    role: null,
    accessFlags: 0,
    selectedPermissions: [],
  }
}

function newUserDirectory() {
  return {
    id: 0,
    name: null,
    path: null,
    default: false,
    userId: null,
    accessFlags: 0,
    selectedPermissions: [],
  }
}

function newSetting() {
  return {
    key: 'Directory_',
    value: null,
  }
}

export default {
  components: { genericDialog },
  name: 'directory-management',
  data() {
    return {
      dirTypes: DirectoryTypes,
      userDirectoryFilter: null,
      filteredUserDirectories: [],
      directorySettings: [],
      groupDirectories: [],
      userDirectories: [],
      permissions: [
        { key: 'Read File', value: 1 },
        { key: 'Read Folder', value: 2 },
        { key: 'Upload File', value: 4 },
        { key: 'Delete File', value: 8 },
        { key: 'Create Folder', value: 16 },
        { key: 'Delete Folder', value: 32 },
        { key: 'Move Folder', value: 64 },
        { key: 'Move File', value: 128 },
      ],
      groupDirectoryHeaders: [
        { text: 'Role', align: 'start', sortable: true, value: 'role' },
        { text: 'Name', value: 'name' },
        { text: 'Path', value: 'path' },
        { text: 'Permissions', value: 'accessFlags' },
        { text: 'Actions', value: 'actions' },
      ],
      userDirectoryHeaders: [
        { text: 'User Id', align: 'start', sortable: true, value: 'userId' },
        { text: 'Name', value: 'name' },
        { text: 'Path', value: 'path' },
        { text: 'Permissions', value: 'accessFlags' },
        { text: 'Actions', value: 'actions' },
      ],
      groupDirectorySearch: null,
      userDirectorySearch: null,
      createDirectoryDialog: false,
      newDirectoryObject: null,
      newDirectoryType: -1,
    }
  },
  computed: {
    ...mapState({
      roles: state =>
        typeof state.auth.admin !== 'undefined' ? state.auth.admin.roles : [],
    }),
    isMobile() {
      let val = this.$vuetify.breakpoint.name
      return val === 'xs' || val === 'sm'
    },
  },
  watch: {
    userDirectoryFilter(newValue) {
      if (
        typeof newValue === 'undefined' ||
        newValue === null ||
        newValue === ''
      ) {
        this.filteredUserDirectories = this.userDirectories
        return
      }

      const tempValue = newValue.slice(0)
      setTimeout(() => {
        if (this.userDirectoryFilter === tempValue) {
          this.$_console_log(
            `[${FN}] userDirectoryFilter watcher: Values should match`,
            tempValue,
            newValue
          )
          this.filteredUserDirectories = this.userDirectories.filter(x =>
            x.userId.includes(tempValue)
          )
        } else {
          this.$_console_log(
            `[${FN}] userDirectoryFilter watcher: Debounce hit, waiting to update`,
            tempValue,
            newValue
          )
        }
      }, 400)
    },
  },
  mounted() {
    this.getData()
  },
  methods: {
    getData() {
      Dispatcher.request(() => {
        service
          .getDirectorySettings()
          .then(resp => {
            this.$_console_log(
              `[${FN}] getData: Successfully got directory settings`
            )
            this.directorySettings = resp.data
          })
          .catch(() =>
            this.$_console_log(
              `[${FN}] getData: Failed to get directory settings`
            )
          )
      })

      Dispatcher.request(() => {
        service
          .getGroupDirectories()
          .then(resp => {
            this.$_console_log(
              `[${FN}] getData: Successfully got group directories`
            )
            this.groupDirectories = []
            if (Array.isArray(resp.data)) {
              resp.data.forEach(item => {
                this.groupDirectories.push(convertFlagsToPermissions(item))
              })
            }
          })
          .catch(() =>
            this.$_console_log(
              `[${FN}] getData: Failed to get group directories`
            )
          )
      })

      Dispatcher.request(() => {
        service
          .getUserDirectories()
          .then(resp => {
            this.$_console_log(
              `[${FN}] getData: Successfully got user directories`
            )
            this.userDirectories = []
            if (Array.isArray(resp.data)) {
              resp.data.forEach(item => {
                this.userDirectories.push(convertFlagsToPermissions(item))
              })
            }

            this.userDirectoryFilter = ''
          })
          .catch(() =>
            this.$_console_log(
              `[${FN}] getData: Failed to get user directories`
            )
          )
      })
    },
    createNewSetting() {
      this.directorySettings.push(newSetting())
    },
    createNewGroupDirectory() {
      this.newDirectoryObject = newGroupDirectory()
      this.newDirectoryType = DirectoryTypes.Group
      this.createDirectoryDialog = true
    },
    createNewUserDirectory() {
      this.newDirectoryObject = newUserDirectory()
      this.newDirectoryType = DirectoryTypes.User
      this.createDirectoryDialog = true
    },
    saveSetting(setting) {
      Dispatcher.request(() => {
        service.setServerSetting(setting)
      })
    },
    saveNewGroupDirectory(group) {
      let obj = convertPemissionsToFlags(group)
      this.$_console_log(
        `[${FN}] saveNewGroupDirectory: cleaned object is`,
        obj
      )

      Dispatcher.request(() => {
        service.addGroupDirectory(obj).then(resp => {
          if (resp.data > 0) {
            obj.id = resp.data
            this.groupDirectories.push(obj)
            this.createDirectoryDialog = false
          }
        })
      })
    },
    saveNewUserDirectory(user) {
      let obj = convertPemissionsToFlags(user)
      this.$_console_log(`[${FN}] saveNewUserDirectory: cleaned object is`, obj)

      Dispatcher.request(() => {
        service.addUserDirectory(obj).then(resp => {
          if (resp.data > 0) {
            obj.id = resp.data
            this.userDirectories.push(obj)
            this.createDirectoryDialog = false
          }
        })
      })
    },
    deleteSetting(key) {
      Dispatcher.request(() => {
        service.deleteServerSetting(key).then(resp => {
          let index = this.directorySettings.findIndex(x => x.key === key)
          if (index > -1) {
            this.directorySettings.splice(index, 1)
          }
        })
      })
    },
    deleteGroupDirectory(id) {
      Dispatcher.request(() => {
        service.deleteGroupDirectory(id).then(resp => {
          let index = this.groupDirectories.findIndex(x => x.id === id)
          if (index > -1) {
            this.groupDirectories.splice(index, 1)
          }
        })
      })
    },
    deleteUserDirectory(id) {
      Dispatcher.request(() => {
        service.deleteUserDirectory(id).then(resp => {
          let index = this.userDirectories.findIndex(x => x.id === id)
          if (index > -1) {
            this.userDirectories.splice(index, 1)
          }
        })
      })
    },
  },
}
</script>

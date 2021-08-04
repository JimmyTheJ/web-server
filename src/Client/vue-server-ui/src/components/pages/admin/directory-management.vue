<template>
  <div>
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
      ></v-card-title>
      <v-card-text>
        <div v-for="(dir, i) in groupDirectories" :key="i">
          <v-layout row v-if="!isMobile" class="no-gutters">
            <v-row>
              <v-col cols="2">
                <v-text-field
                  v-model="dir.name"
                  label="Name:"
                  :readonly="dir.id !== 0"
                />
              </v-col>
              <v-col cols="2">
                <v-select
                  v-model="dir.role"
                  label="Role"
                  :items="roles"
                  :readonly="dir.id !== 0"
                />
              </v-col>
              <v-col cols="4">
                <v-text-field
                  v-model="dir.path"
                  label="Path"
                  :readonly="dir.id !== 0"
                />
              </v-col>

              <v-col cols="3">
                <v-select
                  v-model="dir.selectedPermissions"
                  :items="permissions"
                  label="Permissions"
                  multiple
                  item-text="key"
                  item-value="value"
                  :readonly="dir.id !== 0"
                />
              </v-col>

              <v-col cols="1">
                <v-btn v-if="dir.id > 0" @click="deleteGroupDirectory(dir.id)"
                  ><fa-icon icon="window-close" size="2x"
                /></v-btn>
                <v-btn v-else @click="saveNewGroupDirectory(dir)"
                  ><fa-icon icon="save" size="2x" />
                </v-btn>
              </v-col>
            </v-row>
          </v-layout>
          <v-layout v-else row class="no-gutters">
            <v-row>
              <v-col cols="6">
                <v-text-field
                  v-model="dir.name"
                  label="Name:"
                  :readonly="dir.id !== 0"
                />
              </v-col>
              <v-col cols="6">
                <v-select
                  v-model="dir.role"
                  label="Role"
                  :items="roles"
                  :readonly="dir.id !== 0"
                />
              </v-col>
              <v-col cols="12">
                <v-text-field
                  v-model="dir.path"
                  label="Path"
                  :readonly="dir.id !== 0"
                />
              </v-col>

              <v-col cols="10">
                <v-select
                  v-model="dir.selectedPermissions"
                  :items="permissions"
                  label="Permissions"
                  multiple
                  item-text="key"
                  item-value="value"
                  :readonly="dir.id !== 0"
                />
              </v-col>

              <v-col cols="2">
                <v-btn v-if="dir.id > 0" @click="deleteGroupDirectory(dir.id)"
                  ><fa-icon icon="window-close" size="2x"
                /></v-btn>
                <v-btn v-else @click="saveNewGroupDirectory(dir)"
                  ><fa-icon icon="save" size="2x" />
                </v-btn>
              </v-col>
            </v-row>
          </v-layout>
          <v-divider class="ma-4"></v-divider>
        </div>
      </v-card-text>
    </v-card>
    <v-card>
      <v-card-title>
        User Directories
        <v-btn icon @click="createNewUserDirectory()"
          ><fa-icon icon="plus" color="green"
        /></v-btn>
      </v-card-title>
      <v-card-text>
        <v-text-field
          label="User Directory UserId Filter"
          v-model="userDirectoryFilter"
        />
        <div v-for="(dir, i) in filteredUserDirectories" :key="i">
          <v-layout row v-if="!isMobile" class="no-gutters">
            <v-row>
              <v-col cols="2">
                <v-text-field
                  v-model="dir.name"
                  label="Name:"
                  :readonly="dir.id !== 0"
                />
              </v-col>
              <v-col cols="1">
                <v-text-field
                  v-model="dir.userId"
                  label="Username"
                  :readonly="dir.id !== 0"
                />
              </v-col>
              <v-col cols="4">
                <v-text-field
                  v-model="dir.path"
                  label="Path"
                  :readonly="dir.id !== 0"
                />
              </v-col>

              <v-col cols="1">
                <v-checkbox
                  v-model="dir.default"
                  label="Default"
                  :readonly="dir.id !== 0"
                />
              </v-col>

              <v-col cols="3">
                <v-select
                  v-model="dir.selectedPermissions"
                  :items="permissions"
                  label="Permissions"
                  multiple
                  item-text="key"
                  item-value="value"
                  :readonly="dir.id !== 0"
                />
              </v-col>

              <v-col cols="1">
                <v-btn v-if="dir.id > 0" @click="deleteUserDirectory(dir.id)"
                  ><fa-icon icon="window-close" size="2x"
                /></v-btn>
                <v-btn v-else @click="saveNewUserDirectory(dir)"
                  ><fa-icon icon="save" size="2x"
                /></v-btn>
              </v-col>
            </v-row>
          </v-layout>
          <v-layout v-else row class="no-gutters">
            <v-row>
              <v-col cols="6">
                <v-text-field
                  v-model="dir.name"
                  label="Name:"
                  :readonly="dir.id !== 0"
                />
              </v-col>
              <v-col cols="6">
                <v-text-field
                  v-model="dir.userId"
                  label="Username"
                  :readonly="dir.id !== 0"
                />
              </v-col>
              <v-col cols="12">
                <v-text-field
                  v-model="dir.path"
                  label="Path"
                  :readonly="dir.id !== 0"
                />
              </v-col>

              <v-col cols="2">
                <v-checkbox
                  v-model="dir.default"
                  label="Default"
                  :readonly="dir.id !== 0"
                />
              </v-col>

              <v-col cols="8">
                <v-select
                  v-model="dir.selectedPermissions"
                  :items="permissions"
                  label="Permissions"
                  multiple
                  item-text="key"
                  item-value="value"
                  :readonly="dir.id !== 0"
                />
              </v-col>

              <v-col cols="2">
                <v-btn v-if="dir.id > 0" @click="deleteUserDirectory(dir.id)"
                  ><fa-icon icon="window-close" size="2x"
                /></v-btn>
                <v-btn v-else @click="saveNewUserDirectory(dir)"
                  ><fa-icon icon="save" size="2x"
                /></v-btn>
              </v-col>
            </v-row>
          </v-layout>
          <v-divider class="ma-4"></v-divider>
        </div>
      </v-card-text>
    </v-card>
  </div>
</template>

<script>
const FN = 'directory-management'

import service from '@/services/admin'
import Dispatcher from '@/services/ws-dispatcher'

import { mapState } from 'vuex'

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

function convertPemissionsToFlags(obj) {
  let clone = Object.assign({}, obj)
  if (
    Array.isArray(clone.selectedPermissions) &&
    clone.selectedPermissions.length > 0
  ) {
    clone.accessFlags = 0
    clone.selectedPermissions.forEach(value => {
      clone.accessFlags += value
    })
    delete clone.selectedPermissions

    return clone
  } else {
    if (typeof clone.selectedPermissions !== 'undefined')
      delete clone.selectedPermissions
    return clone
  }
}

function convertFlagsToPermissions(obj) {
  obj.selectedPermissions = []
  let i = 0
  let num = Math.pow(2, 8)

  if (obj.accessFlags > 0) {
    let val = obj.accessFlags
    while (num > 0) {
      if (val / num >= 1) {
        obj.selectedPermissions.push({ key: i++, value: num })
        val -= num
      }
      num /= 2
    }
  }

  return obj
}

export default {
  name: 'directory-management',
  data() {
    return {
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
      ],
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
      if (
        this.groupDirectories.length === 0 ||
        this.groupDirectories[this.groupDirectories.length - 1].id !== 0
      )
        this.groupDirectories.push(newGroupDirectory())
    },
    createNewUserDirectory() {
      if (
        this.userDirectories.length === 0 ||
        this.userDirectories[this.userDirectories.length - 1].id !== 0
      )
        this.userDirectories.push(newUserDirectory())
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
            let index = this.groupDirectories.findIndex(x => x.id === 0)
            if (index > -1) {
              this.groupDirectories[index].id = resp.data
            }
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
            let index = this.userDirectories.findIndex(x => x.id === 0)
            if (index > -1) {
              this.userDirectories[index].id = resp.data
            }
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

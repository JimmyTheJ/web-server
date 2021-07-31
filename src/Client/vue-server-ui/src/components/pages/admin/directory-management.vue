<template>
  <div>
    <v-card>
      <v-card-title
        >Directory Settings<v-btn icon @click="createNewSetting()"
          ><fa-icon icon="plus" color="green"/></v-btn
      ></v-card-title>
      <v-card-text>
        <div v-for="(setting, i) in directorySettings" :key="i">
          <v-layout row>
            <v-flex xs4 px-1>
              <v-text-field v-model="setting.key" label="Key:" />
            </v-flex>
            <v-flex xs6 px-1>
              <v-text-field v-model="setting.value" label="Value:" />
            </v-flex>
            <v-flex xs1 px-1>
              <v-btn @click="saveSetting(setting)"
                ><fa-icon icon="save" size="2x"
              /></v-btn>
            </v-flex>
            <v-flex xs1 px-1>
              <v-btn @click="deleteSetting(setting.key)"
                ><fa-icon icon="window-close" size="2x"
              /></v-btn>
            </v-flex>
          </v-layout>
        </div>
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
          <v-layout row>
            <v-flex xs6 sm3 lg2 px-1>
              <v-text-field
                v-model="dir.name"
                label="Name:"
                :readonly="dir.id !== 0"
              />
            </v-flex>
            <v-flex xs6 sm3 lg1 px-1>
              <v-text-field
                v-model="dir.role"
                label="Role"
                :readonly="dir.id !== 0"
              />
            </v-flex>
            <v-flex xs12 sm6 lg4 px-1>
              <v-text-field
                v-model="dir.path"
                label="Path"
                :readonly="dir.id !== 0"
              />
            </v-flex>

            <v-flex xs3 sm2 lg1 px-1>
              <v-checkbox
                v-model="dir.allowSubDirs"
                label="SubDirs"
                :readonly="dir.id !== 0"
              />
            </v-flex>
            <v-flex xs3 sm2 lg1 px-1>
              <v-checkbox
                v-model="dir.canUpload"
                label="Upload"
                :readonly="dir.id !== 0"
              />
            </v-flex>
            <v-flex xs3 sm2 lg1 px-1>
              <v-checkbox
                v-model="dir.canDelete"
                label="Delete"
                :readonly="dir.id !== 0"
              />
            </v-flex>
            <v-flex xs3 sm2 lg1 px-1>
              <v-btn v-if="dir.id > 0" @click="deleteGroupDirectory(dir.id)"
                ><fa-icon icon="window-close"
              /></v-btn>
              <v-btn v-else @click="saveNewGroupDirectory(dir)">Submit </v-btn>
            </v-flex>
          </v-layout>
        </div>
      </v-card-text>
    </v-card>
    <v-card>
      <v-card-title
        >User Directories
        <v-btn icon @click="createNewUserDirectory()"
          ><fa-icon icon="plus" color="green"/></v-btn
      ></v-card-title>
      <v-card-text>
        <div v-for="(dir, i) in userDirectories" :key="i">
          <v-card>
            <v-layout row>
              <v-flex xs6 sm3 lg2 px-1>
                <v-text-field
                  v-model="dir.name"
                  label="Name:"
                  :readonly="dir.id !== 0"
                />
              </v-flex>
              <v-flex xs6 sm3 lg1 px-1>
                <v-text-field
                  v-model="dir.userId"
                  label="Username"
                  :readonly="dir.id !== 0"
                />
              </v-flex>
              <v-flex xs12 sm6 lg4 px-1>
                <v-text-field
                  v-model="dir.path"
                  label="Path"
                  :readonly="dir.id !== 0"
                />
              </v-flex>
              <v-flex xs3 sm2 lg1 px-1>
                <v-checkbox
                  v-model="dir.default"
                  label="Default"
                  :readonly="dir.id !== 0"
                />
              </v-flex>
              <v-flex xs3 sm2 lg1 px-1>
                <v-checkbox
                  v-model="dir.allowSubDirs"
                  label="Allow Sub"
                  :readonly="dir.id !== 0"
                />
              </v-flex>
              <v-flex xs3 sm2 lg1 px-1>
                <v-checkbox
                  v-model="dir.canUpload"
                  label="Upload"
                  :readonly="dir.id !== 0"
                />
              </v-flex>
              <v-flex xs3 sm2 lg1 px-1>
                <v-checkbox
                  v-model="dir.canDelete"
                  label="Delete"
                  :readonly="dir.id !== 0"
                />
              </v-flex>
              <v-flex xs3 sm2 lg1>
                <v-btn v-if="dir.id > 0" @click="deleteUserDirectory(dir.id)"
                  ><fa-icon icon="window-close"
                /></v-btn>
                <v-btn v-else @click="saveNewUserDirectory(dir)">Submit </v-btn>
              </v-flex>
            </v-layout>
          </v-card>
        </div>
      </v-card-text>
    </v-card>
  </div>
</template>

<script>
const FN = 'directory-management'

import service from '@/services/admin'
import Dispatcher from '@/services/ws-dispatcher'

function newGroupDirectory() {
  return {
    id: 0,
    name: null,
    path: null,
    allowSubDirs: false,
    canUpload: false,
    canDelete: false,
    role: null,
  }
}

function newUserDirectory() {
  return {
    id: 0,
    name: null,
    path: null,
    default: false,
    allowSubDirs: false,
    canUpload: false,
    canDelete: false,
    userId: null,
  }
}

function newSetting() {
  return {
    key: 'Directory_',
    value: null,
  }
}

export default {
  name: 'directory-management',
  data() {
    return {
      directorySettings: [],
      groupDirectories: [],
      userDirectories: [],
    }
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
            this.groupDirectories = resp.data
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
            this.userDirectories = resp.data
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
      Dispatcher.request(() => {
        service.addGroupDirectory(group).then(resp => {
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
      Dispatcher.request(() => {
        service.addUserDirectory(user).then(resp => {
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

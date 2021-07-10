<template>
  <v-container>
    <v-card>
      <v-card-title>
        Manage User's modules
      </v-card-title>
      <v-card-text>
        <v-layout row>
          <v-flex xs12 sm12 md4>
            <v-list-item-group v-model="selectedUserPosition">
              <v-list-item v-for="(item, index) in userList" :key="index">
                <v-list-item-content>
                  {{ item.id }}
                </v-list-item-content>
              </v-list-item>
            </v-list-item-group>
          </v-flex>
          <v-flex xs12 sm6 md4>
            <v-list-item-group v-model="selectedModulePosition">
              <template v-for="(item, index) in moduleList">
                <v-list-item :key="index">
                  <v-list-item-icon @click="addModuleToUser(item)">
                    <fa-icon icon="check" v-if="userHasModule(item)"></fa-icon>
                    <fa-icon icon="times" v-else></fa-icon>
                  </v-list-item-icon>
                  <v-list-item-content>
                    {{ item.name }}
                  </v-list-item-content>
                </v-list-item>
              </template>
            </v-list-item-group>
          </v-flex>
          <v-flex xs12 sm6 md4>
            <v-list
              v-if="
                typeof selectedModule === 'object' && selectedModule !== null
              "
            >
              <template v-for="(item, index) in selectedModule.features">
                <v-list-item :key="index">
                  <v-list-item-icon @click="addFeatureToUser(item)">
                    <fa-icon icon="check" v-if="userHasFeature(item)"></fa-icon>
                    <fa-icon icon="times" v-else></fa-icon>
                  </v-list-item-icon>
                  <v-list-item-content>
                    {{ item.name }}
                  </v-list-item-content>
                </v-list-item>
              </template>
            </v-list>
          </v-flex>
        </v-layout>
      </v-card-text>
    </v-card>
  </v-container>
</template>

<script>
import authService from '../../services/auth'
import moduleService from '../../services/modules'
import DispatchFactory from '../../factories/dispatchFactory'

export default {
  data() {
    return {
      selectedModule: null,
      selectedUser: null,
      selectedUserPosition: null,
      selectedModulePosition: null,
      moduleList: [],
      userList: [],
      usersHaveModuleList: [],
      selectedUserModuleList: [],
      selectedUserFeatureList: [],
    }
  },
  mounted() {
    this.getData()
  },
  watch: {
    selectedUserPosition(newValue) {
      if (typeof newValue === 'undefined' || newValue === null) {
        this.selectedUser = null
        this.selectedModule = null
        return
      }

      this.selectedModule = null
      this.selectedModulePosition = null

      this.selectedUser = this.userList[newValue]
      this.updateSelectedUserList(this.userList[newValue])
    },
    selectedModulePosition(newValue) {
      if (typeof newValue === 'undefined' || newValue === null) {
        this.selectedModule = null
        return
      }

      this.selectedModule = this.moduleList[newValue]
    },
  },
  methods: {
    getData() {
      DispatchFactory.request(() => {
        authService
          .getUsers()
          .then(resp => {
            this.$_console_log(
              '[admin-tools] getData: Successfully got user list'
            )
            this.userList = resp.data
          })
          .catch(() =>
            this.$_console_log('[admin-tools] getData: Failed to get user list')
          )
      })

      DispatchFactory.request(() => {
        moduleService
          .getAllModules()
          .then(resp => {
            this.$_console_log(
              '[admin-tools] getData: Successfully got module list'
            )
            this.moduleList = resp.data
          })
          .catch(() =>
            this.$_console_log(
              '[admin-tools] getData: Failed to get module list'
            )
          )
      })

      DispatchFactory.request(() => {
        moduleService
          .getAllModulesForAllUser()
          .then(resp => {
            this.$_console_log(
              '[admin-tools] getData: Successfully got user has module list'
            )
            this.usersHaveModuleList = resp.data
          })
          .catch(() =>
            this.$_console_log(
              '[admin-tools] getData: Failed to get module lists for users'
            )
          )
      })
    },
    deleteModuleFromUser(module) {
      if (
        typeof this.selectedUser === 'undefined' ||
        this.selectedUser === null
      ) {
        this.$_console_log(
          '[admin-tools] deleteModuleFromUser: selected user is null'
        )
        return false
      }

      if (typeof module === 'undefined' || module === null) {
        this.$_console_log('[admin-tools] deleteModuleFromUser: module is null')
        return false
      }

      let tempUser = Object.assign({}, this.selectedUser)
      let obj = {
        userId: this.selectedUser.id,
        moduleAddOnId: module.id,
      }

      DispatchFactory.request(() => {
        moduleService
          .deleteModuleFromUser(obj)
          .then(() => {
            this.$_console_log(
              '[admin-tools] deleteModuleFromUser: Successfully deleted module from user'
            )
            const index = this.usersHaveModuleList[obj.userId].findIndex(
              x => x.id === module.id
            )
            if (index < 0) {
              // Failed somehow...
              this.$_console_log(
                "[admmin-tools] deleteModuleFromuser: Index doesn't exist"
              )
            } else {
              this.usersHaveModuleList[obj.userId].splice(index, 1)
              this.updateSelectedUserList(tempUser)
            }
          })
          .catch(() =>
            this.$_console_log(
              '[admin-tools] deleteModuleFromUser: Failed to delete module from user'
            )
          )
      })
    },
    deleteFeatureFromUser(feature) {
      if (
        typeof this.selectedModule === 'undefined' ||
        this.selectedModule === null
      ) {
        this.$_console_log(
          '[admin-tools] deleteFeatureFromUser: selected user is null'
        )
        return false
      }

      if (typeof feature === 'undefined' || feature === null) {
        this.$_console_log(
          '[admin-tools] deleteFeatureFromUser: module is null'
        )
        return false
      }

      let tempUser = Object.assign({}, this.selectedUser)
      let obj = {
        userId: tempUser.id,
        moduleFeatureId: feature.id,
      }

      let currentlySelectedModule = Object.assign({}, this.selectedModule)

      DispatchFactory.request(() => {
        moduleService
          .deleteFeatureFromUser(obj)
          .then(() => {
            this.$_console_log(
              '[admin-tools] deleteFeatureFromUser: Successfully deleted feature from user'
            )

            const moduleIndex = this.usersHaveModuleList[obj.userId].findIndex(
              x => x.id === currentlySelectedModule.id
            )
            if (moduleIndex < 0) {
              // It failed somehow
              this.$_console_log(
                "[admmin-tools] deleteFeatureFromUser: Module index doesn't exist"
              )
            } else {
              if (
                !Array.isArray(
                  this.usersHaveModuleList[obj.userId][moduleIndex].features
                )
              ) {
                // It failed somehow
                this.$_console_log(
                  "[admmin-tools] deleteFeatureFromUser: Selected user doesn't have any features for this module"
                )
              } else {
                const featureIndex = this.usersHaveModuleList[obj.userId][
                  moduleIndex
                ].features.findIndex(x => x.id === feature.id)
                this.$_console_log(
                  '[admin-tools] deleteFeatureFromUser: Got feature id, ',
                  featureIndex
                )
                if (featureIndex < 0) {
                  // It failed somehow
                  this.$_console_log(
                    "[admmin-tools] deleteFeatureFromUser: Feature index doesn't exist"
                  )
                } else {
                  this.usersHaveModuleList[obj.userId][
                    moduleIndex
                  ].features.splice(featureIndex, 1)
                }
              }
            }
          })
          .catch(ex =>
            this.$_console_log(
              '[admin-tools] deleteFeatureFromUser: Failed to delete feature from user',
              ex
            )
          )
      })
    },
    addModuleToUser(module) {
      if (this.userHasModule(module)) {
        this.deleteModuleFromUser(module)
        return
      }

      let tempUser = Object.assign({}, this.selectedUser)
      let obj = {
        userId: tempUser.id,
        moduleAddOnId: module.id,
      }

      const tempModule = Object.assign({}, module)
      DispatchFactory.request(() => {
        moduleService
          .addModuleToUser(obj)
          .then(resp => {
            if (resp.data === true) {
              this.$_console_log(
                '[admin-tools] addModuleToUser: Successfully added module to user'
              )

              if (!Array.isArray(this.usersHaveModuleList[obj.userId]))
                this.usersHaveModuleList[obj.userId] = []

              tempModule.features = []
              this.usersHaveModuleList[obj.userId].push(tempModule)
              this.updateSelectedUserList(tempUser)
            }
          })
          .catch(() =>
            this.$_console_log(
              '[admin-tools] addModuleToUser: Failed to get add module to user'
            )
          )
      })
    },
    addFeatureToUser(feature) {
      if (this.userHasFeature(feature)) {
        this.deleteFeatureFromUser(feature)
        return
      }

      let tempUser = Object.assign({}, this.selectedUser)
      let obj = {
        userId: this.selectedUser.id,
        moduleFeatureId: feature.id,
      }

      if (!Array.isArray(this.usersHaveModuleList[obj.userId])) {
        this.$_console_log(
          "[admin-tools] addFeatureToUser: Can't add a feature to a user that doesn't have any modules"
        )
        return
      }

      let currentlySelectedModule = Object.assign({}, this.selectedModule)
      let moduleIndex = this.usersHaveModuleList[obj.userId].findIndex(
        x => x.id === currentlySelectedModule.id
      )
      if (moduleIndex < 0) {
        this.$_console_log(
          "[admin-tools] addFeatureToUser: Can't add a feature to a user that doesn't have the corresponding module"
        )
        return
      }

      DispatchFactory.request(() => {
        moduleService
          .addFeatureToUser(obj)
          .then(resp => {
            if (resp.data === true) {
              this.$_console_log(
                '[admin-tools] addFeatureToUser: Successfully added feature to user'
              )

              if (
                !Array.isArray(
                  this.usersHaveModuleList[obj.userId][moduleIndex].features
                )
              )
                this.usersHaveModuleList[obj.userId][moduleIndex].features = []

              this.usersHaveModuleList[obj.userId][moduleIndex].features.push(
                feature
              )
              this.updateSelectedUserList(tempUser)
            }
          })
          .catch(() =>
            this.$_console_log(
              '[admin-tools] addFeatureToUser: Failed to get add feature to user'
            )
          )
      })
    },
    userHasModule(module) {
      if (
        typeof this.selectedUser === 'undefined' ||
        this.selectedUser === null
      ) {
        this.$_console_log('[admin-tools] userHasModule: selected user is null')
        return false
      }

      if (typeof module === 'undefined' || module === null) {
        this.$_console_log('[admin-tools] userHasModule: module is null')
        return false
      }

      if (!Array.isArray(this.selectedUserModuleList)) {
        this.$_console_log(
          '[admin-tools] userHasModule: selected user module list is not an array'
        )
        return false
      }

      return this.selectedUserModuleList.findIndex(x => x.id === module.id) >= 0
    },
    userHasFeature(feature) {
      if (
        typeof this.selectedUser === 'undefined' ||
        this.selectedUser === null
      ) {
        this.$_console_log(
          '[admin-tools] userHasFeature: selected user is null'
        )
        return false
      }
      if (
        typeof this.selectedModule === 'undefined' ||
        this.selectedModule === null
      ) {
        this.$_console_log(
          '[admin-tools] userHasFeature: selected module is null'
        )
        return false
      }
      if (typeof feature === 'undefined' || feature === null) {
        this.$_console_log('[admin-tools] userHasFeature: feature is null')
        return false
      }

      if (!Array.isArray(this.selectedUserModuleList)) {
        this.$_console_log(
          '[admin-tools] userHasModule: selected user module list is not an array'
        )
        return false
      }

      let moduleIndex = this.selectedUserModuleList.findIndex(
        x => x.id === this.selectedModule.id
      )
      if (moduleIndex < 0) {
        return false
      }

      if (Array.isArray(this.selectedUserModuleList[moduleIndex].features)) {
        let featureIndex = this.selectedUserModuleList[
          moduleIndex
        ].features.findIndex(x => x.id === feature.id)
        if (featureIndex >= 0) {
          return true
        }
      }

      return false
    },
    updateSelectedUserList(value) {
      this.$_console_log('Update Selected User List')
      if (typeof value === 'undefined' || value === null) {
        this.selectedUserModuleList = []
        return
      }

      this.selectedUserModuleList = this.usersHaveModuleList[value.id]
    },
  },
}
</script>

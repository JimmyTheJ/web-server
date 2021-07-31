<template>
  <div>
    <v-overlay v-show="loadingContents" absolute opacity="0.50" z-index="9999">
      <v-progress-circular
        :size="200"
        :width="16"
        color="primary"
        indeterminate
        class="browser-loading-circle"
      ></v-progress-circular>
    </v-overlay>

    <generic-dialog
      :title="deleteTitle"
      :open="deleteDialog"
      :maxWidth="800"
      @dialog-close="closeDeleteConfirmation"
    >
      <v-card v-if="fileToDelete !== null">
        <v-card-text>
          <v-layout row wrap>
            <v-flex xs12>
              <span class="body-1">Are you sure you want to delete: </span>
              <span class="body-1" style="font-weight: bold">
                {{ fileToDelete.title }}
              </span>
            </v-flex>
            <v-flex xs12 class="text-center mt-3">
              <v-btn color="secondary" @click="deleteItem()"> DELETE </v-btn>
            </v-flex>
          </v-layout>
        </v-card-text>
      </v-card>
    </generic-dialog>

    <file-upload v-if="features.upload === true"></file-upload>

    <v-container>
      <v-form v-if="selectable">
        <v-layout row wrap px-2>
          <v-flex xs12>
            <v-select
              v-model="selectedDirectory"
              :items="folders"
              :label="`Select a folder`"
              item-text="name"
              item-value="name"
            ></v-select>
          </v-flex>
        </v-layout>
      </v-form>
      <v-card>
        <v-container>
          <v-layout row wrap>
            <v-flex xs1>
              <v-btn v-if="canGoBack" icon @click="goBack"
                ><fa-icon icon="arrow-left" ma-2 pa-2></fa-icon
              ></v-btn>
            </v-flex>
            <v-flex xs11>
              {{ fullPath }}
            </v-flex>
          </v-layout>
        </v-container>
      </v-card>

      <v-list-item v-for="item in contents" :key="item.id">
        <v-list-item-action>
          <a :href="getDownloadPath(item)" download
            ><fa-icon icon="download"
          /></a>
        </v-list-item-action>
        <v-list-item-avatar>
          <fa-icon :icon="getIcon(item)" size="2x" style="color: gray" />
        </v-list-item-avatar>
        <v-list-item-content
          @click="open(item)"
          :class="{ 'hide-extra': $vuetify.breakpoint.xsOnly ? true : false }"
        >
          <tooltip :value="item.title"></tooltip>
        </v-list-item-content>
        <v-list-item-action v-if="item.size > 0" class="hidden-xs-only">
          {{ getFileSize(item.size) }}
        </v-list-item-action>
        <!-- Ensure admin and not a folder for delete -->
        <v-list-item-action v-if="features.delete">
          <v-btn icon @click="openDeleteConfirmation(item)"
            ><fa-icon size="lg" icon="window-close"
          /></v-btn>
        </v-list-item-action>
      </v-list-item>

      <v-layout row justify-center class="loading-container">
        <v-progress-circular
          v-show="loading"
          indeterminate
          color="purple"
          :width="12"
          :size="120"
        ></v-progress-circular>
      </v-layout>
    </v-container>
  </div>
</template>

<script>
import service from '../../services/file-explorer'
import { mapState } from 'vuex'

import Tooltip from './tooltip.vue'
import FileUpload from './file-upload.vue'
import GenericDialog from './generic-dialog.vue'

import Auth from '../../mixins/authentication'

import { getSubdirectoryString } from '../../helpers/browser'

let path = process.env.VUE_APP_API_URL
const FN = 'FILE EXPLORER'

export default {
  data() {
    return {
      selectedDirectory: '',

      loading: false,
      changing: false,

      deleteDialog: false,
      fileToDelete: null,

      features: {
        upload: false,
        delete: false,
        viewing: false,
      },
    }
  },
  mixins: [Auth],
  components: {
    Tooltip,
    FileUpload,
    GenericDialog,
  },
  props: {
    selectable: {
      type: Boolean,
      default: true,
    },
    parentView: {
      type: String,
      required: true,
    },
  },
  created() {
    this.setActiveFeatures()
  },
  async mounted() {
    if (
      !Array.isArray(this.folders) ||
      (Array.isArray(this.folders) && this.folders.length === 0)
    )
      await this.$store.dispatch('getFolders')

    this.getDirectoryFromRoute()
  },
  computed: {
    ...mapState({
      contents: state => state.fileExplorer.contents,
      folders: state => state.fileExplorer.folders,
      directory: state => state.fileExplorer.directory,
      subDirectories: state => state.fileExplorer.subDirectories,
      loadingContents: state => state.fileExplorer.loadingContents,
      activeModules: state => state.auth.activeModules,
    }),
    fullPath() {
      if (typeof this.directory !== 'undefined' && this.directory !== null) {
        if (
          Array.isArray(this.subDirectories) &&
          this.subDirectories.length > 0
        ) {
          // TODO: Fix to build the list of subdirs
          return `${this.directory}/${getSubdirectoryString(
            this.subDirectories
          )}`
        } else {
          return this.directory
        }
      } else {
        return ''
      }
    },
    canGoBack() {
      if (typeof this.$route.params.folder === 'undefined') return false
      return this.$route.params.folder !== this.selectedDirectory
    },
    deleteTitle() {
      if (this.fileToDelete !== null && this.fileToDelete.isFolder) {
        return 'Delete Folder'
      } else {
        return 'Delete File'
      }
    },
  },
  watch: {
    // Local prop
    selectedDirectory: function(newValue) {
      if (!this.changing) {
        this.$_console_log(
          '[FileExplorer] Watcher - Selected directory: ',
          newValue
        )

        this.$router.push({
          name: 'browser-folder',
          params: { folder: newValue },
        })
      }
    },
  },
  methods: {
    setActiveFeatures() {
      const browserObj = this.activeModules.find(x => x.id === 'browser')
      if (
        typeof browserObj === 'undefined' ||
        !Array.isArray(browserObj.userModuleFeatures)
      )
        return

      if (
        browserObj.userModuleFeatures.some(
          x => x.moduleFeatureId === 'browser-upload'
        )
      )
        this.features.upload = true

      if (
        browserObj.userModuleFeatures.some(
          x => x.moduleFeatureId === 'browser-delete'
        )
      )
        this.features.delete = true

      if (
        browserObj.userModuleFeatures.some(
          x => x.moduleFeatureId === 'browser-viewer'
        )
      )
        this.features.viewing = true
    },
    getDirectoryFromRoute() {
      this.changing = true

      setTimeout(() => {
        if (typeof this.$route.params.folder !== 'undefined') {
          this.$_console_log(
            '[file-explorer] getDirectoryFromRoute: Params.folder is not null, getting base path from url',
            this.$route.params.folder
          )
          if (this.$route.params.folder.includes('/'))
            this.selectedDirectory = this.$route.params.folder.substring(
              0,
              this.$route.params.folder.indexOf('/')
            )
          else this.selectedDirectory = this.$route.params.folder
        } else {
          // Load default folder
          const defaultFolder = this.folders.find(x => x.default === true)
          if (typeof defaultFolder !== 'undefined') {
            this.selectedDirectory = defaultFolder.name
            this.$router.push({
              name: 'browser-folder',
              params: { folder: this.selectedDirectory },
            })
          }
        }

        setTimeout(() => {
          this.changing = false
        }, 5)
      }, 25)
    },

    open(item) {
      if (item.isFolder) {
        let path = ''
        if (this.$route.params.folder === 'undefined') {
          this.$_console_log('[file-explorer] open: Params.folder is null')
          return
        } else {
          path = this.$route.params.folder + '/' + item.title
        }

        this.$store.dispatch('loadFile', null)
        this.$router.push({ name: 'browser-folder', params: { folder: path } })
      } else {
        if (this.features.viewing) {
          this.$_console_log(
            `[${FN}] Open Item: item, dl path`,
            item,
            this.getFilePath(item)
          )
          setTimeout(() => {
            this.$store.dispatch('loadFile', item)
          }, 125)
        }
      }
    },
    goBack() {
      let path = ''
      if (this.$route.params.folder === 'undefined') {
        this.$_console_log('[file-explorer] goBack: Params.folder is null')
        return
      } else {
        let lastIndex = this.$route.params.folder.lastIndexOf('/')
        if (lastIndex === -1) {
          this.$_console_log(
            "[file-explorer] goBack: Params.folder has no / in it. Can't go back from base path"
          )
          return
        }

        path = this.$route.params.folder.substring(0, lastIndex)
      }

      this.$router.push({ name: 'browser-folder', params: { folder: path } })
    },

    getDownloadPath(item) {
      return `${path}/api/directory/download/file/${encodeURI(
        this.fullPath
      )}/${encodeURIComponent(item.title)}?token=${
        this.$store.state.auth.accessToken
      }`
    },
    getFilePath(item) {
      return `${encodeURI(this.fullPath)}/${encodeURIComponent(item.title)}`
    },
    getIcon(item) {
      if (item.isFolder) return 'folder'
      else return 'file'
    },
    getFileSize(size) {
      let type = 0

      while (size / 1024 > 1) {
        size = size / 1024
        type++
      }

      return `${size.toFixed(2)} ${this.getType(type)}`
    },
    getType(type) {
      switch (type) {
        case 0:
          return 'B'
        case 1:
          return 'KB'
        case 2:
          return 'MB'
        case 3:
          return 'GB'
        case 4:
          return 'TB'
      }
    },

    closeDeleteConfirmation() {
      this.fileToDelete = null
      this.deleteDialog = false
    },

    openDeleteConfirmation(file) {
      if (this.deleteEnabled === false) {
        return this.$_console_log(
          'You must have the appropriate permission to delete'
        )
      }

      if (typeof file === 'undefined' || file === null || file === '')
        return this.$_console_log("Invalid file info, can't delete")

      this.fileToDelete = file
      this.deleteDialog = true
    },

    async deleteItem() {
      if (this.deleteEnabled === false) {
        return this.$_console_log(
          'You must have the appropriate permission to delete'
        )
      }

      const file = this.fileToDelete
      if (typeof file === 'undefined' || file === null || file === '')
        return this.$_console_log("Invalid file info, can't delete")

      this.$_console_log(`Delete: ${file.title}`)

      await service
        .deleteFile(
          file.title,
          this.directory,
          getSubdirectoryString(this.subDirectories)
        )
        .then(resp => {
          this.$_console_log('Successfully deleted the file')
          this.$store.dispatch('deleteFile', file.title)
        })
        .catch(() => this.$_console_log('Error deleting the item in upload'))
        .then(() => {
          this.deleteDialog = false
          this.fileToDelete = null
        })
    },
  },
}
</script>

<style>
.loading-container {
  position: fixed;
  left: 50%;
  top: 50%;
}

.hide-extra {
  white-space: nowrap;
}
</style>

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
      @dialog-close="deleteDialog = false"
    >
      <v-card v-if="activeContextMenu > -1">
        <v-card-text>
          <v-layout row wrap>
            <v-flex xs12>
              <span class="body-1">Are you sure you want to delete: </span>
              <span class="body-1" style="font-weight: bold">
                {{ contents[activeContextMenu].title }}
              </span>
            </v-flex>
            <v-flex xs12 class="text-center mt-3">
              <v-btn color="secondary" @click="deleteItem()"> DELETE </v-btn>
            </v-flex>
          </v-layout>
        </v-card-text>
      </v-card>
    </generic-dialog>

    <generic-dialog
      title="Create new Folder"
      :open="createFolderDialog"
      :maxWidth="800"
      @dialog-close="createFolderDialog = false"
    >
      <v-card>
        <v-card-text>
          <v-layout row wrap>
            <v-flex xs12>
              <v-text-field
                v-model="newFolderName"
                label="Folder Name"
              ></v-text-field>
            </v-flex>
            <v-flex xs12 class="text-center mt-3">
              <v-btn color="secondary" @click="createFolder()"> SUBMIT </v-btn>
            </v-flex>
            <v-flex xs12 class="headline red--text text-center">
              {{ createFolderError }}
            </v-flex>
          </v-layout>
        </v-card-text>
      </v-card>
    </generic-dialog>

    <generic-dialog
      title="Rename File"
      :open="renameFileDialog"
      :maxWidth="800"
      @dialog-close="renameFileDialog = false"
    >
      <v-card v-if="activeContextMenu > -1">
        <v-card-text>
          <v-layout row wrap>
            <v-flex xs12>
              <v-text-field
                v-model="tempFileRenameField"
                label="File Name"
              ></v-text-field>
            </v-flex>
            <v-flex xs12 class="text-center mt-3">
              <v-btn color="secondary" @click="renameFile()"> SUBMIT </v-btn>
            </v-flex>
            <v-flex xs12 class="headline red--text text-center">
              {{ renameFileError }}
            </v-flex>
          </v-layout>
        </v-card-text>
      </v-card>
    </generic-dialog>

    <file-upload v-if="features.upload === true"></file-upload>

    <v-card v-if="features.create">
      <v-container class="text-center py-4">
        <v-layout row wrap>
          <v-flex xs12>
            <v-btn @click="createFolderDialog = true" color="green">
              Create New Folder
            </v-btn>
          </v-flex>
        </v-layout>
      </v-container>
    </v-card>

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

      <v-list-item v-for="(item, i) in contents" :key="i">
        <v-list-item-action>
          <a :href="getDownloadPath(item)" download
            ><fa-icon icon="download"
          /></a>
        </v-list-item-action>
        <v-list-item-avatar>
          <fa-icon :icon="getIcon(item)" size="2x" style="color: gray" />
        </v-list-item-avatar>
        <v-list-item-content
          @click.left.exact="open(item)"
          @click.right.exact="activeContextMenu = i"
          @contextmenu.prevent="openContextMenu"
          :class="{
            'hide-extra': $vuetify.breakpoint.xsOnly ? true : false,
            'pointer-arrow': true,
          }"
        >
          <v-menu
            v-model="showMenu"
            :position-x="contextMenuX"
            :position-y="contextMenuY"
            offset-y
            absolute
          >
            <v-list>
              <v-list-item>
                <v-list-item-title>
                  <v-btn
                    :disabled="!canRename(item)"
                    @click="renameFileDialog = true"
                    >Rename</v-btn
                  >
                </v-list-item-title>
              </v-list-item>
              <v-list-item>
                <v-list-item-title>
                  <v-btn :disabled="true">Move</v-btn>
                </v-list-item-title>
              </v-list-item>
              <v-list-item>
                <v-list-item-title>
                  <v-btn
                    :disabled="!canDelete(item)"
                    @click="deleteDialog = true"
                    >Delete</v-btn
                  >
                </v-list-item-title>
              </v-list-item>
              <v-list-item>
                <v-list-item-title>
                  <v-btn :disabled="true">Properties</v-btn>
                </v-list-item-title>
              </v-list-item>
            </v-list>
          </v-menu>
          <tooltip :value="item.title"></tooltip>
        </v-list-item-content>
        <v-list-item-action v-if="item.size > 0" class="hidden-xs-only">
          {{ getFileSize(item.size) }}
        </v-list-item-action>
        <!-- Ensure admin and not a folder for delete -->
        <!-- <v-list-item-action v-if="features.delete">
          <v-btn icon @click="openDeleteConfirmation(item)"
            ><fa-icon size="lg" icon="window-close"
          /></v-btn>
        </v-list-item-action> -->
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
import { DirectoryAccessFlags } from '@/constants.js'

let path = process.env.VUE_APP_API_URL
const FN = 'FILE EXPLORER'

export default {
  data() {
    return {
      selectedDirectory: '',
      newFolderName: null,
      createFolderError: null,
      renameFileError: null,

      tempFileRenameField: null,
      showMenu: null,
      activeContextMenu: -1,
      contextMenuX: 0,
      contextMenuY: 0,

      loading: false,
      changing: false,

      deleteDialog: false,
      createFolderDialog: false,
      renameFileDialog: false,

      features: {
        upload: false,
        delete: false,
        viewing: false,
        create: false,
        move: false,
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
      if (
        this.activeContextMenu > -1 &&
        this.contents[this.activeContextMenu].isFolder
      ) {
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
    newFolderName: function(newValue) {
      if (this.createFolderError !== null) {
        this.createFolderError = null
      }
    },
    renameFileDialog: function(newValue) {
      if (newValue === false) {
        this.renameFileError = null
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

      if (
        browserObj.userModuleFeatures.some(
          x => x.moduleFeatureId === 'browser-create'
        )
      )
        this.features.create = true

      if (
        browserObj.userModuleFeatures.some(
          x => x.moduleFeatureId === 'browser-move'
        )
      )
        this.features.move = true
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
    openContextMenu(e) {
      this.$_console_log(
        '[file-explorer] openContextMenu: Opening Rename Menu by right clicking!'
      )

      // TODO
      this.showMenu = true
      this.contextMenuX = e.clientX
      this.contextMenuY = e.clientY

      this.$nextTick(() => {
        if (this.activeContextMenu > -1)
          this.tempFileRenameField = this.contents[this.activeContextMenu].title
      })
    },
    renameFile() {
      this.$store
        .dispatch('renameFile', {
          oldName: this.contents[this.activeContextMenu].title,
          newName: this.tempFileRenameField,
          dir: this.directory,
          subDir: getSubdirectoryString(this.subDirectories),
        })
        .then(resp => {
          this.renameFileDialog = false
        })
        .catch(() => {
          this.renameFileError = 'Failed to rename file'
        })
    },
    canDelete(item) {
      if (!this.features.delete) {
        return false
      }

      let dir = this.folders.find(x => x.name === this.selectedDirectory)
      if (dir === undefined) {
        return false
      }

      if (item.isFolder) {
        if (dir.accessFlags & DirectoryAccessFlags.DeleteFolder) {
          return true
        }
      } else {
        if (dir.accessFlags & DirectoryAccessFlags.DeleteFile) {
          return true
        }
      }

      return false
    },
    canRename(item) {
      if (!this.features.move) {
        return false
      }

      let dir = this.folders.find(x => x.name === this.selectedDirectory)
      if (dir === undefined) {
        return false
      }

      if (item.isFolder) {
        if (dir.accessFlags & DirectoryAccessFlags.MoveFolder) {
          return true
        }
      } else {
        if (dir.accessFlags & DirectoryAccessFlags.MoveFile) {
          return true
        }
      }

      return false
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

    createFolder() {
      if (this.contents.findIndex(x => x.title === this.newFolderName) > -1) {
        this.$_console_log(`Can't create a folder that already exists`)
        this.createFolderError = 'Folder already exists'
        return
      }

      service
        .createFolder(
          this.newFolderName,
          this.directory,
          getSubdirectoryString(this.subDirectories)
        )
        .then(resp => {
          this.$store.dispatch('addFile', resp.data)
        })
        .catch(() => {
          this.$_console_log('Failed to create folder')
        })
        .then(() => {
          this.newFolderName = null
          this.createFolderDialog = false
          this.createFolderError = null
        })
    },

    async deleteItem() {
      const file = this.contents[this.activeContextMenu]
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

.pointer-arrow {
  cursor: pointer;
}
</style>

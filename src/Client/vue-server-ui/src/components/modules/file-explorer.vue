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
      :title="fileActionTitle"
      :open="fileActionDialog"
      :maxWidth="800"
      @dialog-close="fileActionDialog = false"
    >
      <v-card>
        <v-card-text>
          <v-layout
            row
            wrap
            v-if="
              fileActionActive === operationType.move ||
                fileActionActive === operationType.rename ||
                fileActionActive === operationType.createFolder
            "
          >
            <v-flex xs12>
              <v-text-field
                v-model="fileActionFieldValue"
                :label="fileActionFieldLabel"
              ></v-text-field>
            </v-flex>
            <v-flex xs12 class="text-center mt-3">
              <v-btn color="secondary" @click="fileModificationAction()">
                SUBMIT
              </v-btn>
            </v-flex>
            <v-flex xs12 class="headline red--text text-center">
              {{ fileActionError }}
            </v-flex>
          </v-layout>
          <v-layout row wrap v-if="fileActionActive === operationType.delete">
            <v-flex xs12>
              <span class="body-1">Are you sure you want to delete: </span>
              <span class="body-1" style="font-weight: bold">
                {{ fileActionFieldValue }}
              </span>
            </v-flex>
            <v-flex xs12 class="text-center mt-3">
              <v-btn color="secondary" @click="fileModificationAction()">
                DELETE
              </v-btn>
            </v-flex>
          </v-layout>
        </v-card-text>
      </v-card>
    </generic-dialog>

    <file-upload v-if="features.upload === true"></file-upload>

    <v-menu
      v-model="showMenu"
      :position-x="contextMenuX"
      :position-y="contextMenuY"
      offset-y
      absolute
    >
      <v-list>
        <template v-for="(contextMenuItem, i) in contextMenuMap">
          <v-list-item
            :key="i"
            :disabled="!contextMenuItem.disabled(selectedMenuItem)"
            :class="{
              'clickable-context-menu-item': contextMenuItem.disabled(
                selectedMenuItem
              ),
            }"
          >
            <v-list-item-title>
              <div @click="contextMenuItem.action(selectedMenuItem)">
                {{ contextMenuItem.title }}
              </div>
            </v-list-item-title>
          </v-list-item>
        </template>
      </v-list>
    </v-menu>

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

      <v-list>
        <v-list-item
          v-for="(item, i) in updatedContents"
          :key="i"
          @click.right.exact="
            fileActionIndex = i
            selectedMenuItem = item
          "
          @contextmenu.prevent="openContextMenu"
        >
          <v-list-item-action>
            <a v-if="item.title !== null" :href="getDownloadPath(item)" download
              ><fa-icon icon="download"
            /></a>
          </v-list-item-action>
          <v-list-item-avatar
            v-if="item.title !== null"
            @click.left.exact="open(item)"
            :class="{
              'hide-extra': $vuetify.breakpoint.xsOnly ? true : false,
              'pointer-arrow': true,
            }"
          >
            <fa-icon :icon="getIcon(item)" size="2x" style="color: gray" />
          </v-list-item-avatar>
          <v-list-item-content
            @click.left.exact="open(item)"
            :class="{
              'hide-extra': $vuetify.breakpoint.xsOnly ? true : false,
              'pointer-arrow': item.isFolder !== null,
            }"
          >
            <tooltip :value="item.title"></tooltip>
          </v-list-item-content>
          <v-list-item-action v-if="item.size > 0" class="hidden-xs-only">
            {{ getFileSize(item.size) }}
          </v-list-item-action>
        </v-list-item>
      </v-list>

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
import { faTemperatureLow } from '@fortawesome/free-solid-svg-icons'

let path = process.env.VUE_APP_API_URL
const FN = 'FILE EXPLORER'

let count = 0
const opType = {
  move: ++count,
  rename: ++count,
  copy: ++count,
  paste: ++count,
  move: ++count,
  delete: ++count,
  properties: ++count,
  createFolder: ++count,
}

export default {
  data() {
    return {
      selectedDirectory: '',

      fileActionFieldValue: null,
      fileActionError: null,
      fileActionDialog: false,
      fileActionIndex: -1,
      fileActionActive: -1,

      selectedMenuItem: null,

      showMenu: null,
      contextMenuX: 0,
      contextMenuY: 0,

      loading: false,
      changing: false,

      moveMode: false,

      features: {
        upload: false,
        delete: false,
        viewing: false,
        create: false,
        move: false,
      },

      operationType: opType,

      contextMenuMap: [
        {
          title: 'New Folder',
          disabled: item => this.canMakeFolder(),
          action: item => this.newFolderItem(item),
        },
        {
          title: 'Rename',
          disabled: item => this.canRename(item),
          action: item => this.renameItem(item),
        },
        {
          title: 'Copy',
          disabled: item => this.canCopy(item),
          action: item => this.copyPasteItem(item, 'copy'),
        },
        {
          title: 'Paste',
          disabled: item => this.canPaste(item),
          action: item => this.copyPasteItem(item, 'paste'),
        },
        {
          title: 'Move',
          disabled: item => false, // TODO: Add this back in \\ this.canMove(item),
          action: item => this.moveItem(item),
        },
        {
          title: 'Delete',
          disabled: item => this.canDelete(item),
          action: item => this.deleteItem(item),
        },
        {
          title: 'Properties',
          disabled: item => false,
          action: item => () => {
            this.$_console_log('Properties not implemented yet')
          },
        },
      ],
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
      copied: state => state.fileExplorer.copied,
      activeModules: state => state.auth.activeModules,
    }),
    updatedContents() {
      if (this.contents.length === 0) {
        return [
          {
            active: false,
            extension: null,
            isFolder: null,
            size: null,
            title: null,
          },
        ]
      } else {
        return this.contents
      }
    },
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
    fileActionTitle() {
      if (this.fileActionActive > -1 && this.fileActionIndex > -1) {
        if (this.updatedContents[this.fileActionIndex].isFolder) {
          switch (this.fileActionActive) {
            case this.operationType.move:
              return 'Move Folder'
            case this.operationType.rename:
              return 'Rename Folder'
            case this.operationType.delete:
              return 'Delete Folder'
            case this.operationType.properties:
              return 'Properties'
          }
        } else {
          switch (this.fileActionActive) {
            case this.operationType.move:
              return 'Move File'
            case this.operationType.rename:
              return 'Rename File'
            case this.operationType.delete:
              return 'Delete File'
            case this.operationType.properties:
              return 'Properties'
          }
        }
      }

      if (this.operationType.createFolder) return 'Create Folder'
      else 'N/A'
    },
    fileActionFieldLabel() {
      if (this.fileActionActive > -1 && this.fileActionIndex > -1) {
        if (this.updatedContents[this.fileActionIndex].isFolder) {
          switch (this.fileActionActive) {
            case this.operationType.move:
              return 'New Folder Path'
            case this.operationType.copy:
              return 'Copy Folder Location'
            case this.operationType.rename:
              return 'New Folder Name'
            case this.operationType.delete:
              return 'Folder to Delete'
          }
        } else {
          switch (this.fileActionActive) {
            case this.operationType.move:
              return 'New File Path'
            case this.operationType.copy:
              return 'Copy File Location'
            case this.operationType.rename:
              return 'New File Name'
            case this.operationType.delete:
              return 'File To Delete'
          }
        }
      }

      if (this.operationType.createFolder) return 'Create Folder'
      else 'N/A'
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
    fileActionDialog: function(newValue) {
      if (newValue === false) {
        // Reset all the file action fields and inputs
        this.fileActionFieldValue = null
        this.fileActionError = null
        this.fileActionIndex = -1
        this.fileActionActive = -1
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

      this.$nextTick(() => {
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

        this.$nextTick(() => {
          this.changing = false
        })
      })
    },

    open(item) {
      if (item.isFolder === null) {
        return
      }

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
    },
    renameItem(item) {
      this.$_console_log('Rename Item:', item)

      this.fileActionActive = this.operationType.rename
      this.fileActionDialog = faTemperatureLow
      this.preloadItemName()
    },
    deleteItem(item) {
      this.$_console_log('Delete Item:', item)

      this.fileActionActive = this.operationType.delete
      this.fileActionDialog = true
      this.preloadItemName()
    },
    moveItem(item) {
      this.$_console_log('Move Item:', item)

      this.fileActionActive = this.operationType.move
      this.fileActionDialog = true
      this.preloadItemName()
    },
    newFolderItem(item) {
      this.$_console_log('Create New Folder:', item)

      this.fileActionActive = this.operationType.createFolder
      this.fileActionDialog = true
      this.fileActionFieldValue = ''
    },
    copyPasteItem(item, type) {
      this.$_console_log(`${type}:`, item)

      let obj = {
        name: type === 'copy' ? item.title : this.copied.name,
        directory: this.directory,
        subDirectory: getSubdirectoryString(this.subDirectories),
        isFolder: type === 'copy' ? item.isFolder : this.copied.isFolder,
      }

      this.$store.dispatch(`${type}File`, obj)
    },
    preloadItemName() {
      this.$nextTick(() => {
        this.$_console_log(this.fileActionIndex)
        if (this.fileActionIndex > -1)
          this.fileActionFieldValue = this.updatedContents[
            this.fileActionIndex
          ].title
      })
    },

    fileModificationAction() {
      if (this.fileActionActive === -1) {
        this.$_console_log('fileModificationAction')
        return
      }

      switch (this.fileActionActive) {
        case this.operationType.move:
          this.$_console_log(
            `fileMoficiationAction: Move operation doesn't exist yet`
          )
          break
        case this.operationType.rename:
          this.renameFile()
          break
        case this.operationType.delete:
          this.deleteFile()
          break
        case this.operationType.properties:
          this.$_console_log(
            `fileMoficiationAction: Properties operation doesn't exist yet`
          )
          break
        case this.operationType.createFolder:
          this.createFolder()
          break
      }
    },

    renameFile() {
      this.$store
        .dispatch('renameFile', {
          oldName: this.updatedContents[this.fileActionIndex].title,
          newName: this.fileActionFieldValue,
          dir: this.directory,
          subDir: getSubdirectoryString(this.subDirectories),
          isFolder: this.updatedContents[this.fileActionIndex].isFolder,
        })
        .then(resp => {
          this.fileActionDialog = false
        })
        .catch(() => {
          this.fileActionError = 'Failed to rename file'
        })
    },
    canDelete(item) {
      if (!this.features.delete || item == null || item.isFolder == null) {
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
      if (!this.features.move || item == null || item.isFolder == null) {
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
    canCopy(item) {
      if (!this.features.move || item == null || item.isFolder == null) {
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
    canPaste(item) {
      if (!this.features.move || item == null || item.isFolder == null) {
        return false
      }

      let dir = this.folders.find(x => x.name === this.selectedDirectory)
      if (dir === undefined) {
        return false
      }

      if (this.copied == null) {
        this.$_console_log(`Can't paste if you haven't copied anything.`)
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
    canMove(item) {
      if (!this.features.move || item == null || item.isFolder == null) {
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
    canMakeFolder() {
      return this.features.create
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
      if (item.title === null) {
        return
      }

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
      if (
        this.updatedContents.findIndex(x => x.title === this.newFolderName) > -1
      ) {
        this.$_console_log(`Can't create a folder that already exists`)
        this.createFolderError = 'Folder already exists'
        return
      }

      service
        .createFolder(
          this.fileActionFieldValue,
          this.directory,
          getSubdirectoryString(this.subDirectories)
        )
        .then(resp => {
          this.$store.dispatch('addFile', resp.data)
          this.fileActionDialog = false
        })
        .catch(() => {
          this.$_console_log('Failed to create folder')
          this.fileActionError = 'Failed to create folder'
        })
    },

    async deleteFile() {
      const file = this.updatedContents[this.fileActionIndex]
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
          this.fileActionDialog = false
        })
        .catch(() => {
          this.$_console_log('Error deleting the item')
          this.fileActionError = 'Failed to delete file'
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

.clickable-context-menu-item {
  cursor: pointer;
}

.clickable-context-menu-item:hover {
  background: #444;
}

.v-list-item {
  min-height: 36px;
}
</style>

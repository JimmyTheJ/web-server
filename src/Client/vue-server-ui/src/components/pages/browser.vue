<template>
  <div>
    <file-explorer
      parentView="browser"
      :goFile="goFile"
      @loadFile="loadFile"
    ></file-explorer>

    <generic-dialog
      :title="dialogTitle"
      :open="dialogOpen"
      @dialog-close="dialogOpen = false"
    >
      <file-viewer @file-back="browserGo(-1)" @file-forward="browserGo(1)">
        <template v-if="type === mediaTypes.video">
          <video-player :url="path" :on="on" @player-off="on = false" />
        </template>
        <template v-else-if="type === mediaTypes.image">
          <image-viewer :url="path" />
        </template>
        <template v-else-if="type === mediaTypes.text">
          <text-viewer :url="path" :on="on" @text-off="on = false" />
        </template>
      </file-viewer>
    </generic-dialog>
  </div>
</template>

<script>
import Service from '../../services/file-explorer'

import Explorer from '../modules/file-explorer'
import FileViewer from '../modules/viewers/file-viewer'
import VideoPlayer from '../modules/viewers/video-player'
import TextViewer from '../modules/viewers/text-viewer'
import ImageViewer from '../modules/viewers/image-viewer'
import GenericDialog from '../modules/generic-dialog.vue'

import { Roles } from '../../constants'

import store from '../../store/index'
import router from '../../router'
import ConMsgs from '../../mixins/console'

export default {
  data() {
    return {
      level: 0,
      folders: [],

      type: null,
      path: null,
      on: false,

      dialogOpen: false,

      mediaTypes: {
        video: 'video',
        image: 'image',
        text: 'text',
      },

      goFile: {
        file: null,
        num: 0,
      },
    }
  },
  components: {
    'generic-dialog': GenericDialog,
    'file-explorer': Explorer,
    'file-viewer': FileViewer,
    'video-player': VideoPlayer,
    'text-viewer': TextViewer,
    'image-viewer': ImageViewer,
  },
  async beforeRouteEnter(to, from, next) {
    let success = await handleRouteChange(to, from, next)
    if (!success) {
      store.dispatch('pushNotification', {
        text: `Failed to change route to this location (${to.fullPath}). It must not exist.`,
        type: 2,
        group: { type: 'browser', value: 'pathFail' },
      })
      router.push({ name: 'browser' })
    } else {
      next()
    }
  },
  async beforeRouteUpdate(to, from, next) {
    const success = await handleRouteChange(to, from, next)
    if (!success) {
      store.dispatch('pushNotification', {
        text: `Failed to change route to this location (${to.fullPath}). It must not exist.`,
        type: 2,
        group: { type: 'browser', value: 'pathFail' },
      })
      router.push({ name: 'browser' })
    } else {
      next()
    }
  },
  beforeRouteLeave(to, from, next) {
    // When leaving the browser page we should clear out the data stored in the vuex store
    // to create a better experience when /if we come back to the browser screen
    if (to.name !== 'browser') store.dispatch('clearFileExplorer')

    next()
  },
  created() {
    let role = this.$store.state.auth.role

    // TODO: Get the list from the server
    if (role === Roles.Name.Admin) {
      this.level = Roles.Level.Admin
    } else if (role === Roles.Name.Elevated) {
      this.level = Roles.Level.Elevated
    } else if (role === Roles.Name.General) {
      this.level = Roles.Level.General
    } else this.$router.push({ name: 'start' })
  },
  computed: {
    dialogTitle() {
      switch (this.type) {
        case this.mediaTypes.video:
          return 'Video Player'
        case this.mediaTypes.image:
          return 'Image Viewer'
        case this.mediaTypes.text:
          return 'Text Viewer'
      }
    },
  },
  watch: {
    dialogOpen(newValue) {
      // Close dialog
      if (newValue === false) {
        // If our player isn't on
        if (!this.on) {
          const self = this
          // Turn it on so when we turn it off it'll flip the watcher
          this.on = true
          setTimeout(() => {
            self.on = false
          }, 50)
        }
        // Otherwise just turn it off
        else {
          this.on = false
        }
      }
    },
  },
  methods: {
    getType(file) {
      if (typeof file === 'undefined' || file === '') {
        this.$_console_log('[File Viewer] getType: Undefined or empty')
        return this.mediaTypes.text
      }

      if (!file.includes('.')) {
        this.$_console_log(
          "[File Viewer] getType: Doesn't include a period (.) "
        )
        return this.mediaTypes.text
      }

      const index = file.lastIndexOf('.')
      const extension = file.slice(index).toLowerCase()

      // TODO: Create exhaustive list of extensions
      switch (extension) {
        case '.mkv':
        case '.avi':
        case '.mpeg':
        case '.mpg':
        case '.mp4':
        case '.wmv':
        case '.webm':
        case '.mp3':
          return this.mediaTypes.video
        case '.jpg':
        case '.jpeg':
        case '.gif':
        case '.tiff':
        case '.bmp':
        case '.png':
        case '.img':
          return this.mediaTypes.image
        default:
          return this.mediaTypes.text
      }
    },
    loadFile(file) {
      this.$_console_log('[BROWSER] LoadFile: ', file)

      this.type = this.getType(file)

      this.path = file
      this.dialogOpen = true
      setTimeout(() => {
        this.on = true
      }, 15)
    },
    browserGo(num) {
      if (typeof num !== 'number' || num === 0) {
        this.$_console_log('[Browser] Browser Go: Number is not a number or 0')
        return
      }
      if (typeof this.path !== 'string') {
        this.$_console_log('[Browser] Browser Go: Path is not a string')
        return
      }

      let filename
      if (this.path.includes('/')) {
        if (this.path.lastIndexOf('/') === this.path.length - 1) {
          filename = this.path.substring(0, this.path.lastIndexOf('/'))
          if (filename.includes('/')) {
            if (filename.lastIndexOf('/') === filename.length - 1) {
              filename = filename.substring(0, filename.lastIndexOf('/'))
            } else {
              filename = filename.substring(filename.lastIndexOf('/') + 1)
            }
          }
        } else {
          filename = this.path.substring(this.path.lastIndexOf('/') + 1)
        }
      } else {
        filename = this.path
      }

      this.goFile = {
        file: filename,
        num: num,
      }
    },
  },
}

async function handleRouteChange(to, from, next) {
  ConMsgs.methods.$_console_log('Route changed. To, from:', to, from)

  let toHasSubDir = false
  let fromHasSubDir = false
  let toBasePath = null
  let fromBasePath = null
  let toSubDirs = null
  let fromSubDirs = null

  if (typeof to.params.folder !== 'undefined') {
    if (to.params.folder.includes('/')) {
      toHasSubDir = true
      const firstIndex = to.params.folder.indexOf('/')
      toBasePath = to.params.folder.substring(0, firstIndex)
      toSubDirs = to.params.folder.substring(firstIndex + 1)
    } else {
      toBasePath = to.params.folder
    }
  }

  if (typeof from.params.folder !== 'undefined') {
    if (from.params.folder.includes('/')) {
      fromHasSubDir = true
      const firstIndex = to.params.folder.indexOf('/')
      fromBasePath = from.params.folder.substring(0, firstIndex)
      fromSubDirs = from.params.folder.substring(firstIndex + 1)
    } else {
      fromBasePath = from.params.folder
    }
  }

  ConMsgs.methods.$_console_log(
    'Route change info: ',
    toHasSubDir,
    toBasePath,
    toSubDirs,
    fromHasSubDir,
    fromBasePath,
    fromSubDirs
  )

  if (to.params.folder === from.params.folder) {
    // noop
    ConMsgs.methods.$_console_log('Staying in the same folder')
  } else if (fromBasePath !== toBasePath && toHasSubDir === false) {
    // Going from no selected folder to some selected folder
    store.dispatch('changeDirectory', toBasePath)
  } else if (toBasePath !== null && toHasSubDir) {
    store.dispatch('goDirectory', { directory: toBasePath, subDirs: toSubDirs })
  }

  try {
    await store.dispatch('loadDirectory')
  } catch (ex) {
    // Failed request. Go back to where we came from
    if (ex.status >= 400) {
      if (fromSubDirs !== null) {
        await store.dispatch('goDirectory', {
          directory: fromBasePath,
          subDirs: fromSubDirs,
        })
        await store.dispatch('loadDirectory')
      } else if (fromBasePath !== null) {
        await store.dispatch('goDirectory', {
          directory: fromBasePath,
          subDirs: null,
        })
        await store.dispatch('loadDirectory')
      } else {
        await store.dispatch('clearFileExplorer')
        await store.dispatch('loadDirectory')
        return false
      }
    }
  }

  return true
}
</script>

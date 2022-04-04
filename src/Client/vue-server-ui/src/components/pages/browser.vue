<template>
  <div>
    <file-explorer parentView="browser"></file-explorer>

    <generic-dialog
      :title="dialogTitle"
      :open="dialogOpen"
      @dialog-close="dialogOpen = false"
    >
      <file-viewer :dialog="dialogOpen" :activeIndex="activeIndex" />
    </generic-dialog>
  </div>
</template>

<script>
const FN = 'Browser'

import { mapState } from 'vuex'

import Explorer from '@/components/modules/file-explorer'
import FileViewer from '@/components/modules/viewers/file-viewer'
import GenericDialog from '@/components/modules/generic-dialog.vue'

import { getFileType } from '@/helpers/browser'
import {
  MediaTypes,
  Delay,
  NotificationActions,
  NotificationTypes,
} from '@/constants'
import store from '@/store/index'
import router from '@/router'
import ConMsgs from '@/mixins/console'

export default {
  data() {
    return {
      dialogOpen: false,
      dialogTitle: null,
      activeIndex: -1,
    }
  },
  components: {
    'generic-dialog': GenericDialog,
    'file-explorer': Explorer,
    'file-viewer': FileViewer,
  },
  async beforeRouteEnter(to, from, next) {
    let success = await handleRouteChange(to, from, next)
    if (!success) {
      store.dispatch('pushNotification', {
        text: `Failed to change route to this location (${to.fullPath}). It must not exist.`,
        action: NotificationActions.Failed,
        group: { type: NotificationTypes.Browser, value: 'pathFail' },
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
        action: NotificationActions.Failed,
        group: { type: NotificationTypes.Browser, value: 'pathFail' },
      })
      router.push({ name: 'browser' })
    } else {
      next()
    }
  },
  beforeRouteLeave(to, from, next) {
    // When leaving the browser page we should clear out the data stored in the vuex store
    // to create a better experience when /if we come back to the browser screen
    if (to.name !== 'browser') store.dispatch('clearDirectoryModule')

    next()
  },
  computed: {
    ...mapState({
      directory: state => state.directory,
    }),
  },
  watch: {
    'directory.file': {
      handler(newValue, oldValue) {
        this.$_console_log(
          `[${FN}] file watcher: old value / new value`,
          oldValue,
          newValue
        )

        // If the dialog isn't open, and we aren't in the process
        // of closing it or an active file then open the dialog
        if (
          !this.dialogOpen &&
          typeof newValue !== 'undefined' &&
          newValue != null
        ) {
          this.dialogOpen = true

          this.$nextTick(() => {
            let index = this.directory.filteredFiles.findIndex(
              x => x.title === newValue.title
            )

            this.$_console_log(
              `[${FN}] directory.file watcher: replace old with new`,
              index
            )

            if (index >= 0) this.activeIndex = index
          })
        }
        // Closing the dialog. Cleanup.
        else if (typeof newValue === 'undefined' || newValue === null) {
          this.dialogOpen = false

          // When closing the dialog we also need to make sure the video players stop.
          // The code in the activeItem watcher in file-viewer isn't going to trigger
          let videoElements = document.getElementsByClassName('video-player')
          videoElements.forEach(item => {
            item.pause()
          })
        } else if (oldValue == newValue);
        else {
          // Situation like where we already have the file viewer open but we are switching
          // between files in the carousel. In this case we need to update the dialog title
          // It's a bit annoying having to do this here. Considering putting something in
          // the vuex store to handle it so we don't need to
          this.updateDialogTitle()
        }
      },
      deep: true,
    },
    dialogOpen: function(newValue) {
      if (!newValue) {
        this.$store.dispatch('loadFile', null)
        this.activeIndex = -1
      } else {
        this.updateDialogTitle()
      }
    },
  },
  methods: {
    updateDialogTitle() {
      let fileType = getFileType(this.directory.file)
      this.$_console_log(
        `[${FN}] updateDialogTitle: ${fileType} - ${typeof fileType}`
      )

      switch (fileType) {
        case MediaTypes.Video:
          this.dialogTitle = 'Video Player'
          break
        case MediaTypes.Image:
          this.dialogTitle = 'Image Viewer'
          break
        case MediaTypes.Text:
          this.dialogTitle = 'Text Viewer'
          break
        default:
          this.dialogTitle = 'Folder'
          break
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
        await store.dispatch('clearDirectoryModule')
        await store.dispatch('loadDirectory')
        return false
      }
    }
  }

  return true
}
</script>

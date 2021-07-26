<template>
  <v-carousel v-model="activeItem">
    <v-carousel-item v-for="(item, i) in fileExplorer.filteredFiles" :key="i">
      <v-card height="100%" class="text-center">
        <template v-if="getMediaType(item) === mediaTypes.Video">
          <video-player :file="item" :url="buildPath(item)" :dialog="dialog" />
        </template>
        <template v-else-if="getMediaType(item) === mediaTypes.Image">
          <image-viewer :file="item" :url="buildPath(item)" :dialog="dialog" />
        </template>
        <template v-else-if="getMediaType(item) === mediaTypes.Text">
          <text-viewer :file="item" :url="buildPath(item)" :dialog="dialog" />
        </template>
      </v-card>
    </v-carousel-item>
  </v-carousel>
</template>

<script>
const FN = 'File Viewer'

import { mapState } from 'vuex'
import { MediaTypes } from '@/constants'
import { getSubdirectoryString, getFileType } from '@/helpers/browser'

import VideoPlayer from './video-player.vue'
import TextViewer from './text-viewer.vue'
import ImageViewer from './image-viewer.vue'

export default {
  name: 'file-viewer',
  data() {
    return {
      activeItem: -1,
      mediaTypes: MediaTypes,
    }
  },
  components: {
    'video-player': VideoPlayer,
    'text-viewer': TextViewer,
    'image-viewer': ImageViewer,
  },
  props: {
    dialog: {
      type: Boolean,
    },
    activeIndex: {
      type: Number,
      default: 0,
      required: false,
    },
  },
  computed: {
    ...mapState({
      fileExplorer: state => state.fileExplorer,
    }),
  },
  watch: {
    activeIndex: function(newValue) {
      if (newValue === -1) return
      this.setActiveIndex()
    },
    activeItem: function(newValue, oldValue) {
      this.$_console_log(
        `[${FN} active item watcher]: old(${oldValue}) isInteger (${Number.isInteger(
          oldValue
        )}) 
        / new(${newValue}) isInteger (${Number.isInteger(newValue)})`
      )

      let videoElements = document.getElementsByClassName('video-player')
      videoElements.forEach(item => {
        item.pause()
      })

      if (Number.isInteger(newValue) && newValue >= 0) {
        this.$store.dispatch(
          'loadFile',
          this.fileExplorer.filteredFiles[newValue]
        )
        this.$store.dispatch('setActiveFile', { index: newValue, value: true })
      }
      if (Number.isInteger(oldValue) && oldValue >= 0) {
        this.$store.dispatch('setActiveFile', { index: oldValue, value: false })
      }
    },
  },
  created() {
    this.setActiveIndex()
  },
  methods: {
    getMediaType(file) {
      return getFileType(file)
    },
    buildPath(file) {
      if (typeof file === 'undefined' || file === null || file.isFolder)
        return null

      let subDirStr = getSubdirectoryString(this.fileExplorer.subDirectories)
      let fullStr = `${this.fileExplorer.directory}/${subDirStr}/${file.title}`

      //this.$_console_log(`[${FN}] subDir / fullPath: `, subDirStr, fullStr)

      return fullStr
    },
    setActiveIndex() {
      this.activeItem = this.activeIndex
    },
  },
}
</script>

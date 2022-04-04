<template>
  <v-img
    :src="getPath"
    :max-width="getMaxWidth"
    :max-height="getMaxHeight"
    contain
  />
</template>

<script>
const FN = 'Image Viewer'
import { mapState } from 'vuex'

export default {
  name: 'text-viewer',
  data() {
    return {
      windowHeight: window.innerHeight,
      windowWidth: window.innerWidth,

      basepath: process.env.VUE_APP_API_URL,
    }
  },
  props: {
    dialog: {
      type: Boolean,
    },
    url: {
      type: String,
      required: true,
    },
    file: {
      type: Object,
      required: true,
    },
  },
  computed: {
    ...mapState({
      accessToken: state => state.auth.accessToken,
      directory: state => state.directory,
    }),
    getMaxWidth() {
      return this.windowWidth * 0.8
    },
    getMaxHeight() {
      return this.windowHeight * 0.66
    },
    getPath() {
      return `${this.basepath}/api/directory/download/file/${this.url}?token=${this.accessToken}`
    },
    isActive: function() {
      return this.file.active
    },
  },
  watch: {
    url(newValue) {
      this.$_console_log(`[${FN}] Url watcher: url value`, newValue)
    },
    isActive(newValue) {
      this.$_console_log(`[${FN}] isActive watcher: new value -> ${newValue}`)
      if (newValue === true); // noop
    },
  },
  mounted() {
    window.addEventListener('resize', this.getWindowSize)
  },
  beforeDestroy() {
    window.removeEventListener('resize', this.getWindowSize)
  },
  methods: {
    getWindowSize() {
      this.windowHeight = window.innerHeight
      this.windowWidth = window.innerWidth
    },
  },
}
</script>

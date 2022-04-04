<template>
  <div class="video-container center">
    <video
      id="video-player"
      class="video-player"
      ref="player"
      preload="none"
      width="420"
      controls
    >
      <source :src="path" />
    </video>
  </div>
</template>

<script>
const FN = 'Video Player'
import { mapState } from 'vuex'

export default {
  name: 'video-player',
  data() {
    return {
      player: null,
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
  mounted() {
    this.player = document.getElementById('video-player')
  },
  computed: {
    ...mapState({
      accessToken: state => state.auth.accessToken,
      directory: state => state.directory,
    }),
    type: function() {
      if (
        typeof this.directory.file === 'undefined' ||
        this.directory.file === null
      ) {
        this.$_console_log(`[${FN}] type: File is undefined or null`)
        return null
      } else if (this.directory.file.isFolder) {
        this.$_console_log(`[${FN}] type: File is a folder, it has no type`)
        return null
      }

      // TODO: Create exhaustive list of extensions
      switch (this.directory.file.extension) {
        case '.mkv':
        case '.avi':
        case '.mpeg':
        case '.mpg':
        case '.mp4':
        case '.wmv':
          return 'video/mp4'
        case '.webm':
          return 'video/webm'
        case '.mp3':
          return 'audio/mpeg'
        default:
          return null
      }
    },
    path: function() {
      return `${process.env.VUE_APP_API_URL}/api/serve-file/${this.url}?token=${this.accessToken}`
    },
  },
}
</script>

<style scoped>
.center {
  margin: 0 auto;
}
</style>

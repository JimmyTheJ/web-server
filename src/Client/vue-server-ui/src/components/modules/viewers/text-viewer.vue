<template>
  <v-container class="vertical-scroll">
    <v-row v-for="(line, i) in lines" :key="i" class="vs-line">
      <span v-for="(letter, j) in line" :key="j" :class="getClasses(letter)">
        {{ letter }}
      </span>
    </v-row>
    <!-- Still working on getting this working... -->
    <!-- <v-virtual-scroll :items="lines" item-height="20" height="360">
      <template v-slot:default="{ line }">
        <v-list-item>
          <v-list-item-content>
            <v-list-item-title>
              {{ line }}

            </v-list-item-title>
          </v-list-item-content>
        </v-list-item>
      </template>
    </v-virtual-scroll> -->
  </v-container>
</template>

<script>
const FN = 'Text Viewer'
import { mapState } from 'vuex'

import service from '../../../services/file-explorer'
import DispatchFactory from '../../../factories/dispatchFactory'

export default {
  name: 'text-viewer',
  data() {
    return {
      text: null,
      lines: [],
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
    this.getData()
  },
  methods: {
    getClasses(letter) {
      if (letter === ' ') return 'vs-space'
      else if (letter === '\t') return 'vs-tab'
      else return
    },
    async getData() {
      if (
        typeof this.url === 'undefined' ||
        this.url === null ||
        this.url === ''
      ) {
        this.$_console_log(`[${FN}] Get Data: url value is null or empty`)
        return
      }

      const tmpUrl = this.url
      await DispatchFactory.request(() => {
        return service
          .getFile(tmpUrl)
          .then(resp => {
            this.$_console_log(`[${FN}] File data:`, resp.data)
            let text = resp.data
            this.lines = []

            let line = ''
            for (let i = 0; i < text.length; i++) {
              // Legacy macOS new lines
              if (
                text[i] === '\r' &&
                i != text.length - 1 &&
                text[i + 1] !== '\n'
              ) {
                this.lines.push(line)
                line = ''
              }
              // Windows new lines
              else if (
                text[i] === '\r' &&
                i != text.length - 1 &&
                text[i + 1] === '\n'
              ) {
                this.lines.push(line)
                line = ''
                i++ // skip the \n character after the carridge return
              }
              // Linux new lines
              else if (text[i] === '\n') {
                this.lines.push(line)
                line = ''
              }
              // Not a newline, just writting out text
              else {
                line += text[i]
              }
            }
            // Push what's left in the buffer to the list
            this.lines.push(line)
          })
          .catch(() => {
            this.$_console_log(
              `[${FN}] Get Data: Failed to get data from ${tmpUrl}`
            )
          })
      })
    },
  },
}
</script>

<style scoped>
* {
  --spacing: 4px;
  --line-height: 20px;
}

.vertical-scroll {
  overflow-y: auto;
  height: 100%;
}

.vs-line {
  min-height: var(--line-height);
}

.vs-space {
  min-width: var(--spacing);
}

.vs-tab {
  min-width: calc(4 * var(--spacing));
}
</style>

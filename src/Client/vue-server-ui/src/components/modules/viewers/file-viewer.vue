<template>
  <v-card>
    <v-card-text>
      <v-layout row>
        <v-flex xs1>
          <v-btn @click="goBack()" class="chevron-center" icon>
            <fa-icon icon="chevron-left" size="3x"></fa-icon>
          </v-btn>
        </v-flex>

        <v-flex xs10>
          <slot> File Viewer contents </slot>
        </v-flex>

        <v-flex xs1>
          <v-btn @click="goForward()" class="chevron-center" icon>
            <fa-icon icon="chevron-right" size="3x"></fa-icon>
          </v-btn>
        </v-flex>
      </v-layout>
    </v-card-text>
  </v-card>
</template>

<script>
export default {
  name: 'file-viewer',
  data() {
    return {
      dialog: false,
    }
  },
  watch: {
    dialog: function(newValue) {
      if (newValue === false) this.$emit('viewer-off', true)
    },
    open: function(newValue) {
      this.dialog = newValue
    },
  },
  mounted() {
    window.addEventListener('keyup', this.keypressNavigation)
  },
  beforeDestroy() {
    window.removeEventListener('keyup', this.keypressNavigation)
  },
  methods: {
    keypressNavigation(evt) {
      //this.$_console_log('Event', evt);

      // Left arrow
      if (evt.keyCode === 37) {
        this.goBack()
      }
      // Right arrow
      else if (evt.keyCode === 39) {
        this.goForward()
      }
    },
    goBack() {
      this.$_console_log('[File Viewer] Go Back')
      this.$emit('file-back')
    },
    goForward() {
      this.$_console_log('[File Viewer] Go Forward')
      this.$emit('file-forward')
    },
  },
}
</script>

<style>
.center {
  margin: 0 auto;
}

.chevron-center {
  position: relative;
  top: 45%;
  width: 55px !important;
  height: 55px !important;
}
</style>

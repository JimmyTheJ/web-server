<template>
  <v-dialog
    v-model="dialog"
    :max-width="maxWidth"
    :fullscreen="$_window_isMobile"
  >
    <v-toolbar>
      <v-btn v-if="!hideClose" icon @click="dialog = false">
        <fa-icon icon="window-close"></fa-icon>
      </v-btn>
      <v-toolbar-title class="headline pl-2">
        <slot name="header"> {{ title }} </slot>
      </v-toolbar-title>
    </v-toolbar>
    <slot>
      <v-card><v-card-text>Your content here</v-card-text></v-card>
    </slot>
  </v-dialog>
</template>

<script>
import WindowMixin from '../../mixins/window'

export default {
  name: 'generic-dialog',
  mixins: [WindowMixin],
  data() {
    return {
      dialog: false,
    }
  },
  props: {
    title: {
      type: String,
      default: 'Action required',
    },
    open: {
      type: Boolean,
      required: true,
    },
    maxWidth: {
      type: Number,
      required: false,
    },
    hideClose: {
      type: Boolean,
      default: false,
    },
  },
  watch: {
    dialog: function (newValue) {
      if (newValue === false) this.$emit('dialog-close', true)
    },
    open: function (newValue) {
      this.dialog = newValue
    },
  },
  computed: {
    getMaxWidth() {
      let computedWidth = this.$_window_getMaxWidth
      if (
        typeof this.maxWidth === 'undefined' ||
        this.maxWidth === null ||
        computedWidth < this.maxWidth
      )
        return computedWidth
      else return this.maxWidth
    },
  },
}
</script>
<template>
  <div>
    <file-explorer
      :folders="folders"
      :level="LEVEL.General"
      :selectable="true"
      :parentView="`file-server`"
      ref="fileExplorer"
    ></file-explorer>
  </div>
</template>

<script>
import FileExplorer from '../modules/file-explorer'

import Service from '../../services/file-explorer'

import { Roles } from '../../constants'

export default {
  data() {
    return {
      folders: [],
      LEVEL: Roles.Level,
    }
  },
  components: {
    'file-explorer': FileExplorer,
  },
  created() {
    this.getFolders()
  },
  methods: {
    async getFolders() {
      await Service.getFolderList(Roles.Level.General)
        .then(resp => {
          this.folders = resp.data
          if (this.folders.length > 0) {
            this.$refs.fileExplorer.setSelectedFolder(this.folders[0])
            this.$refs.fileExplorer.loadDirectory()
          }
        })
        .catch(() => {
          this.$_console_log('Failed to upload file')
        })
    },
  },
}
</script>

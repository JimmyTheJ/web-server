<template>
  <div>
    <v-container grid-list-xl v-show="uploadFiles.length > 0">
      <div class="text-xs-center">Files being uploaded...</div>
      <v-list>
        <template v-for="(file, i) in uploadFiles">
          <v-list-item>
            <v-list-item-content>
              {{ file.name }}
            </v-list-item-content>
          </v-list-item>
        </template>
      </v-list>
    </v-container>

    <v-container mt-1 class="upload-area">
      <v-form enctype="multipart/form-data">
        <v-layout row wrap>
          <v-flex xs12>
            <label id="upload-button" for="upload-files" class="upload-button"
              >UPLOAD FILES</label
            >
          </v-flex>
          <input
            ref="fUpload"
            type="file"
            name="upload-files"
            id="upload-files"
            class="file-upload"
            multiple
            @change="setFiles"
            hidden
          />
        </v-layout>
      </v-form>
    </v-container>
  </div>
</template>

<script>
import * as CONST from '../../constants'
import { mapState } from 'vuex'
import { getSubdirectoryString } from '../../helpers/browser'

import service from '../../services/file-explorer'

export default {
  name: 'file-upload',
  data() {
    return {
      // Upload
      uploadFiles: [],
    }
  },
  computed: {
    ...mapState({
      directory: state => state.fileExplorer.directory,
      subDirectories: state => state.fileExplorer.subDirectories,
    }),
  },
  methods: {
    // Upload
    setFiles(e) {
      this.$_console_log(e.target.files)
      for (let i = 0; i < e.target.files.length; i++) {
        this.uploadFiles.push(e.target.files[i])
      }
      this.uploadMultipleFiles()
    },
    async uploadMultipleFiles() {
      for (let i = 0; i < this.uploadFiles.length; i++) {
        this.$_console_log(this.uploadFiles[i].name)

        await this.sendFile(this.uploadFiles[i])
          .then(resp => {
            this.$_console_log('File sent!')
          })
          .catch(() => {
            this.$_console_log('Failed to upload file')
          })
      }

      // Clean the list
      this.uploadFiles = []
      this.$refs.fUpload.value = ''
      this.$_console_log('Finished sending all files')
    },
    async sendFile(file) {
      let formData = new FormData()
      formData.append('File', file)
      formData.append('Name', file.name)
      formData.append('Directory', this.directory)

      let subDirs = getSubdirectoryString(this.subDirectories)
      this.$_console_log('SubDirs: ', subDirs)
      if (subDirs !== '') formData.append('SubDirectory', subDirs)

      await service
        .uploadFile(formData)
        .then(resp => {
          this.$_console_log('Successfully uploaded file')

          this.$store.dispatch('addFile', resp.data)
          this.$store.dispatch('pushNotification', {
            text: `Successfully uploaded file ${file.name}`,
            type: 0,
          })
        })
        .catch(() => {
          this.$_console_log('Error uploading files')

          this.$store.dispatch('pushNotification', {
            text: `Failed uploading file ${file.name}`,
            type: 2,
          })
        })
    },
  },
}
</script>

<style scoped>
.upload-area:hover {
  border: 1px dashed #d8d8d8;
  background-color: #646464;
  color: #323232;
  font-weight: bold;
}

.upload-area {
  border-radius: 5px;
  background-color: #424242;
}

.upload-button {
  display: block;
  text-align: center;
  line-height: 150%;
  font-size: 0.85em;
}

.small {
  height: 80px;
}
</style>

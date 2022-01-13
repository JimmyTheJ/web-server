<template>
  <div>
    <!-- <v-container grid-list-xl v-show="uploadingFiles.length > 0">
      <div class="text-xs-center">Files being uploaded...</div>
      <v-list>
        <template v-for="(file, i) in uploadingFiles">
          <v-list-item :key="i">
            <v-list-item-content>
              {{ file.name }}
            </v-list-item-content>
          </v-list-item>
        </template>
      </v-list>
    </v-container> -->

    <v-container mt-1>
      <v-file-input
        v-model="files"
        counter
        multiple
        show-size
        truncate-length="15"
        ref="fUpload"
        :disabled="copying"
        @change="uploadMultipleFiles"
      ></v-file-input>
    </v-container>
  </div>
</template>

<script>
import * as CONST from '../../constants'
import { mapState } from 'vuex'
import { getSubdirectoryString } from '../../helpers/browser'

import service from '../../services/file-explorer'

import { NotificationActions, NotificationTypes } from '@/constants'

export default {
  name: 'file-upload',
  data() {
    return {
      // Upload
      files: [],
      copying: false,
      numFilesCopying: 0,
    }
  },
  computed: {
    ...mapState({
      directory: state => state.fileExplorer.directory,
      subDirectories: state => state.fileExplorer.subDirectories,
    }),
  },
  watch: {
    numFilesCopying(newValue, oldValue) {
      if (newValue === 0 && oldValue > 0) {
        this.copying = false
        this.files = []
      }
    },
  },
  methods: {
    async uploadMultipleFiles(files) {
      if (
        typeof files === 'undefined' ||
        !Array.isArray(files) ||
        files === null
      ) {
        this.$_console_log('File list is null or empty')
        return
      }

      if (files.length === 0) {
        this.$_console_log('File list is empty')
        return
      }

      this.copying = true
      this.numFilesCopying = files.length
      let self = this
      for (let i = 0; i < files.length; i++) {
        let formData = new FormData()
        formData.append('file', files[i])
        formData.append('name', files[i].name)
        formData.append('directory', self.directory)

        let subDirs = getSubdirectoryString(self.subDirectories)
        self.$_console_log('SubDirs: ', subDirs)
        if (subDirs !== '') formData.append('subDirectory', subDirs)

        self.$_console_log(
          'Form Data:',
          formData.get('file'),
          formData.get('name'),
          formData.get('directory'),
          formData.get('subDirs')
        )

        await service
          .uploadFile(formData)
          .then(function(resp) {
            self.$_console_log('Successfully uploaded file')

            self.$store.dispatch('addFile', resp.data)
            self.$store.dispatch('pushNotification', {
              text: `Successfully uploaded file ${files[i].name}`,
              action: NotificationActions.Success,
              group: {
                type: NotificationTypes.Upload,
                value: NotificationActions.Success,
              },
            })
          })
          .catch(function(err) {
            self.$_console_log('Error uploading files', err)

            self.$store.dispatch('pushNotification', {
              text: `Failed uploading file ${files[i].name}`,
              action: NotificationActions.Failed,
              group: {
                type: NotificationTypes.Upload,
                value: NotificationActions.Failed,
              },
            })
          })
          .finally(function() {
            self.numFilesCopying--
          })
      }
    },
    async sendFile(file) {
      let formData = new FormData()
      formData.append('file', file)
      formData.append('name', file.name)
      formData.append('directory', this.directory)

      let subDirs = getSubdirectoryString(this.subDirectories)
      this.$_console_log('SubDirs: ', subDirs)
      if (subDirs !== '') formData.append('subDirectory', subDirs)

      this.$_console_log(
        'Form Data:',
        formData.get('file'),
        formData.get('name'),
        formData.get('directory'),
        formData.get('subDirs')
      )

      // try {
      //   let resp = await service.uploadFile(formData)
      //   this.$_console_log('Successfully uploaded file')

      //   this.$store.dispatch('addFile', resp.data)
      //   this.$store.dispatch('pushNotification', {
      //     text: `Successfully uploaded file ${file.name}`,
      //     action: NotificationActions.Success,
      //     group: {
      //       type: NotificationTypes.Upload,
      //       value: NotificationActions.Success,
      //     },
      //   })
      // } catch (e) {
      //   this.$_console_log('Error uploading files', e)
      // }

      return await service
        .uploadFile(formData)
        .then(function(resp) {
          this.$_console_log('Successfully uploaded file')

          this.$store.dispatch('addFile', resp.data)
          this.$store.dispatch('pushNotification', {
            text: `Successfully uploaded file ${file.name}`,
            action: NotificationActions.Success,
            group: {
              type: NotificationTypes.Upload,
              value: NotificationActions.Success,
            },
          })
        })
        .catch(function(err) {
          this.$_console_log('Error uploading files', err)

          this.$store.dispatch('pushNotification', {
            text: `Failed uploading file ${file.name}`,
            action: NotificationActions.Failed,
            group: {
              type: NotificationTypes.Upload,
              value: NotificationActions.Failed,
            },
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

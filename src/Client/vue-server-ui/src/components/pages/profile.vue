<template>
  <div>
    <v-dialog
      v-model="passwordChangeRequiredDialog"
      transition="dialog-bottom-transition"
      max-width="720"
    >
      <template v-slot:default="dialog">
        <v-card>
          <v-toolbar color="purple" dark>Notice</v-toolbar>
          <v-card-text class="text-center">
            <div class="headline pa-4">
              Please change your password now
            </div>
          </v-card-text>
          <v-card-actions class="justify-end">
            <v-btn
              text
              @click="
                dialog.value = false
                showPasswordChange = true
              "
              >Close</v-btn
            >
          </v-card-actions>
        </v-card>
      </template>
    </v-dialog>
    <v-container>
      <v-card>
        <v-card-title class="headline text-center">
          <div>User Profile</div>
          <v-spacer></v-spacer>
          <label id="upload-button" for="upload-files" class="upload-button">
            <v-avatar>
              <v-img v-if="hasAvatar" :src="avatarPath"></v-img>
              <fa-icon v-else icon="user" size="2x" />
            </v-avatar>
          </label>
          <input
            ref="fUpload"
            type="file"
            name="upload-files"
            id="upload-files"
            @change="uploadAvatarImage"
            hidden
          />
        </v-card-title>
        <v-card-text>
          <v-layout row wrap>
            <v-flex xs12 sm8>
              <v-text-field
                v-model="newDisplayName"
                label="Display name"
                :readonly="!editDisplayName"
                @keyup.enter="updateDisplayName()"
                @keyup.esc="editDisplayName = false"
              >
                <template v-slot:prepend>
                  <v-btn
                    v-if="editDisplayName === false"
                    icon
                    @click="editDisplayName = !editDisplayName"
                  >
                    <fa-icon icon="edit" />
                  </v-btn>
                  <v-btn
                    v-else
                    icon
                    @click="
                      ;(newDisplayName = user.displayName),
                        (editDisplayName = !editDisplayName)
                    "
                  >
                    <fa-icon icon="window-close" />
                  </v-btn>
                </template>
                <template v-if="editDisplayName === true" v-slot:append>
                  <v-btn @click="updateDisplayName()"> SUBMIT </v-btn>
                </template>
              </v-text-field>
            </v-flex>
          </v-layout>
        </v-card-text>
        <v-btn @click="showPasswordChange = !showPasswordChange"
          ><div class="headline ma-2">Change Password</div></v-btn
        >
        <change-password v-show="showPasswordChange" :hideUsername="true" />
      </v-card>
    </v-container>
  </div>
</template>

<script>
import { mapState } from 'vuex'
import ChangePassword from '../modules/change-password.vue'

export default {
  data() {
    return {
      passwordChangeRequiredDialog: false,
      uploadFile: null,
      editDisplayName: false,
      newDisplayName: '',
      showPasswordChange: false,
    }
  },
  components: {
    'change-password': ChangePassword,
  },
  created() {
    this.passwordChangeRequiredDialog = this.user.changePassword
  },
  mounted() {
    this.newDisplayName = this.user.displayName
  },
  computed: {
    ...mapState({
      user: state => state.auth.user,
    }),
    hasAvatar() {
      if (
        typeof this.user.avatar === 'undefined' ||
        this.user.avatar === null ||
        this.user.avatar === ''
      ) {
        return false
      }

      return true
    },
    avatarPath() {
      return `${process.env.VUE_APP_API_URL}/public/${this.user.avatar}`
    },
  },
  methods: {
    uploadAvatarImage(e) {
      const files = e.target.files
      this.$_console_log(files)
      if (files.length !== 1) {
        this.$_console_log(
          'SetFiles: File list contains more or less than 1 file'
        )
        return
      }

      let formData = new FormData()
      formData.append('File', files[0])

      this.$store
        .dispatch('updateAvatarImage', formData)
        .then(() => {})
        .catch(() => {})
        .then(() => {
          // Cleanup
          this.uploadFile = null
          this.$refs.fUpload.value = ''
        })
    },
    updateDisplayName() {
      this.$store
        .dispatch('updateDisplayName', this.newDisplayName)
        .then(() => {})
        .catch(() => {})
        .then(() => (this.editDisplayName = false))
    },
  },
}
</script>

<style scoped>
.upload-button:hover {
  cursor: pointer;
}
</style>

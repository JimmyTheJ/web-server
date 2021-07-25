<template>
  <v-container>
    <v-alert v-if="error" v-model="error" :value="true" type="error" dismissible
      >Invalid information sent. Please fix and try again</v-alert
    >
    <v-form ref="form" v-model="valid" lazy-validation>
      <v-text-field
        v-model="auth.user.displayName"
        v-show="!hideUsername"
        :label="`Username`"
        name="Username"
        readonly
      />
      <v-text-field
        :append-icon="oldPasswordOn ? 'visibility' : 'visibility_off'"
        @click:append="oldPasswordOn = !oldPasswordOn"
        :type="oldPasswordOn ? 'password' : 'text'"
        v-model="form.oldPassword"
        name="Old Password"
        :rules="rules.oldPassword"
        :label="`Old Password`"
        :disabled="auth.user.changePassword"
      />
      <v-text-field
        :append-icon="newPasswordOn ? 'visibility' : 'visibility_off'"
        @click:append="newPasswordOn = !newPasswordOn"
        :type="newPasswordOn ? 'password' : 'text'"
        v-model="form.newPassword"
        name="New Password"
        :rules="rules.newPassword"
        :label="`New Password`"
        required
      />
      <v-text-field
        :append-icon="confirmNewPasswordOn ? 'visibility' : 'visibility_off'"
        @click:append="confirmNewPasswordOn = !confirmNewPasswordOn"
        :type="confirmNewPasswordOn ? 'password' : 'text'"
        v-model="form.confirmNewPassword"
        name="Confirm New Password"
        :rules="rules.confirmNewPassword"
        :label="`Confirm New Password`"
        required
      />
      <v-btn :disabled="!valid || btnClicked" @click.prevent="changePassword()"
        >Change Password</v-btn
      >
    </v-form>
  </v-container>
</template>

<script>
import { Roles } from '../../constants.js'
import { mapState } from 'vuex'

function getNewForm() {
  return {
    oldPassword: '',
    newPassword: '',
    confirmNewPassord: '',
    checked: false,
  }
}

export default {
  data() {
    return {
      form: getNewForm(),
      oldPasswordOn: true,
      newPasswordOn: true,
      confirmNewPasswordOn: true,

      show: true,
      valid: true,
      btnClicked: false,
      rules: {
        newPassword: [
          v => !!v || 'Required',
          v => v.length >= 16 || 'Password must be at least 16 characters',
        ],
        confirmNewPassword: [
          v => !!v || 'Required',
          v => v === this.form.newPassword || 'Passwords must match',
        ],
      },
      error: false,
    }
  },
  props: {
    hideUsername: {
      type: Boolean,
    },
  },
  computed: {
    ...mapState({
      auth: state => state.auth,
    }),
  },
  methods: {
    changePassword() {
      this.btnClicked = true
      this.$refs.form.validate()

      if (this.valid) {
        this.$store
          .dispatch('passwordChanged', {
            data: this.form,
            isAdmin: this.auth.role === Roles.Name.Admin,
          })
          .then(resp => {
            this.error = false
            this.$router.push({ name: 'start' })
          })
          .catch(() => {
            this.error = true
          })
          .then(() => (this.btnClicked = false))
      } else {
        this.btnClicked = false
      }
    },
  },
}
</script>

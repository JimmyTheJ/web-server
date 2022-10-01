<template>
  <v-container>
    <v-alert v-if="error" v-model="error" :value="true" type="error" dismissible
      >Username or password incorrect</v-alert
    >
    <v-form ref="form" v-model="valid" lazy-validation>
      <v-text-field v-model="form.username" :rules="rules.username" :label="`Username`" name="Username" required />
      <v-text-field
        :append-icon="passwordOn ? 'visibility' : 'visibility_off'"
        @click:append="passwordOn = !passwordOn"
        :type="passwordOn ? 'password' : 'text'"
        v-model="form.password"
        name="Password"
        :rules="rules.password"
        :label="`Password`"
        required
      />
      <!--<v-checkbox v-model="form.checked" v-bind:label="`Keep me signed in`"></v-checkbox>-->

      <v-btn :disabled="!valid || btnClicked" @click.prevent="login()">Login</v-btn>
    </v-form>
  </v-container>
</template>

<script>
import { mapState } from 'vuex'

import { TokenValidation } from '@/constants.js'
import Auth from '@/mixins/authentication'
import Dispatcher from '@/services/ws-dispatcher.js'
import service from '@/services/auth.js'

export default {
  data() {
    return {
      form: {
        username: '',
        password: '',
        checked: false,
      },
      passwordOn: true,
      show: true,
      valid: true,
      btnClicked: false,
      rules: {
        username: [v => !!v || 'Required'],
        password: [v => !!v || 'Required'],
      },
      error: false,
    }
  },
  mixins: [Auth],
  computed: {
    ...mapState({
      auth: state => state.auth,
    }),
  },
  beforeDestroy() {
    window.removeEventListener('keyup', this.enterKeyListener)
  },
  async mounted() {
    window.addEventListener('keyup', this.enterKeyListener)

    let result = await this.$_auth_checkLogin()
    if (result) this.redirectToHome()
  },
  methods: {
    enterKeyListener(e) {
      if (e.keyCode === 13) {
        if (this.valid && !this.btnClicked) this.login()
      }
    },
    async login() {
      this.btnClicked = true
      await this.$_auth_login(this.form)
        .then(() => {
          this.error = false
          this.redirectToHome()
        })
        .catch(() => {
          this.error = true
        })
        .then(() => (this.btnClicked = false))
    },
    redirectToHome() {
      this.$router.replace(this.$route.query.redirect || '/home')
    },
  },
}
</script>

<template>
  <v-container>
    <v-alert v-if="error" v-model="error" :value="true" type="error" dismissible
      >There was an error registering you</v-alert
    >
    <v-alert
      v-if="success"
      v-model="success"
      :value="true"
      type="success"
      dismissible
      >User {{ form.username }} successfully created</v-alert
    >
    <v-form ref="form" v-model="valid" lazy-validation>
      <v-text-field
        v-model="form.username"
        :rules="rules.username"
        label="Username"
        name="Username"
        required
      ></v-text-field>
      <v-text-field
        name="Password"
        v-model="form.password"
        label="Password"
        :append-icon="passwordOn ? 'visibility' : 'visibility_off'"
        :type="passwordOn ? 'password' : 'text'"
        :rules="rules.password"
        required
        @click:append="passwordOn = !passwordOn"
      ></v-text-field>
      <v-text-field
        name="ConfirmPassword"
        v-model="form.confirmPassword"
        label="Confirm Password"
        :append-icon="passwordConfirmOn ? 'visibility' : 'visibility_off'"
        :type="passwordConfirmOn ? 'password' : 'text'"
        :rules="rules.confirmPassword"
        required
        @click:append="passwordConfirmOn = !passwordConfirmOn"
      ></v-text-field>
      <v-select
        :items="roles"
        v-model="form.role"
        label="Role"
        name="Role"
        single-line
      ></v-select>
      <v-btn :disabled="!valid || btnClicked" @click.prevent="register()"
        >Register</v-btn
      >
    </v-form>
  </v-container>
</template>

<script>
import { mapState } from 'vuex'
import Auth from '@/mixins/authentication'
import { Roles } from '@/constants'

function newForm() {
  return {
    username: '',
    password: '',
    confirmPassword: '',
    role: null,
  }
}

export default {
  name: 'register-user',
  mixins: [Auth],
  data() {
    return {
      passwordOn: true,
      passwordConfirmOn: true,
      form: newForm(),
      rules: {
        username: [],
        password: [
          v => !!v || 'Required',
          v => v.length >= 16 || 'Password must be at least 16 characters',
        ],
        newPassword: [
          v => !!v || 'Required',
          v => v === this.form.newPassword || 'Passwords must match',
        ],
      },
      show: true,
      valid: true,
      btnClicked: false,
      error: false,
      success: false,
    }
  },
  computed: {
    ...mapState({
      roles: state =>
        typeof state.auth.admin !== 'undefined' ? state.auth.admin.roles : [],
    }),
  },
  beforeDestroy() {
    //window.removeEventListener('keyup', this.enterKeyListener)
  },
  mounted() {
    //window.addEventListener('keyup', this.enterKeyListener)
    this.$nextTick(() => {
      if (Array.isArray(this.roles))
        this.form.role = this.roles.find(x => x === Roles.Name.General)
    })
  },
  methods: {
    enterKeyListener(e) {
      if (e.keyCode === 13) {
        if (this.valid && !this.btnClicked) this.register()
      }
    },
    register() {
      this.btnClicked = true
      this.$refs.form.validate()

      if (this.valid) {
        this.$_auth_register(this.form)
          .then(() => {
            this.form = newForm()
            this.error = false
            this.success = true
          })
          .catch(() => {
            this.error = true
            this.success = false
          })
          .then(() => (this.btnClicked = false))
      } else {
        this.btnClicked = false
      }
    },
  },
}
</script>

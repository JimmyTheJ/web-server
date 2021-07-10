<template>
  <div id="app">
    <v-app>
      <router-view></router-view>
    </v-app>
  </div>
</template>

<script>
import * as CONST from './constants'
import { setTimeout } from 'core-js'
import { parse as parseJwt } from './helpers/jwt'
import AuthenticationMixin from '@/mixins/authentication.js'

export default {
  data() {
    return {}
  },
  created() {
    this.refreshTokenJob()

    this.$_auth_addRoutes()
  },
  mixins: [AuthenticationMixin],
  methods: {
    refreshTokenJob() {
      setTimeout(() => {
        //getRefreshToken
        if (this.$store.state.auth.isAuthorize) {
          const token = parseJwt(this.$store.state.auth.accessToken)
          let time = new Date().getTime() / 1000
          // If the refresh token is going to expire before the next loop happens we should refresh it
          if (time + CONST.Admin.RefreshTokenTimer / 1000 >= token.exp) {
            this.$_console_log(
              'Token timeout is about to be reached. Getting new token'
            )
            this.$store
              .dispatch('refreshToken')
              .then(() => {})
              .catch(() =>
                this.$_console_log('Failed to get time released refresh token')
              )
          }
        }

        this.refreshTokenJob()
      }, CONST.Admin.RefreshTokenTimer)
    },
  },
}
</script>

<template>
  <div id="app">
    <v-app>
      <router-view></router-view>
    </v-app>
  </div>
</template>

<script>
const FN = 'App'
import * as CONST from '@/constants'
import { parse as parseJwt, requiresRefresh } from '@/helpers/jwt'
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
      setInterval(this.getRefreshToken, CONST.Admin.RefreshTokenTimer)
    },
    getRefreshToken() {
      this.$_console_log(
        `[${FN}]: Checking if we need to refresh our JWT token`
      )
      if (this.$store.state.auth.isAuthorize) {
        if (requiresRefresh(this.$store.state.auth.accessToken)) {
          this.$store
            .dispatch('refreshToken')
            .then(() => {})
            .catch(() =>
              this.$_console_log(`[${FN}]: Failed to get refreshed JWT token`)
            )
        }
      }
    },
  },
}
</script>

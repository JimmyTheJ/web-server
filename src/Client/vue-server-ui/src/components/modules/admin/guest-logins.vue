<template>
  <v-card>
    <v-card-title>Guest Logins</v-card-title>
    <v-card-text>
      <div v-for="(guest, i) in guestLoginList" :key="i">
        <v-layout row>
          <v-flex xs5 px-1>
            <v-text-field
              v-model="guest.iPAddress"
              label="IP Address:"
              readonly
            />
          </v-flex>
          <v-flex xs3 px-1>
            <v-text-field
              v-model="guest.failedLogins"
              label="Failed logins:"
              readonly
            />
          </v-flex>
          <v-flex xs2 px-1>
            <v-text-field v-model="guest.blocked" label="Blocked:" readonly />
          </v-flex>
          <v-flex xs2 px-1 mt-3>
            <v-btn icon @click="unblockGuest(guest.iPAddress)"
              ><fa-icon icon="window-close" size="md"></fa-icon
            ></v-btn>
          </v-flex>
        </v-layout>
      </div>
    </v-card-text>
  </v-card>
</template>

<script>
const FN = 'guest-login'

import authService from '@/services/auth'
import DispatchFactory from '@/factories/dispatchFactory'

export default {
  name: 'guest-logins',
  data() {
    return {
      guestLoginList: [],
    }
  },
  mounted() {
    this.getData()
  },
  methods: {
    getData() {
      DispatchFactory.request(() => {
        authService
          .getGuestLogins()
          .then(resp => {
            this.$_console_log(
              `[${FN}] getData: Successfully got guest login list`
            )
            this.guestLoginList = resp.data
          })
          .catch(() =>
            this.$_console_log(
              `[${FN}] getData: Failed to get guest login list`
            )
          )
      })
    },
    unblockGuest(ip) {
      DispatchFactory.request(() => {
        authService
          .unblockGuest(ip)
          .then(resp => {
            this.$_console_log(
              `[${FN}] unblockGuest: Successfully unblocked IP`
            )
            let login = this.guestLoginList.find(x => x.iPAddress == ip)
            if (login !== undefined) {
              login.blocked = false
              login.failedLogins = 0
            }
          })
          .catch(() =>
            this.$_console_log(`[${FN}] unblockGuest: Failed to unblock IP`)
          )
      })
    },
  },
}
</script>

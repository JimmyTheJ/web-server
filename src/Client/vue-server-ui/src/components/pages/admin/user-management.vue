<template>
  <v-container>
    <v-card>
      <v-toolbar color="gray" dark flat>
        <template>
          <v-tabs v-model="tab" align-with-title>
            <v-tabs-slider color="purple"></v-tabs-slider>

            <v-tab v-for="item in tabs" :key="item">
              {{ item }}
            </v-tab>
          </v-tabs>
        </template>
      </v-toolbar>

      <v-tabs-items v-model="tab">
        <v-tab-item key="0">
          <user-modules />
        </v-tab-item>
        <v-tab-item key="1">
          <register-user />
        </v-tab-item>
        <v-tab-item key="2">
          <guest-logins />
        </v-tab-item>
      </v-tabs-items>
    </v-card>
  </v-container>
</template>

<script>
import Auth from '@/mixins/authentication'
import UserModules from '@/components/modules/admin/user-modules.vue'
import GuestLogins from '@/components/modules/admin/guest-logins.vue'
import RegisterUser from '@/components/modules/admin/register-user.vue'

export default {
  mixins: [Auth],
  data() {
    return {
      tab: null,
      tabs: ['User Modules', 'Register Users', 'Guest Logins'],
    }
  },
  components: {
    'user-modules': UserModules,
    'guest-logins': GuestLogins,
    'register-user': RegisterUser,
  },
  computed: {},
  beforeDestroy() {},
  created() {
    if (typeof this.$store.state.auth.admin === 'undefined')
      this.$store.dispatch('getRoles')

    if (
      typeof this.$store.state.auth.userMap === 'undefined' ||
      Object.keys(this.$store.state.auth.userMap).length === 0
    )
      this.$store.dispatch('getAllOtherUsers')
  },
  methods: {},
}
</script>

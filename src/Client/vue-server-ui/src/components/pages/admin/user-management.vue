<template>
  <div>
    <template>
      <v-tabs
        v-model="tab"
        :align-with-title="!isMobile"
        :align-left="isMobile"
        :vertical="isMobile"
      >
        <v-tabs-slider color="purple"></v-tabs-slider>

        <v-tab v-for="item in tabs" :key="item.name">
          <fa-icon :icon="item.icon" class="mx-2" />
          {{ item.name }}
        </v-tab>
      </v-tabs>
    </template>

    <v-divider />

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
  </div>
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
      tabs: [
        { name: 'User Modules', icon: 'users' },
        { name: 'Register Users', icon: 'user-plus' },
        { name: 'Guest Logins', icon: 'door-open' },
      ],
    }
  },
  components: {
    'user-modules': UserModules,
    'guest-logins': GuestLogins,
    'register-user': RegisterUser,
  },
  computed: {
    isMobile() {
      let val = this.$vuetify.breakpoint.name
      return val === 'xs' || val === 'sm'
    },
  },
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

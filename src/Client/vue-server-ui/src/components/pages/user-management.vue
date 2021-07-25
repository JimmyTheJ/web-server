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
          <guest-logins />
        </v-tab-item>
      </v-tabs-items>
    </v-card>
  </v-container>
</template>

<script>
import Auth from '../../mixins/authentication'
import UserModules from '../modules/admin/user-modules.vue'
import GuestLogins from '../modules/admin/guest-logins.vue'

export default {
  mixins: [Auth],
  data() {
    return {
      tab: null,
      tabs: ['User Modules', 'Guest Logins'],
    }
  },
  components: {
    'user-modules': UserModules,
    'guest-logins': GuestLogins,
  },
  computed: {},
  beforeDestroy() {},
  mounted() {},
  methods: {},
}
</script>

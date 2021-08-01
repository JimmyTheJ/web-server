<template>
  <div>
    <v-layout v-if="!isMobile">
      <v-flex xs2>
        <v-card class="mx-auto fill-height">
          <v-navigation-drawer permanent width="100%" class="fill-height">
            <v-row class="fill-height" no-gutters>
              <v-list class="grow">
                <v-list-item v-for="route in routes" :key="route.name" link>
                  <router-link :to="route.path">
                    <v-list-item-title
                      v-text="route.meta.display"
                    ></v-list-item-title>
                  </router-link>
                </v-list-item>
              </v-list>
            </v-row>
          </v-navigation-drawer>
        </v-card>
      </v-flex>
      <v-flex xs10>
        <router-view />
      </v-flex>
    </v-layout>
    <v-layout v-else row>
      <v-flex xs12>
        <v-toolbar flat tile dense>
          <template v-for="(route, i) in routes">
            <v-btn :to="route.path" class="mx-2" :key="i">
              {{ route.meta.display }}
            </v-btn>
          </template>
        </v-toolbar>
      </v-flex>
      <v-flex xs12>
        <router-view />
      </v-flex>
    </v-layout>
  </div>
</template>

<script>
const FN = 'admin-tools'

import Auth from '@/mixins/authentication'
import UserModules from '@/components/modules/admin/user-modules.vue'
import GuestLogins from '@/components/modules/admin/guest-logins.vue'
import RegisterUser from '@/components/modules/admin/register-user.vue'

export default {
  mixins: [Auth],
  data() {
    return {
      routes: [],
    }
  },
  components: {
    'user-modules': UserModules,
    'guest-logins': GuestLogins,
    'register-user': RegisterUser,
  },
  beforeDestroy() {},
  created() {
    this.routes = this.$router.getRoutes().filter(route => {
      if (typeof route.parent !== 'undefined' && route.parent !== null) {
        if (route.parent.name === 'admin-tools') {
          return true
        }
      }
      return false
    })
  },
  mounted() {
    this.$_console_log('Routes: ', this.routes)
    if (typeof this.$store.state.auth.admin === 'undefined')
      this.$store.dispatch('getRoles')
  },
  computed: {
    isMobile() {
      let val = this.$vuetify.breakpoint.name
      return val === 'xs' || val === 'sm'
    },
  },
  methods: {},
}
</script>

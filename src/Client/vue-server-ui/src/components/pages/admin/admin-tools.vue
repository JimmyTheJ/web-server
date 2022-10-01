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
    <div v-else>
      <v-layout row no-gutters>
        <v-flex xs1>
          <v-menu
            :close-on-content-click="true"
            :nudge-width="200"
            v-model="menu"
            offset-x
          >
            <template v-slot:activator="{ on }">
              <v-btn v-on="on" icon><fa-icon icon="bars"></fa-icon></v-btn>
            </template>
            <v-card>
              <v-list>
                <template v-for="(route, i) in routes">
                  <v-list-item :to="{ name: route.name }" :key="i">
                    <v-list-item-title class="ml-2">
                      {{ route.meta.display }}
                    </v-list-item-title>
                  </v-list-item>
                </template>
              </v-list>
            </v-card>
          </v-menu>
        </v-flex>
        <v-flex xs10>
          <div class="text-center headline">
            {{ $route.meta.display }}
          </div>
        </v-flex>
        <v-flex xs12>
          <router-view />
        </v-flex>
      </v-layout>
    </div>
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
      menu: null,
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
    this.$_console_log(`[${FN}] Routes: `, this.routes)
    if (typeof this.$store.state.auth.admin === 'undefined')
      this.$store.dispatch('getRoles')
  },
  computed: {
    isMobile() {
      let val = this.$vuetify.breakpoint.name
      return val === 'xs' || val === 'sm'
    },
    getDrawerHeight() {
      return 16 + this.menuItems * 48
    },
  },
  methods: {},
}
</script>

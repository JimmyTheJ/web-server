<template>
  <div>
    <v-navigation-drawer
      style="top: 56px; overflow: hidden"
      v-model="drawer"
      class="grey lighten-4"
      disable-resize-watcher
      width="180"
      :height="getDrawerHeight"
      absolute
      app
    >
      <v-list light>
        <template v-for="child in routes">
          <v-list-item
            :to="{ name: child.name }"
            v-if="authorized(child) && !child.meta.hidden"
            :key="child.name"
            @click="drawer = false"
          >
            <v-list-item-content>
              {{ child.meta.display }}
            </v-list-item-content>
          </v-list-item>
        </template>
      </v-list>
    </v-navigation-drawer>

    <v-app-bar color="purple" dark>
      <v-app-bar-nav-icon
        @click.stop.prevent="drawer = !drawer"
      ></v-app-bar-nav-icon>
      <div v-show="$vuetify.breakpoint.name !== 'xs'">
        <v-toolbar-items dark>
          <div
            v-for="(child, index) in routes"
            :key="index"
            class="menu-toolbar-item"
          >
            <template v-if="authorized(child)">
              <div v-show="index + 1 < maxMenuItems">
                <!--<router-link :to="child.route"></router-link>-->
                <template v-if="!child.meta.hidden">
                  <v-btn :to="{ name: child.name }" dark text>{{
                    child.meta.display
                  }}</v-btn>
                  <v-divider class="mx-3" inset vertical></v-divider>
                </template>
              </div>
            </template>
          </div>
          <!--<div v-show="maxMenuItems < menuItems">...</div>-->
        </v-toolbar-items>
      </div>
      <v-spacer></v-spacer>
      <v-badge bordered :content="numNewMessages" :value="numNewMessages" mr-2>
        <v-icon medium @click="$store.dispatch('openNotifications')"
          >mdi-bell</v-icon
        >
      </v-badge>
      <v-menu
        :close-on-content-click="true"
        :nudge-width="200"
        v-model="menu"
        offset-x
      >
        <template v-slot:activator="{ on }">
          <v-btn v-on="on" icon
            ><fa-icon icon="ellipsis-v" size="2x"></fa-icon
          ></v-btn>
        </template>
        <v-card>
          <v-list>
            <v-list-item :to="{ name: 'profile' }">
              <v-avatar>
                <v-img v-if="hasAvatar" :src="avatarPath"></v-img>
                <fa-icon v-else icon="user" size="2x"></fa-icon>
              </v-avatar>
              <v-list-item-title class="ml-2">{{
                user.displayName
              }}</v-list-item-title>
            </v-list-item>
            <v-list-item @click="$_auth_logout">
              <fa-icon icon="door-open" size="2x"></fa-icon>
              <v-list-item-title class="ml-2">Logout</v-list-item-title>
            </v-list-item>
          </v-list>
        </v-card>
      </v-menu>
    </v-app-bar>
  </div>
</template>

<script>
import { mapState } from 'vuex'

import Auth from '../../mixins/authentication'
import Module from '../../mixins/module'
import { Roles } from '../../constants'

export default {
  data() {
    return {
      routes: [],
      drawer: false,

      menu: false,
      menuItems: 0,
      maxMenuItems: 0,

      authLevel: 0,

      screenSize: window.outerWidth,
    }
  },
  mixins: [Auth, Module],
  props: {
    source: String,
  },
  computed: {
    ...mapState({
      modules: state => state.auth.modules,
      user: state => state.auth.user,
      conversations: state => state.chat.conversations,
      notificationMsgs: state => state.notifications.messages,
    }),
    getDrawerHeight() {
      return 16 + this.menuItems * 48
    },
    hasAvatar() {
      if (
        typeof this.user.avatar === 'undefined' ||
        this.user.avatar === null ||
        this.user.avatar === ''
      ) {
        return false
      }

      return true
    },
    avatarPath() {
      return `${process.env.VUE_APP_API_URL}/public/${this.user.avatar}`
    },
    numNewMessages() {
      return this.$store.getters.numNewMsgs
    },
  },
  created() {
    this.getAuthLevel()

    this.routes = this.$router.getRoutes().filter(route => {
      if (typeof route.parent !== 'undefined' && route.parent !== null) {
        if (route.parent.name === 'home') {
          return true
        }
      }
      return false
    })

    this.getMenuItemCount()
  },
  mounted() {
    this.getMaxMenuItems()
    window.addEventListener('resize', this.resizeScreen, false)
  },
  beforeDestroy() {
    window.removeEventListener('resize', this.resizeScreen, false)
  },
  methods: {
    getMaxMenuItems() {
      const menuItems = document.getElementsByClassName('menu-toolbar-item')

      // Display one menu item if something has gone wrong
      if (typeof menuItems === 'undefined' || menuItems === null) {
        this.$_console_log('Menu items are null or empty')
        return 1
      }

      let width = 0

      // Go through each of the HTML elements and add up their widths
      menuItems.forEach(item => {
        if (typeof item.clientWidth === 'undefined') return
        width += item.clientWidth
      })

      const average = width / menuItems.length

      // Truncate the decimals of the value and make sure to factor in the to menu on the sides
      this.maxMenuItems = Math.floor((this.screenSize - 96) / average)
    },
    resizeScreen(e) {
      this.screenSize = window.outerWidth
      this.getMaxMenuItems()
    },
    getAuthLevel() {
      let role = this.$store.state.auth.role

      if (role === Roles.Name.Admin) this.authLevel = Roles.Level.Admin
      else if (role === Roles.Name.Elevated)
        this.authLevel = Roles.Level.Elevated
      else if (role === Roles.Name.General) this.authLevel = Roles.Level.General
      else this.authLevel = Roles.Level.Default
    },
    authorized(route) {
      // Check authorization level first
      if (route.meta.authLevel > this.authLevel) {
        //this.$_console_log("[Menu] Level is greater than auth level. Not authorized");
        return false
      }
      // Allow all default level routes without additional checks
      if (route.meta.authLevel <= Roles.Level.Default) {
        return true
      }
      // Show all admin only features to admins
      if (
        this.authLevel === Roles.Level.Admin &&
        route.meta.authLevel === Roles.Level.Admin
      ) {
        return true
      }

      return this.$_module_userHasModule(route)
    },
    getMenuItemCount() {
      this.$_console_log('[Menu] Get Menu Count.')
      for (let i = 0; i < this.routes.length; i++) {
        let auth = this.authorized(this.routes[i])
        if (auth && !this.routes[i].meta.hidden) {
          this.menuItems++
        }
      }
    },
    //checkScreenSize(i) {
    //    this.$_console_log('Checking screen after resize... index value: ' + (i + 1));

    //    if ((this.screenSize - 50) / (i + 1) < 190) {
    //        this.screenOverflow = true;
    //        return false;
    //    }

    //    this.screenOverflow = false;
    //    return true;
    //}
  },
}
</script>

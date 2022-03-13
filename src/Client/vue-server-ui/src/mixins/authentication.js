import store from '@/store/index'
import { defaultRoutes, adminRoutes, moduleRoutes } from '@/routes'
import { Roles } from '@/constants'
import ConMsgs from './console'

export default {
  methods: {
    $_auth_checkLogin(err) {
      ConMsgs.methods.$_console_log(
        '[Authentication mixin] $_auth_checkLogin: Called'
      )

      if (store.state.auth.isAuthorize) {
        this.$router.replace(this.$route.query.redirect || '/home')
      }
      return err
    },
    async $_auth_login(data) {
      ConMsgs.methods.$_console_log(
        '[Authentication mixin] $_auth_login: Called'
      )
      let error = false
      await this.$store
        .dispatch('signin', data)
        .then(async () => {
          // Get modules for this user
          await this.$store
            .dispatch('getModules')
            .then(() => ConMsgs.methods.$_console_log('Got user modules'))
            .catch(() =>
              ConMsgs.methods.$_console_log('Failed to get user modules')
            )

          this.$_auth_addRoutes()
          this.$_auth_checkLogin(error)
        })
        .catch(ex => {
          ConMsgs.methods.$_console_log('Error logging in', ex)
          this.$store.dispatch('clearCredentials')
          error = true
        })
    },
    async $_auth_register(data) {
      ConMsgs.methods.$_console_log(
        '[Authentication mixin] $_auth_register: Called',
        data
      )
      await this.$store
        .dispatch('register', data)
        .then(resp => {
          // Uncomment to auto login after registering (Make it a setting ?)
          //this.$_auth_login(data)
          //    .then(resp2 => ConMsgs.methods.$_console_log("success in login"))
          //    .catch(() => ConMsgs.methods.$_console_log("error in login"));
          return Promise.resolve(resp.data)
        })
        .catch(() => {
          ConMsgs.methods.$_console_log('Error registering')
          return Promise.reject('Failed to create user')
        })
    },
    async $_auth_logout() {
      ConMsgs.methods.$_console_log(
        '[Authentication mixin] $_auth_logout: Called'
      )
      await this.$store
        .dispatch('signout')
        .then(() => {})
        .catch(() => ConMsgs.methods.$_console_log('logout fail'))
        .then(() => this.$router.push({ name: 'index' }))
    },
    $_auth_convertRole(r) {
      ConMsgs.methods.$_console_log(
        '[Authentication mixin] $_auth_convertRole: Called'
      )

      let role = Roles.Level.Default

      if (typeof r === 'undefined' || r === null) return Roles.Level.Default

      Object.keys(Roles.Level).forEach((val, index) => {
        if (Roles.Name[val] == r) role = Roles.Level[val]
      })

      ConMsgs.methods.$_console_log(`[Authentication] convertRole: ${role}`)

      return role
    },
    $_auth_addRoutes() {
      let self = this
      let activeModules = this.$store.state.auth.activeModules

      ConMsgs.methods.$_console_log(
        '[Authentication] addRoutes: Active modules: ',
        activeModules
      )
      ConMsgs.methods.$_console_log(
        '[Authentication] addRoutes: Old list of routes: ',
        this.$router.getRoutes()
      )

      // If user if an admin we need to add the admin routes so they can start modifying module / feature permissions and creating accounts
      if (this.$store.state.auth.role === Roles.Name.Admin) {
        adminRoutes.forEach(item => {
          self.$router.addRoute('home', item)
        })
      }

      // Add any routes that don't require any permission level, these will by definition not be included in the module list from the API
      defaultRoutes.forEach(item => {
        if (item.meta.authLevel === Roles.Level.Default) {
          self.$router.addRoute('home', item)
        }
      })

      // Go through the API returned modules this user has active and add their respective routes
      activeModules.forEach(module => {
        let foundItemByName = moduleRoutes.find(x => x.name === module.id)
        ConMsgs.methods.$_console_log(
          '[Authentication] addRoutes: Found Item by name: ',
          foundItemByName
        )

        let foundItemByMeta = moduleRoutes.find(
          x => x.meta.relative === module.id
        )
        ConMsgs.methods.$_console_log(
          '[Authentication] addRoutes: Found Item by meta.relative: ',
          foundItemByMeta
        )

        if (
          foundItemByName !== undefined &&
          this.$router.getRoutes().find(x => x.name === foundItemByName) ===
            undefined
        ) {
          self.$router.addRoute('home', foundItemByName)
        }

        if (
          foundItemByMeta !== undefined &&
          this.$router.getRoutes().find(x => x.name === foundItemByMeta) ===
            undefined
        ) {
          self.$router.addRoute('home', foundItemByMeta)
        }
      })

      ConMsgs.methods.$_console_log(
        '[Authentication] addRoutes: New list of routes: ',
        this.$router.getRoutes()
      )
    },
  },
}

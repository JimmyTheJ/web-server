import store from '@/store/index'
import {
  defaultRoutes,
  adminRoutes,
  moduleRoutes,
  adminDirectoryManagementChildRoute,
} from '@/routes'
import { Modules, Roles, TokenValidation } from '@/constants'
import ConMsgs from './console'

import chat from '@/store/modules/chat'
import library from '@/store/modules/library'
import directory from '@/store/modules/directory'

import Dispatcher from '@/services/ws-dispatcher'
import authService from '@/services/auth'

export default {
  methods: {
    async $_auth_checkLogin(skipTokenCheck) {
      let auth = this.$store.state.auth

      ConMsgs.methods.$_console_log(
        '[Authentication mixin] $_auth_checkLogin: Called'
      )

      if (store.state.auth.isAuthorize) {
        let tokenValid = false

        if (!skipTokenCheck) {
          await Dispatcher.request( async () => {
            await authService
              .validateToken(auth.accessToken)
              .then(res => {
                this.$_console_log('Successfully validated token.', res)
                switch (res.data) {
                  case TokenValidation.Valid:
                    tokenValid = true
                    break
                  case TokenValidation.RequiresNewJwt:
                    this.$store
                      .dispatch('refreshToken')
                      .then(() => {
                        tokenValid = true
                      })
                      .catch(e => {
                        // Eat it
                      })
                    break
                  case TokenValidation.MissingRefreshToken:
                  case TokenValidation.InvalidRefreshToken:
                  default:
                    ConMsgs.methods.$_console_log('Token missing or invalid')
                    break
                }
              })
              .catch(e => {})
          })
        }
        else {
          tokenValid = true
        }

        if (tokenValid) {
          // Get modules for this user
          await this.$store
            .dispatch('getModules')
            .then(() => ConMsgs.methods.$_console_log('Got user modules'))
            .catch(() => {
              ConMsgs.methods.$_console_log('Failed to get user modules')
              return false
            })

          this.$_auth_addModules()
          this.$_auth_addRoutes()
          
          let activeModules = this.$store.state.module.activeModules
          if (activeModules.findIndex(x => x.id === Modules.Chat) > -1) {
            await this.$store.dispatch('getUsersMap')
            .then(() => { ConMsgs.methods.$_console_log('Succesfully got user map')})
            .catch(() => { ConMsgs.methods.$_console_log('Failed to get user map')})
          }

          this.$router.replace(this.$route.query.redirect || '/home')
          return true
        }
        else {
          return false
        }
      }

      return false
    },
    async $_auth_login(data) {
      ConMsgs.methods.$_console_log(
        '[Authentication mixin] $_auth_login: Called'
      )

      try {
        let resp = await this.$store.dispatch('signin', data)
        this.$_auth_checkLogin(true)

        return Promise.resolve(resp)
      }
      catch (e) {
        ConMsgs.methods.$_console_log('Error logging in', ex)
        this.$store.dispatch('clearCredentials', new Array())

        return Promise.reject(false)
      }
    },
    async $_auth_register(data) {
      ConMsgs.methods.$_console_log(
        '[Authentication mixin] $_auth_register: Called',
        data
      )
      await this.$store
        .dispatch('register', data)
        .then(resp => {
          return Promise.resolve(resp)
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
        .then(() => ConMsgs.methods.$_console_log('logout success'))
        .catch(() => ConMsgs.methods.$_console_log('logout fail'))
        .then(() => {          
          this.$_auth_removeModules()
          this.$router.push({ name: 'index' })
        })
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
      let activeModules = self.$store.state.module.activeModules

      ConMsgs.methods.$_console_log(
        '[Authentication] addRoutes: Active modules: ',
        activeModules
      )
      ConMsgs.methods.$_console_log(
        '[Authentication] addRoutes: Old list of routes: ',
        self.$router.getRoutes()
      )

      // If user if an admin we need to add the admin routes so they can start modifying module / feature permissions and creating accounts
      if (self.$store.state.auth.role === Roles.Name.Admin) {
        let adminTools = adminRoutes.find(x => x.path === 'admin-tools')
        // Append the Directory Management sub-route into admin-tools assuming we have the Directory Module loaded
        if (
          self.$store.state.module.enabledModules.indexOf(
            Modules.Directory.charAt(0).toUpperCase() +
              Modules.Directory.slice(1)
          ) > -1
        ) {
          adminTools.children.push(adminDirectoryManagementChildRoute)
        }

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
          self.$router.getRoutes().find(x => x.name === foundItemByName) ===
            undefined
        ) {
          self.$router.addRoute('home', foundItemByName)
        }

        if (
          foundItemByMeta !== undefined &&
          self.$router.getRoutes().find(x => x.name === foundItemByMeta) ===
            undefined
        ) {
          self.$router.addRoute('home', foundItemByMeta)
        }
      })

      ConMsgs.methods.$_console_log(
        '[Authentication] addRoutes: New list of routes: ',
        self.$router.getRoutes()
      )
    },
    $_auth_addModules() {
      let self = this
      let activeModules = self.$store.state.module.activeModules

      ConMsgs.methods.$_console_log(
        '[Authentication] addModules: Active modules: ',
        activeModules
      )

      activeModules.forEach(module => {
        switch (module.id) {
          case 'directory':
            if (!self.$store.hasModule('directory')) {
              self.$store.registerModule('directory', directory)
            }
            break
          case 'chat':
            if (!self.$store.hasModule('chat')) {
              self.$store.registerModule('chat', chat)
            }
            break
          case 'library':
            if (!self.$store.hasModule('library')) {
              self.$store.registerModule('library', library)
            }
            break
          default:
            break
        }
      })
    },
    $_auth_removeModules() {
      let self = this
      let activeModules = self.$store.state.module.activeModules

      ConMsgs.methods.$_console_log(
        '[Authentication] addModules: Active modules: ',
        activeModules
      )

      activeModules.forEach(module => {
        if (self.$store.hasModule(module.id)) {
          self.$store.unregisterModule(module.id)
        }
      })
    },
  },
}

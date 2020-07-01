import * as MUTATIONS from '../store/mutation_types'
import store from '../store/index'
import { Roles } from '../constants'
import ConMsgs from './console'

export default {
    methods: {
        $_auth_checkLogin(err) {
            ConMsgs.methods.$_console_log('[Authentication mixin] $_auth_checkLogin: Called');

            if (store.state.auth.isAuthorize) {
                this.$router.replace(this.$route.query.redirect || '/home');
            }
            return err;
        },
        async $_auth_login(data) {
            ConMsgs.methods.$_console_log('[Authentication mixin] $_auth_login: Called');
            let error = false;
            await this.$store.dispatch('signin', data)
                .then(async resp => {
                    // Get CSRF token
                    this.$store.dispatch('getCsrfToken')
                        .then(() => ConMsgs.methods.$_console_log("Got csrf token!"))
                        .catch(() => ConMsgs.methods.$_console_log("Did not get token :("))

                    this.$store.dispatch('getNewConversationNotifications')
                        .then(resp => {
                            ConMsgs.methods.$_console_log('New message notifications:', resp.data);
                            this.buildNotificationList(resp.data);
                        }).catch(() => ConMsgs.methods.$_console_log("No new message notifications"))

                    // Get modules for this user
                    await this.$store.dispatch('getModules')
                        .then(() => ConMsgs.methods.$_console_log("Got user modules"))
                        .catch(() => ConMsgs.methods.$_console_log("Failed to get user modules"))

                    this.$_auth_checkLogin(error);
                }).catch(() => {
                    this.$store.dispatch('signout')
                    error = true;
                });
        },
        async $_auth_register(data) {
            ConMsgs.methods.$_console_log('[Authentication mixin] $_auth_register: Called');
            await this.$store.dispatch('register', data)
                .then(resp => {
                    // Uncomment to auto login after registering (Make it a setting ?)
                    //this.$_auth_login(data)
                    //    .then(resp2 => ConMsgs.methods.$_console_log("success in login"))
                    //    .catch(() => ConMsgs.methods.$_console_log("error in login"));
                }).catch(() => {
                    ConMsgs.methods.$_console_log("Error registering");
                    return Promise.reject("Failed to create user");
                });
        },
        async $_auth_logout() {
            ConMsgs.methods.$_console_log('[Authentication mixin] $_auth_logout: Called');
            await this.$store.dispatch("signout")
                .then(resp => {
                    ConMsgs.methods.$_console_log("Logout success. Clearing state.")
                    this.$store.dispatch("clearNotifications");
                    this.$store.dispatch("clearLibrary");
                    this.$store.dispatch("clearFileExplorer");
                }).catch(() => ConMsgs.methods.$_console_log("logout fail"))
                .then(() => this.$router.push({ name: 'index' }) );
        },
        $_auth_convertRole(r) {
            ConMsgs.methods.$_console_log('[Authentication mixin] $_auth_convertRole: Called');

            let role = Roles.Level.Default;

            if (typeof r === 'undefined' || r === null)
                return Roles.Level.Default;

            Object.keys(Roles.Level).forEach((val, index) => {
                if (Roles.Name[val] == r)
                    role = Roles.Level[val];
            });

            ConMsgs.methods.$_console_log(`[Authentication] convertRole: ${role}`);

            return role;
        },
        buildNotificationList(data) {
            if (!Array.isArray(data) || data.length === 0) {
                return;
            }

            // TODO: Build notification list here
            data.forEach(value => {
                if (Array.isArray(value.messages) && value.messages.length > 0) {
                    // There are new messages
                    const obj = {
                        text: `New messages from ${value.title}`,
                        type: 1
                    }

                    this.$store.dispatch('pushNotification', obj);
                }
            })
        },                            
    }
}

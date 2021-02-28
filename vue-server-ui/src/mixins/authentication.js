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
                    // Get the list of all other users for the chat system
                    this.$store.dispatch('getAllOtherUsers')
                        .then(() => ConMsgs.methods.$_console_log("Got user list"))
                        .catch(() => ConMsgs.methods.$_console_log("Failed to get user list"))

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
            ConMsgs.methods.$_console_log('[Authentication mixin] $_auth_register: Called', data);
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
                .then(() => {}).catch(() => ConMsgs.methods.$_console_log("logout fail"))
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
    }
}
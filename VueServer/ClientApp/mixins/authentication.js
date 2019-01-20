import * as MUTATIONS from '../store/mutation_types'
import { Roles } from '../constants'
import ConMsgs from './console'

export default {
    data() {
        return {
            error: false
        }
    },
    methods: {
        checkCurrentLogin() {
            if (localStorage.accessToken) {
                this.$router.replace(this.$route.query.redirect || '/home');
            }
            return this.error;
        },
        async login(data) {
            await this.$store.dispatch('signin', data)
                .then(resp => {
                    this.$store.dispatch('getCsrfToken')
                        .then(resp => ConMsgs.methods.$_console_log("Got csrf token!"))
                        .catch(() => ConMsgs.methods.$_console_log("Did not get token :("))
                }).catch(() => {
                    this.$store.dispatch('signout')
                    //this.$store.dispatch('getCsrfToken')
                    //    .then(response => ConMsgs.methods.$_console_log("Got csrf token!"))
                    //    .catch(() => ConMsgs.methods.$_console_log("Did not get token :("))
                    //ConMsgs.methods.$_console_log("Login error");
                    this.error = true;
                });
            this.checkCurrentLogin();
        },
        async register(data) {
            await this.$store.dispatch('register', data)
                .then(resp => {
                    this.login(data)
                        .then(resp2 => ConMsgs.methods.$_console_log("success in login"))
                        .catch(() => ConMsgs.methods.$_console_log("error in login"));
                }).catch(() => {
                    ConMsgs.methods.$_console_log("Error registering");
                    this.error = true;
                });
        },
        async logout() {
            await this.$store.dispatch("signout")
                .then(resp => ConMsgs.methods.$_console_log("Logout success"))
                .catch(() => ConMsgs.methods.$_console_log("logout fail"))
                .then(() => this.$router.push({ name: 'index' }) );
        },
        convertRole(r) {
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

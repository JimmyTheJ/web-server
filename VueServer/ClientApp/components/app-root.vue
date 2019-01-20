<template>
    <div id="app">
        <v-app dark>
            <router-view></router-view>
        </v-app>
    </div>
</template>

<script>
    import * as CONST from '../constants'
    import { setTimeout } from 'core-js';

    export default {
        data() {
            return {
                
            }
        },
        created() {
            if (this.$store.getters.getIsAuthorize) {
                this.$store.dispatch('getCsrfToken').then(resp => {
                    this.$_console_log("Got csrf token!")
                }).catch(() => this.$_console_log("Did not get token :("))
            }
        },
        created() {
            this.refreshTokenJob();
        },
        methods: {
            refreshTokenJob() {
                this.$_console_log('Refresh token job started');
                setTimeout(() => {
                    this.$_console_log('Token timeout reached. Getting new tokens');
                    if (this.$store.getters.getIsAuthorize) {
                        this.$store.dispatch('refreshToken').then(resp => {
                            this.$store.dispatch('getCsrfToken').then(resp => {
                                this.$_console_log("Got csrf token!");
                            }).catch(() => this.$_console_log("Did not get token :("))
                        }).catch(() => this.$_console_log('Failed to get time released refresh token'));
                    }
                    this.refreshTokenJob();
                }, CONST.Admin.RefreshTokenTimer);
            },
        }
    }
</script>

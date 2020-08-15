<template>
    <div id="app">
        <v-app>
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
            //this.refreshTokenJob();
        },
        methods: {
            refreshTokenJob() {
                this.$_console_log('Refresh token job started');
                setTimeout(() => {
                    this.$_console_log('Token timeout reached. Getting new tokens');
                    if (this.$store.state.auth.isAuthorize) {
                        this.$store.dispatch('refreshToken').then(() => {}).catch(() => this.$_console_log('Failed to get time released refresh token'));
                    }
                    this.refreshTokenJob();
                }, CONST.Admin.RefreshTokenTimer);
            },
        }
    }
</script>

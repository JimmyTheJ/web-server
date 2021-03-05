<template>
    <div class="main-container">
        <h1 class="text-xs-center">Jimmy's Webserver</h1>
        <router-view></router-view>
        <div v-if="$route.name === 'login' && env === 'development'" class="text-xs-center ma-1">
            <p>No account ? <router-link to="register">Register here</router-link></p>
        </div>
        <div v-else-if="$route.name === 'register'" class="text-xs-center ma-1">
            <p>If you already have an account you can <router-link to="login">Login</router-link></p>
        </div>
    </div>
</template>

<script>
    export default {
        data() {
            return {
                env: process.env.NODE_ENV
            }
        },
        created() {
            if (this.$route.fullPath === '/')
                this.$router.push({ name: 'login' });
        },
    }
</script>


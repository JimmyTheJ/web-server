<template>
    <div>
        <main-menu :source="`home`"></main-menu>
        <router-view :key="$route.fullPath"></router-view>
        <notification-bar></notification-bar>
    </div>
</template>

<script>
    import Menu from './modules/menu'
    import NotificationBar from './modules/notification-bar'

    export default {
        data() {
            return {

            }
        },
        components: {
            'main-menu': Menu,
            'notification-bar': NotificationBar,
        },
        created() {
            if (!this.$store.state.auth.isAuthorize)
                this.$router.push('/');

            if (this.$route.fullPath === '/home')
                this.$router.push({ name: 'start' });
        },
        methods: {

        },
    }
</script>

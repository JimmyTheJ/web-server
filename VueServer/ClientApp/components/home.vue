<template>
    <div>
        <main-menu :source="`home`"></main-menu>
        <router-view></router-view>
    </div>
</template>

<script>
    import Menu from './modules/menu'

    export default {
        data() {
            return {

            }
        },
        components: {
            'main-menu': Menu
        },
        created() {
            if (!this.$store.getters.getIsAuthorize)
                this.$router.push('/');

            if (this.$route.fullPath === '/home')
                this.$router.push({ name: 'start' });
        },
        methods: {

        },
    }
</script>

<template>
    <v-img :src="getPath" :max-width="getMaxWidth" :max-height="getMaxHeight" contain />
</template>

<script>
    import { mapState } from 'vuex'

    export default {
        name: "text-viewer",
        data() {
            return {
                windowHeight: window.innerHeight,
                windowWidth: window.innerWidth,

                basepath: process.env.API_URL,
            }
        },
        props: {
            url: {
                type: String,
                required: true,
            },
        },
        computed: {
            ...mapState({
                accessToken: state => state.auth.accessToken
            }),
            getMaxWidth() {
                return this.windowWidth * 0.8;
            },
            getMaxHeight() {
                return this.windowHeight * 0.66
            },
            getPath() {
                return `${this.basepath}/api/serve-file/${this.url}?token=${this.accessToken}`
            },
        },
        watch: {
            url(newValue) {
                this.$_console_log('[Image Viewer] Url watcher: url value', newValue)
            },
        },
        mounted() {
            window.addEventListener('resize', this.getWindowSize);
        },
        beforeDestroy() {
            window.removeEventListener('resize', this.getWindowSize);
        },
        methods: {
            getWindowSize() {
                this.windowHeight = window.innerHeight;
                this.windowWidth = window.innerWidth;
            },
        },
    }
</script>

<template>
    <div>
        {{ text }}
    </div>
</template>

<script>
    import service from '../../../services/file-explorer'

    export default {
        name: "text-viewer",
        data() {
            return {
                text: null
            }
        },
        props: {
            url: {
                type: String,
                required: true,
            },
            on: {
                type: Boolean,
                required: true,
            }
        },
        watch: {
            on(newValue) {
                if (newValue === true)
                    this.getData();
            },
            url(newValue) {
                this.$_console_log('[Text Viewer] Url watcher: url value', newValue)

                this.getData();
            },
        },
        methods: {
            getData() {
                if (typeof this.url === 'undefined' || this.url === null || this.url === '') {
                    this.$_console_log('[Text Viewer] Get Data: url value is null or empty');
                    return;
                }

                const tmpUrl = this.url;
                service.getFile(this.url).then(resp => {
                    this.text = resp.data;
                }).catch(() => {
                    this.$_console_log(`[Text Viewer] Get Data: Failed to get data from ${tmpUrl}`);
                });
            },
        }
    }
</script>

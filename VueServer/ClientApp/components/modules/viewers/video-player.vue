<template>
    <iframe :src="path" width="480" height="320">

    </iframe>
    <!--<div class="video-container center">-->

        <!--<video id="video-player"
               class="video-player"
               ref="player"
               preload="none"
               width="420"
               controls>
            <source :src="path" />
        </video>-->
    <!--</div>-->
</template>

<script>
    import service from '../../../services/file-explorer'

    const basepath = process.env.API_URL;

    export default {
        name: "video-player",
        data() {
            return {
                player: null,
                path: null,
                loadingFile: false,
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
        mounted() {
            this.player = document.getElementById('video-player');
        },
        computed: {
            type: function () {
                if (typeof this.url === 'undefined' || this.url === '') {
                    this.$_console_log('[VIDEO PLAYER] type: Undefined or empty');
                    return "";
                }

                if (!this.url.includes('.')) {
                    this.$_console_log('[VIDEO PLAYER] type: Doesn\'t include a period (.) ');
                    return "";
                }
                
                const index = this.url.lastIndexOf('.');
                const extension = this.url.slice(index).toLowerCase();

                // TODO: Create exhaustive list of extensions
                switch (extension) {
                    case '.mkv':
                    case '.avi':
                    case '.mpeg':
                    case '.mpg':
                    case '.mp4':
                    case '.wmv':
                        return 'video/mp4';
                    case '.webm':
                        return 'video/webm';
                    case '.mp3':
                        return 'audio/mpeg';
                    default:
                        return '';
                }
            },
        },
        watch: {
            on(newValue) {
                if (newValue === true) {
                    this.getFile(this.url);
                }
            },
            //on(newValue) {
            //    this.pausePlayer();
            //    if (newValue === true)
            //        this.loadPlayer();
            //},
            url(newValue) {
                this.$_console_log('[Video Player] Url watcher: url value', newValue)
                this.getFile(newValue);
            },
            //url(newValue) {
            //    this.$_console_log('[Video Player] Url watcher: url value', newValue)

            //    this.pausePlayer();

            //    if (typeof newValue === 'undefined' || newValue === null || newValue === '') {
            //        this.$_console_log('[Video Player] Empty url string');
            //        return;
            //    }
            //    else {
            //        this.loadPlayer();
            //    }
            //},
        },
        methods: {
            getFile(val) {
                if (this.loadingFile === false) {
                    this.loadingFile = true;

                    service.getFilePath(val).then(resp => {
                        this.path = `${basepath}/${resp.data}`;
                    }).catch(() => this.$_console_log('Failed to serve media')).then(() => {
                        this.loadingFile = false;
                    });
                }
            }
            //loadPlayer() {
            //    this.path = `${basepath}/api/serve-file/${this.url}`

            //    const self = this;
            //    setTimeout(() => {
            //        self.player.load();
            //        if (self.player.canPlayType(self.type)) {
            //            self.player.play();
            //        }
            //        else {
            //            self.$_console_log('[Video Player] Load Player: Can\'t play type');
            //        }
                        
            //    }, 25);   
            //},
            //pausePlayer() {
            //    this.player.pause();
            //}
        }
    }
</script>

<style scoped>
    .center {
        margin: 0 auto;
    }
</style>

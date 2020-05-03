<template>
    <div>
        <file-explorer :folders="folders"
                       parentView="browser"
                       :goFile="goFile"
                       @loadFile="loadFile"></file-explorer>

        <file-viewer :open="dialogOpen" @viewer-off="dialogOpen = false" @file-back="browserGo(-1)" @file-forward="browserGo(1)">
            <template v-if="type === mediaTypes.video">
                <p slot="header">Video Player</p>
                <video-player :url="path" :on="on" @player-off="on = false" />
            </template>
            <template v-else-if="type === mediaTypes.image">
                <p slot="header">Image Viewer</p>
                <image-viewer :url="path" />
            </template>
            <template v-else-if="type === mediaTypes.text">
                <p slot="header">Text Viewer</p>
                <text-viewer :url="path" :on="on" @text-off="on = false" />
            </template>
        </file-viewer>
    </div>
</template>

<script>
    import Service from '../../services/file-explorer'

    import Explorer from '../modules/file-explorer'
    import FileViewer from '../modules/viewers/file-viewer'
    import VideoPlayer from '../modules/viewers/video-player'
    import TextViewer from '../modules/viewers/text-viewer'
    import ImageViewer from '../modules/viewers/image-viewer'

    import { Roles } from '../../constants'

    export default {
        data() {
            return {
                level: 0,
                folders: [],

                type: null,
                path: null,
                on: false,
                dialogOpen: false,

                mediaTypes: {
                    video: 'video',
                    image: 'image',
                    text: 'text'
                },

                goFile: {
                    file: null,
                    num : 0
                },
            }
        },
        components: {
            'file-explorer': Explorer,
            'file-viewer': FileViewer,
            'video-player': VideoPlayer,
            'text-viewer': TextViewer,
            'image-viewer': ImageViewer,
        },
        created() {
            let role = this.$store.state.auth.role;

            // TODO: Get the list from the server
            if (role === Roles.Name.Admin) {
                this.level = Roles.Level.Admin;
            }
            else if (role === Roles.Name.Elevated) {
                this.level = Roles.Level.Elevated;
            }
            else if (role === Roles.Name.General) {
                this.level = Roles.Level.General;
            }
            else
                this.$router.push({ name: 'start' });

            this.getFolders();
        },
        watch: {
            dialogOpen(newValue) {
                // Close dialog
                if (newValue === false) {
                    // If our player isn't on
                    if (!this.on) {
                        const self = this;
                        // Turn it on so when we turn it off it'll flip the watcher
                        this.on = true;
                        setTimeout(() => {
                            self.on = false;
                        }, 50);
                    }
                    // Otherwise just turn it off
                    else {
                        this.on = false;
                    }
                }
            }
        },
        methods: {
            async getFolders() {
                await Service.getFolderList().then(resp => {
                    this.folders = resp.data;
                }).catch(() => {
                    this.$_console_log("Failed to upload file");
                });
            },
            getType(file) {
                if (typeof file === 'undefined' || file === '') {
                    this.$_console_log('[File Viewer] getType: Undefined or empty');
                    return this.mediaTypes.text;
                }

                if (!file.includes('.')) {
                    this.$_console_log('[File Viewer] getType: Doesn\'t include a period (.) ');
                    return this.mediaTypes.text;
                }

                const index = file.lastIndexOf('.');
                const extension = file.slice(index).toLowerCase();

                // TODO: Create exhaustive list of extensions
                switch (extension) {
                    case '.mkv':
                    case '.avi':
                    case '.mpeg':
                    case '.mpg':
                    case '.mp4':
                    case '.wmv':
                    case '.webm':
                    case '.mp3':
                        return this.mediaTypes.video;
                    case '.jpg':
                    case '.jpeg':
                    case '.gif':
                    case '.tiff':
                    case '.bmp':
                    case '.png':
                    case '.img':
                        return this.mediaTypes.image;
                    default:
                        return this.mediaTypes.text;
                }
            },
            loadFile(file) {
                this.$_console_log('[BROWSER] LoadFile: ', file);

                this.type = this.getType(file);

                this.path = file;
                this.dialogOpen = true;
                setTimeout(() => {
                    this.on = true;
                }, 15);
                
            },
            browserGo(num) {
                if (typeof num !== 'number' || num === 0) {
                    this.$_console_log('[Browser] Browser Go: Number is not a number or 0');
                    return;
                }
                if (typeof this.path !== 'string') {
                    this.$_console_log('[Browser] Browser Go: Path is not a string');
                    return;
                }

                let filename;
                if (this.path.includes('/')) {
                    if (this.path.lastIndexOf('/') === this.path.length - 1) {
                        filename = this.path.substring(0, this.path.lastIndexOf('/'));
                        if (filename.includes('/')) {
                            if (filename.lastIndexOf('/') === filename.length - 1) {
                                filename = filename.substring(0, filename.lastIndexOf('/'));
                            }
                            else {
                                filename = filename.substring(filename.lastIndexOf('/') + 1);
                            }
                        }
                    }
                    else {
                        filename = this.path.substring(this.path.lastIndexOf('/') + 1);
                    }                    
                }
                else {
                    filename = this.path;
                }
                
                this.goFile = {
                    file: filename,
                    num: num
                }
            }
        }
    }
</script>

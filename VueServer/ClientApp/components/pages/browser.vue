<template>
    <div>
        <file-explorer :folders="folders"
                       parentView="browser"
                       @loadFile="loadFile"></file-explorer>

        <video-player :file="path" :on="videoOn" @player-off="videoOn = false"></video-player>
        <text-viewer :file="path" :on="textOn" @text-off="textOn = false"></text-viewer>
    </div>
</template>

<script>
    import { Roles } from '../../constants'
    import Service from '../../services/file-explorer'

    // Components
    import Explorer from '../modules/file-explorer'
    import VideoPlayer from '../modules/video-player'
    import TextViewer from '../modules/text-viewer'

    const TYPES = {
        None: 0,
        Image: 1,
        Text: 2,
        Video: 3
    };

    function getExtension(path) {
        console.log(typeof path);
        if (typeof path !== 'string' && typeof path !== 'String') {
            return;
        }

        const ext = path.substr(path.lastIndexOf('.'));
        return ext;
    }

    export default {
        data() {
            return {
                level: 0,
                folders: [],

                path: '',
                type: TYPES.None,

                videoOn: false,
                textOn: false,
                imageOn: false,
            }
        },
        components: {
            'file-explorer': Explorer,
            'video-player': VideoPlayer,
            'text-viewer': TextViewer,
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
        methods: {
            async getFolders() {
                await Service.getFolderList().then(resp => {
                    this.folders = resp.data;
                }).catch(() => {
                    this.$_console_log("Failed to upload file");
                });
            },
            openData() {
                if (typeof this.path === 'undefined' || this.path === null || this.path === '') {
                    this.$_console_log('[browser] openData: path is null or empty');
                    return;
                }

                const ext = getExtension(this.path);
                this.$_console_log('Extensions = ', ext);
                switch (ext) {
                    case '.mkv':
                    case '.avi':
                    case '.mpeg':
                    case '.mpg':
                    case '.mp4':
                    case '.wmv':
                    case '.webm':
                    case '.mp3':
                        this.videoOn = true;
                        this.textOn = false;
                        break;
                    case '.bat':
                    case '.conf':
                    case '.config':
                    case '.css':
                    case '.js':
                    case '.log':
                    case '.ini':
                    case '.ps1':
                    case '.rtf':
                    case '.sass':
                    case '.scss':
                    case '.sh':
                    case '.txt':
                    case '.xml':
                        this.videoOn = false;
                        this.textOn = true;
                        break;
                    default:
                        this.videoOn = false;
                        this.textOn = false;
                        break;
                }
                
            },
            loadFile(file) {
                // Add check to load different modules depending on what the file type is
                this.$_console_log('[BROWSER] LoadFile: ', file);
                this.path = file;
                this.openData();
            }
        }
    }
</script>

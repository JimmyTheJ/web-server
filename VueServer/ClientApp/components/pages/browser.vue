<template>
    <div>
        <file-explorer :folders="folders"
                       parentView="browser"
                       @loadFile="loadFile"></file-explorer>

        <video-player :url="path" :on="on" @player-off="on = false"></video-player>
    </div>
</template>

<script>
    import Service from '../../services/file-explorer'
    import Explorer from '../modules/file-explorer'
    import VideoPlayer from '../modules/video-player'
    import { Roles } from '../../constants'

    export default {
        data() {
            return {
                level: 0,
                folders: [],

                path: '',
                on: false
            }
        },
        components: {
            'file-explorer': Explorer,
            'video-player': VideoPlayer,
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
            loadFile(file) {
                // Add check to load different modules depending on what the file type is
                this.$_console_log('[BROWSER] LoadFile: ', file);
                this.path = file;
                this.on = true;
            }
        }
    }
</script>

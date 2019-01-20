<template>
    <file-explorer :folders="folders"
                   :level="level"
                   :parentView="`browser`"></file-explorer>
</template>

<script>
    import Service from '../../services/file-explorer'
    import Explorer from '../modules/file-explorer'
    import { Roles } from '../../constants'

    export default {
        data() {
            return {
                level: 0,
                folders: [],
            }
        },
        components: {
            'file-explorer': Explorer,
        },
        created() {
            let role = this.$store.getters.getUserRole;

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
        }
    }
</script>

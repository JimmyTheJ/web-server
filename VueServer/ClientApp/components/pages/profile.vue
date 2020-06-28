<template>
    <v-container>
        <v-card>
            <v-card-title class="headline text-center">
                <div>User Id: {{ user.id }}</div>
                <v-spacer></v-spacer>
                <label id="upload-button" for="upload-files" class="upload-button">
                    <v-avatar>
                        <v-img v-if="hasAvatar" :src="avatarPath"></v-img>
                        <v-icon v-else dark>fas fa-user</v-icon>
                    </v-avatar>
                </label>
                <input ref="fUpload" type="file" name="upload-files" id="upload-files" @change="uploadAvatarImage" hidden />
            </v-card-title>
            <v-card-text>
                <v-layout row>
                    <!--<v-flex xs12 class="headline text-center">Userid: {{ user.id }}</v-flex>-->
                    <v-flex xs4>
                        Display name:
                    </v-flex>
                    <v-flex xs8>
                        {{ user.displayName }}
                    </v-flex>
                </v-layout>
            </v-card-text>
        </v-card>
    </v-container>
</template>

<script>
    import { mapState } from 'vuex'

    export default {
        data() {
            return {
                uploadFile: null,
            }
        },
        computed: {
            ...mapState({
                user: state => state.auth.user,
            }),
            hasAvatar() {
                if (typeof this.user.avatar === 'undefined' || this.user.avatar === null || this.user.avatar === '') {
                    return false;
                }

                return true;
            },
            avatarPath() {
                return `${process.env.API_URL}/public/${this.user.avatar}`;
            },
        },
        methods: {
            uploadAvatarImage(e) {
                const files = e.target.files;
                console.log(files);
                if (files.length !== 1) {
                    this.$_console_log('SetFiles: File list contains more or less than 1 file');
                    return;
                }

                let formData = new FormData();
                formData.append("File", files[0]);

                this.$store.dispatch('updateAvatarImage', formData)
                    .then(() => { }).catch(() => { }).then(() => {
                        // Cleanup
                        this.uploadFile = null;
                        this.$refs.fUpload.value = '';
                    });
            },
        },
    }
</script>

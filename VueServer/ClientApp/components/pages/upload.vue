<template>
    <div>
        <v-container grid-list-xl>
            <v-expansion-panel v-model="panel" expand>
                <v-expansion-panel-content v-for="(folder, i) in folderFiles" :key="folder.id">
                    <div slot="header">{{ folder.folder }} ({{ folder.files.length }})</div>
                    <v-list>
                        <div v-if="folder.files.length === 0" class="text-xs-center">
                            No files here
                        </div>
                        <template v-for="file in folder.files">
                            <v-list-tile>
                                <v-list-tile-content>
                                    {{ file }}
                                </v-list-tile-content>
                                <v-list-tile-action v-if="isAdmin">
                                    <v-btn icon @click="deleteItem(file, folder.folder)"><fa-icon size="lg" icon="window-close" /></v-btn>
                                </v-list-tile-action>
                            </v-list-tile>
                        </template>
                    </v-list>
                </v-expansion-panel-content>
            </v-expansion-panel>
        </v-container>

        <v-container grid-list-xl v-show="files.length > 0">
            <div class="text-xs-center">Files being uploaded...</div>
            <v-list>
                <template v-for="(file, i) in files">
                    <v-list-tile>
                        <v-list-tile-content>
                            {{ file.name }}
                        </v-list-tile-content>
                    </v-list-tile>
                </template>
            </v-list>
        </v-container>

        <v-form enctype="multipart/form-data">
            <v-card id="drop-area">
                <v-layout row wrap>
                    <!--<v-flex xs12>
                        <div class="text-xs-center red--text text--accent-4 headline">Upload files here</div>
                    </v-flex>-->

                    <v-flex xs12>
                        <v-select :items="paths" v-model="path" label="Hard Drive"></v-select>
                    </v-flex>

                    <v-flex xs12 class="text-xs-center">
                        <label id="upload-button" for="upload-files">UPLOAD FILES</label>
                    </v-flex>
                    <input ref="fUpload" type="file" name="upload-files" id="upload-files" class="file-upload" multiple @change="setFiles" hidden />
                </v-layout>
            </v-card>
        </v-form>
        
    </div>
</template>

<script>
    import service from '../../services/upload'
    import * as CONST from '../../constants'

    let dropArea = null;

    export default {
        data() {
            return {
                paths: [],
                panel: [],
                path: '',
                files: [

                ],
                folderFiles: [

                ],
                isAdmin: false,
                c: CONST.Roles,
            }
        },
        created() {
            let role = this.$store.getters.getUserRole;

            if (role === CONST.Roles.Name.Admin) {
                this.isAdmin = true;
            }
            else if (role !== CONST.Roles.Name.Elevated) {
                this.$router.push({ name: 'start' });
            }

            this.getData();
        },
        methods: {
            async getData() {
                service.getFolders().then(resp => {
                    this.paths = resp.data
                    if (this.paths.length > 0) {
                        this.path = this.paths[0];
                    }
                }).catch(() => this.$_console_log('Failed to get list of folders'));

                service.getList().then(resp => {
                    this.folderFiles = resp.data
                }).catch(() => this.$_console_log('Failed to get uploaded file directory contents'));
            },
            setFiles(e) {
                this.$_console_log(e.target.files);
                for (let i = 0; i < e.target.files.length; i++) {
                    this.files.push(e.target.files[i]);
                }
                this.uploadFiles();
            },
            async sendFile(file) {
                let formData = new FormData();
                formData.append("File", file);
                formData.append("Name", this.path)

                await service.uploadFile(formData).then(resp => {
                    this.$_console_log("Successfully uploaded file");
                    let fIndex = this.folderFiles.findIndex(x => x.folder === this.path);
                    if (fIndex !== -1) {
                        this.folderFiles[fIndex].files.push(file.name);
                    }
                }).catch(() => this.$_console_log("Error uploading files"))
                .then(resp => {
                    let index = this.files.findIndex(x => x.name === file.name);
                    this.$_console_log(`Found file at index: ${index}`);
                    if (index !== -1)
                        this.files.splice(index, 1);
                });
            },
            //sortFolder() {
            //    let index = this.paths.findIndex(x => x.value === this.path);
            //    this.$_console_log('');
            //    //if (index > -1)
            //    //  this.folderFiles[index].files.sort();
            //},
            async uploadFiles() {
                while (this.files.length > 0) {
                    await this.sendFile(this.files[0]).then(resp => {
                        this.$_console_log("File sent!");
                    }).catch(() => this.$_console_log("Failed to upload file"));
                }

                this.$_console_log("Finished sending all files");
                //this.sortFolder();

                this.files = [];
                this.$refs.fUpload.value = '';
            },
            async deleteItem(file, folder) {
                if (!this.isAdmin)
                    return this.$_console_log("You're not allowed to do that sir...");

                this.$_console_log(`Delete: ${file} from folder: ${folder}`);
                let fi = file;
                let fol = folder;

                await service.deleteFile(file, folder).then(resp => {
                    this.$_console_log('Successfully deleted the file');
                    let fIndex = this.folderFiles.findIndex(x => x.folder === fol);
                    if (fIndex > -1) {
                        let fiIndex = this.folderFiles[fIndex].files.indexOf(fi);
                        this.folderFiles[fIndex].files.splice(fiIndex, 1);
                    }
                }).catch(() => this.$_console_log('Error deleting the item in upload'));
            },
        },
    }
</script>

<style scoped>
    #upload-button {
        border: 2px solid gray;
        border-radius: 7px;
        padding: 8px;
        margin: 8px;
        cursor: pointer;
    }

    #upload-button:hover {
        background-color: gray;
    }

    #drop-area {
        border: 2px dashed #ccc;
        border-radius: 35px;
        min-width: 120px;
        max-width: 240px;
        font-family: sans-serif;
        margin: 100px auto;
        padding: 20px;
    }

    /*#drop-area.highlight {
        border-color: purple;
    }*/

    p {
        margin-top: 0;
    }

    .my-form {
        margin-bottom: 10px;
    }

    /*#gallery {
        margin-top: 10px;
    }

    #gallery img {
        width: 150px;
        margin-bottom: 10px;
        margin-right: 10px;
        vertical-align: middle;
    }*/

    .button {
        display: inline-block;
        padding: 10px;
        background: #ccc;
        cursor: pointer;
        border-radius: 5px;
        border: 1px solid #ccc;
    }

    .button:hover {
        background: #ddd;
    }

    /*#fileElem {
        display: none;
    }*/
</style>

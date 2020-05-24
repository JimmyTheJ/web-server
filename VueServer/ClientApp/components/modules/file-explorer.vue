<template>
    <div>
        <v-container grid-list-xl v-show="uploadFiles.length > 0">
            <div class="text-xs-center">Files being uploaded...</div>
            <v-list>
                <template v-for="(file, i) in uploadFiles">
                    <v-list-item>
                        <v-list-item-content>
                            {{ file.name }}
                        </v-list-item-content>
                    </v-list-item>
                </template>
            </v-list>
        </v-container>

        <v-container mt-1 class="upload-area" v-if="role >= roleOptions.Elevated">
            <v-form enctype="multipart/form-data">
                <v-layout row wrap>
                    <v-flex xs12>
                        <label id="upload-button" for="upload-files" class="upload-button">UPLOAD FILES</label>
                    </v-flex>
                    <input ref="fUpload" type="file" name="upload-files" id="upload-files" class="file-upload" multiple @change="setFiles" hidden />
                </v-layout>
            </v-form>
        </v-container>

        <v-container>
            <v-form v-if="selectable">
                <v-layout row wrap px-2>
                    <v-flex xs12>
                        <v-select v-model="selectedDirectory" :items="folders" :label="`Select a folder`" item-text="name" item-value="name"></v-select>
                    </v-flex>
                </v-layout>
            </v-form>
            <v-card>
                <v-container>
                    <v-layout row wrap>
                        <v-flex xs1>
                            <v-btn icon @click="goBack"><fa-icon icon="arrow-left" ma-2 pa-2></fa-icon></v-btn>
                        </v-flex>
                        <v-flex xs11>
                            {{ fullPath }}
                        </v-flex>
                    </v-layout>
                </v-container>
            </v-card>

            <v-list-item v-for="item in contents" :key="item.id">
                <v-list-item-action>
                    <a :href="getDownloadPath(item)" download><fa-icon icon="download" /></a>
                </v-list-item-action>
                <v-list-item-avatar>
                    <fa-icon :icon="getIcon(item)" size="2x" style="color: gray;" />
                </v-list-item-avatar>
                <v-list-item-content @click="open(item)" :class="{ 'hide-extra': $vuetify.breakpoint.xsOnly ? true : false }">
                    <tooltip :value="item.title"></tooltip>
                </v-list-item-content>
                <v-list-item-action v-if="item.size > 0" class="hidden-xs-only">
                    {{ getFileSize(item.size) }}
                </v-list-item-action>
                <!-- Ensure admin and not a folder for delete -->
                <v-list-item-action v-if="role === roleOptions.Admin && !item.isFolder">
                    <v-btn icon @click="deleteItem(item)"><fa-icon size="lg" icon="window-close" /></v-btn>
                </v-list-item-action>
            </v-list-item>

            <v-layout row justify-center class="loading-container">
                <v-progress-circular v-show="loading" indeterminate color="purple" :width="12" :size="120"></v-progress-circular>
            </v-layout>
        </v-container>
    </div>
</template>

<script>
    import * as CONST from '../../constants'
    import service from '../../services/file-explorer'
    import { mapState } from 'vuex'
    import Tooltip from './tooltip'
    import Auth from '../../mixins/authentication'

    import { getSubdirectoryString, getSubdirectoryArray } from '../../helpers/browser'

    let path = process.env.API_URL;

    export default {
        data() {
            return {
                selectedDirectory: '',

                loading: false,
                changing: false,

                // Upload
                uploadFiles: [],

                role: CONST.Roles.Level.Default,
                roleOptions: CONST.Roles.Level,
            }
        },
        mixins: [Auth],
        components: {
            "tooltip": Tooltip,
        },
        props: {
            selectable: {
                type: Boolean,
                default: true,
            },
            parentView: {
                type: String,
                required: true,
            },
            goFile: {
                type: Object,
                default: () => {
                    file: null;
                    num: 0;
                }
            },
        },
        mounted() {
            this.loadFromPath();

            this.role = this.$_auth_convertRole(this.$store.state.auth.role);
        },
        computed: {
            ...mapState({
                contents: state => state.fileExplorer.contents,
                folders: state => state.fileExplorer.folders,
                directory: state => state.fileExplorer.directory,
                subDirectories: state => state.fileExplorer.subDirectories,
            }),
            fullPath() {
                if (typeof this.directory !== 'undefined' && this.directory !== null) {
                    if (Array.isArray(this.subDirectories) && this.subDirectories.length > 0) {
                        // TODO: Fix to build the list of subdirs
                        return `${this.directory}/${getSubdirectoryString(this.subDirectories)}`;
                    }
                    else {
                        return this.directory;
                    }
                }
                else {
                    return '';
                }
            }
        },
        watch: {
            // Local prop
            selectedDirectory: function (newValue) {
                if (!this.changing) {
                    this.$_console_log('[FileExplorer] Watcher - Selected directory: ', newValue);

                    this.$store.dispatch('changeDirectory', newValue);
                }
            },
            // Vuex prop
            directory: function (newValue) {
                if (!this.changing) {
                    this.$_console_log('[FileExplorer] Watcher - Vuex Directory:', newValue);

                    this.$store.dispatch('loadDirectory');
                    //this.setRoute(newValue, true);
                    //this.openDir();
                }
            },
            // Vuex prop
            subDirectories: {
                handler(newValue) {
                    if (!this.changing) {
                        this.$_console_log('[FileExplorer] Watcher - Vuex SubDirectories:', newValue);

                        this.$store.dispatch('loadDirectory');
                    }
                },
                deep: true
            },
            goFile: {
                handler(newValue) {
                    if (typeof newValue !== 'object' || newValue === null) {
                        this.$_console_log('[FileExplorer] goFile watcher: value is not an object or null');
                        this.goFile = { file: null, num: 0 }
                        return;
                    }

                    if (newValue.num === 0) {
                        this.$_console_log('[FileExplorer] goFile watcher: Num is 0, therefore we aren\t navigating to a new file');
                        return;
                    }

                    const folderlessList = this.contents.filter(x => x.isFolder === false).slice(0);
                    this.$_console_log('Folderless List:', folderlessList);
                    if (folderlessList.length === 0) {
                        this.$_console_log('[FileExplorer] goFile watcher: This folder only contains other folders. Nothing to load.');
                        return;
                    }

                    const fileIndex = folderlessList.findIndex(x => encodeURI(x.title) === newValue.file);
                    if (fileIndex === -1) {
                        this.$_console_log('[FileExplorer] goFile watcher: File not found in list');
                        return;
                    }

                    if ((fileIndex + newValue.num) >= folderlessList.length) {
                        this.$_console_log('[FileExplorer] goFile watcher: Navigating here will overflow the list.');
                        return;
                    }
                    else if ((fileIndex + newValue.num) < 0) {
                        this.$_console_log('[FileExplorer] goFile watcher: Navigating here will underflow the list.');
                        return;
                    }
                    else {
                        // Everything is good to go. Change files.
                        const fileToOpen = folderlessList[fileIndex + newValue.num];
                        this.open(fileToOpen);
                    }
                },
                deep: true
            }
        },
        methods: {
            async loadFromPath() {
                this.changing = true;
                let hasFolder = false;

                let getFolderPromise = this.$store.dispatch('getFolders');

                let route = this.$route;
                this.$_console_log(route);

                // If additional folder params are passed in, extract them so we can load it with the correct path
                if (typeof route.params.folder !== 'undefined') {
                    const dirArray = getSubdirectoryArray(route.params.folder);

                    if (dirArray.length === 1) {
                        hasFolder = true;
                        this.$store.dispatch('changeDirectory', dirArray[0]);
                    }
                    else if (dirArray.length > 1) {
                        hasFolder = true;
                        this.$store.dispatch('changeDirectory', dirArray[0]);

                        for (let i = 1; i < dirArray.length; i++) {
                            await this.$store.dispatch('goForwardDirectory', dirArray[i]);
                        }
                    }
                }

                // Use default folder if one exists
                if (hasFolder === false) {
                    await getFolderPromise;

                    let fol = this.folders.find(x => x.default === true);
                    this.$_console_log('[File Explorer] LoadFromPath: No folder present in path', fol);
                    if (typeof fol !== 'undefined') {
                        this.selectedDirectory = fol.name;
                        this.$store.dispatch('changeDirectory', fol.name);
                        //this.$store.dispatch('changeDirectory', fol.name)
                    }
                }

                setTimeout(() => {
                    this.changing = false;
                    this.$store.dispatch('loadDirectory');
                }, 250);
            },

            open(item) {
                if (item.isFolder) {
                    this.$store.dispatch('goForwardDirectory', item.title);
                }
                else {
                    this.$_console_log('[FILE EXPLORER] Open Item: item, dl path', item, this.getFilePath(item))
                    setTimeout(() => {
                        this.$emit('loadFile', this.getFilePath(item));
                    }, 125);
                }
            },
            goBack() {
                this.$store.dispatch('goBackDirectory');
            },



            getDownloadPath(item) {
                return `${path}/api/directory/download/file/${encodeURI(this.fullPath)}/${encodeURIComponent(item.title)}`;
                //return `${path}/api/directory/download?fileName=${encodeURI(item.title)}&folder=${encodeURI(fullFolderPath)}`;
            },
            getFilePath(item) {
                return `${encodeURI(this.fullPath)}/${encodeURIComponent(item.title)}`;
            },
            getIcon(item) {
                if (item.isFolder)
                    return 'folder';
                else
                    return 'file';
            },
            getFileSize(size) {
                let type = 0;

                while (size / 1024 > 1) {
                    size = size / 1024;
                    type++;
                }

                return `${size.toFixed(2)} ${this.getType(type)}`;
            },
            getType(type) {
                switch (type) {
                    case 0:
                        return 'B';
                    case 1:
                        return 'KB';
                    case 2:
                        return 'MB';
                    case 3:
                        return 'GB';
                    case 4:
                        return 'TB';
                }
            },


            // Upload
            setFiles(e) {
                if (this.role < CONST.Roles.Level.Elevated) {
                    this.$_console_log('Users without elevated or higher access cannot upload files');
                    return;
                }

                this.$_console_log(e.target.files);
                for (let i = 0; i < e.target.files.length; i++) {
                    this.uploadFiles.push(e.target.files[i]);
                }
                this.uploadMultipleFiles();
            },
            async uploadMultipleFiles() {
                for (let i = 0; i < this.uploadFiles.length; i++) {
                    await this.sendFile(this.uploadFiles[i]).then(resp => {
                        this.$_console_log("File sent!");
                    }).catch(() => {
                        this.$_console_log("Failed to upload file")
                    });
                }

                // Clean the list 
                this.uploadFiles = [];
                this.$refs.fUpload.value = '';
                this.$_console_log("Finished sending all files");

                // TODO: Repopulate the list with the new file
                //location.reload();
            },
            async sendFile(file) {
                let routeData = this.getPathFromRoute();
                this.$_console_log('[FILE EXPLORER] Sendfile then route data:', routeData);

                let formData = new FormData();
                formData.append("File", file);
                formData.append("Name", file.name);
                formData.append("Directory", routeData.dir);
                if (routeData.subDir !== null)
                    formData.append("SubDirectory", routeData.subDir);

                await service.uploadFile(formData).then(resp => {
                    this.$_console_log("Successfully uploaded file");

                    this.$store.dispatch('pushNotification', {
                        text: `Successfully uploaded file ${file.name}`,
                        type: 0
                    });
                }).catch(() => {
                    this.$_console_log("Error uploading files");

                    this.$store.dispatch('pushNotification', {
                        text: `Failed uploading file ${file.name}`,
                        type: 2
                    });
                });
            },
            async deleteItem(file) {
                if (this.role < CONST.Roles.Level.Admin)
                    return this.$_console_log("Non Admins are not allowed to delete files");                    

                if (typeof file === 'undefined' || file === null || file === '')
                    return this.$_console_log("Invalid file info, can't delete");

                this.$_console_log(`Delete: ${file.title}`);

                await service.deleteFile(file.title, this.directory, getSubdirectoryString(this.subDirectories)).then(resp => {
                    this.$_console_log('Successfully deleted the file');
                    this.$store.dispatch('deleteFile', file);
                }).catch(() => this.$_console_log('Error deleting the item in upload'));
            },
        }
    }
</script>

<style>
    .loading-container {
        position: fixed;
        left: 50%;
        top: 50%;
    }

    .upload-area:hover {
        border: 1px dashed #D8D8D8;
        background-color: #646464;
        color: #323232;
        font-weight: bold;
    }

    .upload-area {
        border-radius: 5px;
        background-color: #424242;        
    }

    .upload-button {
        display: block;
        text-align: center;
        line-height: 150%;
        font-size: .85em;
    }

    .small {
        height: 80px;
    }

    .hide-extra {
        white-space: nowrap;
    }
</style>

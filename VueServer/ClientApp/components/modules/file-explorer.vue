<template>
    <div>
        <v-snackbar v-model="dialog.on"
                    :bottom="dialog.y === 'bottom'"
                    :left="dialog.x === 'left'"
                    :color="dialog.type === 'success' ? 'success' : dialog.type === 'error' ? 'red' : 'info'"
                    :multi-line="dialog.mode === 'multi-line'"
                    :right="dialog.x === 'right'"
                    :timeout="dialog.timeout"
                    :top="dialog.y === 'top'"
                    :vertical="dialog.mode === 'vertical'">
            <div v-if="dialog.type === 'success'">
                <fa-icon icon="check"></fa-icon>
            </div>
            <p class="black--text text--lighten-2">{{ dialog.message }}</p>
            <v-btn color="pink"
                   text
                   @click="dialog.on = false">
                Close
            </v-btn>
        </v-snackbar>

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

        <v-container mt-1 class="upload-area">
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
                <v-layout row wrap>
                    <v-flex xs12>
                        <v-select v-model="selectedFolder" :items="folders" :label="`Select a folder`" item-text="name" item-value="name"></v-select>
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

            <v-list-item v-for="item in dirContents" :key="item.id">
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
                <v-list-item-action v-if="isAdmin && !item.isFolder">
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
    import { mapGetters } from 'vuex'
    import Tooltip from './tooltip'

    let path = process.env.API_URL;

    export default {
        data() {
            return {
                selectedFolder: null,
                selectedSubDirs: null,

                childDirNames: [],
                mainDirContents: [],
                dirContents: [],

                loading: false,
                changing: false,

                defaultFolder: '',

                // Upload
                uploadFiles: [],

                isAdmin: false,

                dialog: {
                    on: false,
                    x: null,
                    y: 'top',
                    message: '',
                    mode: 'multi-line',
                    type: 'success',
                    timeout: 5000
                },
            }
        },
        components: {
            "tooltip": Tooltip,
        },
        props: {
            folders: {
                type: Array,
                required: true,
            },
            selectable: {
                type: Boolean,
                default: true,
            },
            parentView: {
                type: String,
                required: true,
            }
        },
        mounted() {
            this.changing = true;
            this.setSelectedObjects(this.getPathFromRoute());

            setTimeout(() => {
                let defaultFol = this.folders.find(x => x.default === true);
                if (typeof defaultFol === 'object')
                    this.defaultFolder = defaultFol.name;

                this.setRoute();
                this.openDir();

                this.changing = false;
            }, 250);
            this.isAdmin = this.$store.state.auth.role === 'Administrator';
        },
        computed: {
            fullPath() {
                if (typeof this.selectedFolder !== 'undefined' && this.selectedFolder !== null) {
                    if (this.selectedSubDirs !== null) {
                        return `${this.selectedFolder}/${this.selectedSubDirs}`;
                    }
                    else {
                        return this.selectedFolder;
                    }
                }
                else {
                    return '';
                }
            }
        },
        watch: {
            selectedFolder: function (newValue, oldValue) {
                if (!this.changing && newValue !== oldValue) {
                    this.$_console_log('[FileExplorer] Watcher - selectedFolder: Setting child dir names to empty array', newValue, oldValue);
                    this.setRoute(newValue, true);
                    this.openDir();
                }
            }
        },
        methods: {
            setSelectedObjects(obj) {
                if (obj.code > 0) {
                    this.selectedFolder = obj.dir;

                    if (obj.subDir !== null) {
                        this.selectedSubDirs = obj.subDir;
                    }
                    else {
                        this.selectedSubDirs = null;
                    }
                }
                else {
                    this.selectedFolder = null;
                    this.selectedSubDirs = null;
                }
            },
            setRoute(value, override) {
                this.changing = true;
                //this.$_console_log('[FileExplorer] SetRoute: Route', this.$route);
                let routeObj = this.getPathFromRoute();
                let routePath = this.$route.fullPath;

                this.$_console_log('[SETROUTE] Route path then Route obj:', routePath, routeObj, value);
                //if (routeObj.code === 0)
                //    return this.openDir();

                if (value) {
                    this.$_console_log('[SETROUTE] We passed a value!', value);
                    // Change base dir 
                    if (override) {
                        this.$router.push(`${routeObj.basePath}/${value}`);
                    }
                    // Go Back
                    else if (value === '../') {
                        if (routeObj.code === 0 || routeObj.dir === null)
                            return; // Can't go back from nothing
                        else if (routeObj.subDir === null)
                            this.$router.push(`${routeObj.basePath}/${routeObj.dir}`);  // Go back to base path
                        else {
                            let index = routeObj.subDir.lastIndexOf('/');
                            let newSub = routeObj.subDir.substring(0, index);
                            this.$router.push(`${routeObj.basePath}/${routeObj.dir}/${newSub}`);
                        }
                            
                    }
                    // Go forward
                    else {
                        if (routeObj.dir === null)
                            this.$router.push(`${routeObj.basePath}/${value}`);
                        else if (routeObj.subDir === null)
                            this.$router.push(`${routeObj.basePath}/${routeObj.dir}/${value}`);
                        else
                            this.$router.push(`${routeObj.basePath}/${routeObj.dir}/${routeObj.subDir}/${value}`);
                    }
                }
                else {
                    // Default folder logic
                    if (routeObj.code === 0) {
                        let subRoute = `${routeObj.basePath}/${this.defaultFolder}`;
                        this.$_console_log('[SETROUTE] Route / Sub Route', routePath, subRoute);
                        if (routePath !== subRoute) {
                            this.selectedFolder = this.defaultFolder;
                            this.$router.push(`${routeObj.basePath}/${this.defaultFolder}`);
                        }
                    }
                }

                this.changing = false;
            },
            openDir() {
                let pathObj = this.getPathFromRoute();
                if (pathObj.code === -1)
                    return false;
                else if (pathObj.code === 0) {
                    this.setRoute();
                    setTimeout(() => { this.changing = false }, 25);
                    //return;
                }
                else {
                    this.setRoute();
                    setTimeout(() => { this.changing = false }, 25);
                }

                //service.loadDirectory(dir, subDir).then(resp => {
                service.loadDirectory(pathObj.dir, pathObj.subDir).then(resp => {
                    this.$_console_log('Service load dir: ', resp);
                    this.dirContents = resp.data;
                }).catch(() => {
                    // TODO: Determine how to handle this properly
                    this.$_console_log('[FileExplorer] OpenDir: Error getting the directory');
                    this.goBack();
                }).then(() => {
                    this.loading = false;
                    this.changing = false;
                });
            },
            getPathFromRoute() {
                // Regex path matching. Setup to /home/[parentView]/[someword&numbers&-_]*
                // Ex: /home/browser/meMe-town_Land
                let regexPath = new RegExp(`(\/home\/${this.parentView}(\/([a-z0-9-_'" ]*))*)`, 'gi');
                if (this.$route.fullPath.match(regexPath)) {
                    this.$_console_log('Matched');

                    let ss = `/home/${this.parentView}`;
                    let route = this.$route.fullPath.substring(ss.length);
                    let basePath = this.$route.fullPath.substring(0, ss.length);

                    this.$_console_log('[GETPATHFROMROUTE] basepath + route', basePath, route, ss);

                    if (route.length > 0) {
                        // Clean first /
                        if (route[0] === '/') {
                            route = route.substring(1);
                            this.$_console_log(`[GETPATHFROMROUTE] Clean route dir`, route);
                        }

                        // Path was /home/parentView/ treat this same as else statement of parent if statement
                        if (route === '') {
                            this.$_console_log(`[GETPATHFROMROUTE] Route is '' exiting with code 0`, route);
                            return { basePath: basePath, path: null, subDir: null, code: 0 };
                        }

                        // Directory part
                        let dir = '';
                        let subDir = '';
                        if (route.includes('/')) {
                            dir = route.substring(0, route.indexOf('/'));
                            route = route.substring(dir.length);
                            this.$_console_log(`[GETPATHFROMROUTE] dir`, dir, route);

                            // Clean first /
                            if (route[0] === '/') {
                                route = route.substring(1);
                                this.$_console_log(`[GETPATHFROMROUTE] Clean route subDir`, route);
                            }

                            // Subdirectory part
                            if (route.length > 0) {
                                subDir = route;                                
                                this.$_console_log(`[GETPATHFROMROUTE] subDir`, route);
                            }
                        }
                        else {
                            dir = route.substring(0);
                            route = route.substring(dir.length);
                            this.$_console_log(`[GETPATHFROMROUTE] dir`, route);
                        }

                        return {
                            basePath: basePath,
                            dir: dir,
                            subDir: subDir === '' ? null : subDir,
                            code: subDir === '' ? 2 : 1 };
                    }
                    else {
                        return { basePath: basePath, code: 0 };
                    }
                }
                else {
                    this.$_console_log(`[FileExplorer] SetRoute: Route doesn't match /home/${this.parentView}[/String]*', can't load directory.`);
                    return { code: -1 };
                }
            },
            open(item) {
                if (item.isFolder) {
                    this.setRoute(item.title);
                }
                else {
                    //this.$emit('loadFile', `${path}/home/${this.parentView}/${this.fullPath}/${item.title}`);
                    this.$_console_log('[FILE EXPLORER] Open Item: item, dl path', item, this.getMediaPath(item))
                    setTimeout(() => {
                        this.$emit('loadFile', this.getMediaPath(item));
                    }, 125);
                }
            },
            goBack() {
                this.setRoute('../');
            },
            getDownloadPath(item) {
                return `${path}/api/directory/download/file/${encodeURI(this.fullPath)}/${encodeURIComponent(item.title)}`;
                //return `${path}/api/directory/download?fileName=${encodeURI(item.title)}&folder=${encodeURI(fullFolderPath)}`;
            },
            getMediaPath(item) {
                return `${path}/api/serve-file/${encodeURI(this.fullPath)}/${encodeURIComponent(item.title)}`;
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

                location.reload();
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
                    //let fIndex = this.folderFiles.findIndex(x => x.folder === this.selectedFolder);
                    //if (fIndex !== -1) {
                    //    this.folderFiles[fIndex].files.push(file.name);

                    //    this.dialog.on = true;
                    //    this.dialog.message = `Successfully uploaded file ${file.name}`;
                    //    this.dialog.type = 'success';
                    //    this.dialog.timeout = 5000;
                    //}
                }).catch(() => {
                    this.$_console_log("Error uploading files");

                    this.dialog.on = true;
                    this.dialog.message = `Failed uploading file ${file.name}`;
                    this.dialog.type = 'error';
                    this.dialog.timeout = 15000;
                });
            },
            async deleteItem(file) {
                if (!this.isAdmin)
                    return this.$_console_log("You're not allowed to do that sir...");

                if (typeof file === 'undefined' || file === null || file === '')
                    return this.$_console_log("Invalid file info, can't delete");

                this.$_console_log(`Delete: ${file.title}`);
                let routeData = this.getPathFromRoute();

                await service.deleteFile(file.title, routeData.dir, routeData.subDir === null ? '' : routeData.subDir).then(resp => {
                    this.$_console_log('Successfully deleted the file');
                    let index = this.dirContents.findIndex(x => x.title === file.title);
                    if (index > -1) {
                        this.dirContents.splice(index, 1);
                    }
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

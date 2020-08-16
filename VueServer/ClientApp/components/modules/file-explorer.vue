<template>
    <div>
        <file-upload v-if="features.upload === true"></file-upload>

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
                <v-list-item-action v-if="features.delete">
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
    import service from '../../services/file-explorer'
    import { mapState } from 'vuex'

    import Tooltip from './tooltip'
    import FileUpload from './file-upload'

    import Auth from '../../mixins/authentication'


    import { getSubdirectoryString, getSubdirectoryArray, splitPathFromRoute } from '../../helpers/browser'

    let path = process.env.API_URL;

    export default {
        data() {
            return {
                selectedDirectory: '',

                loading: false,
                changing: false,
                features: {
                    upload: false,
                    delete: false,
                    viewing: false,
                }                
            }
        },
        mixins: [Auth],
        components: {
            "tooltip": Tooltip,
            "file-upload": FileUpload,
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
        async mounted() {
            if (!Array.isArray(this.folders) || (Array.isArray(this.folders) && this.folders.length === 0))
                await this.$store.dispatch('getFolders');

            this.setActiveFeatures();

            this.readRoute();
            this.loadFromPath();

            this.role = this.$_auth_convertRole(this.$store.state.auth.role);
        },
        computed: {
            ...mapState({
                contents: state => state.fileExplorer.contents,
                folders: state => state.fileExplorer.folders,
                directory: state => state.fileExplorer.directory,
                subDirectories: state => state.fileExplorer.subDirectories,
                activeModules: state => state.auth.activeModules,
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
            },
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

                    this.$router.push({ name: 'browser-folder', params: { folder: newValue } })
                }
            },
            // Vuex prop
            subDirectories: {
                handler(newValue) {
                    if (!this.changing) {
                        this.$_console_log('[FileExplorer] Watcher - Vuex SubDirectories:', newValue);

                        let baseDir = this.directory;
                        let theCharIs = baseDir.charAt(baseDir.length - 1);

                        this.$_console_log('BaseDir and CharAt:', baseDir, theCharIs);

                        if (theCharIs === '/')
                            baseDir = baseDir.substr(0, baseDir.length - 1);

                        this.$router.push({ name: 'browser-folder', params: { folder: `${baseDir}/${getSubdirectoryString(newValue)}` } })
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
            setActiveFeatures() {
                const browserObj = this.activeModules.find(x => x.id === 'browser');
                if (typeof browserObj === 'undefined' || !Array.isArray(browserObj.userModuleFeatures))
                    return;

                if (browserObj.userModuleFeatures.some(x => x.moduleFeatureId === 'browser-upload'))
                    this.features.upload = true;

                if (browserObj.userModuleFeatures.some(x => x.moduleFeatureId === 'browser-delete'))
                    this.features.delete = true;

                if (browserObj.userModuleFeatures.some(x => x.moduleFeatureId === 'browser-viewer'))
                    this.features.viewing = true;
            },
            readRoute() {
                this.changing = true;
                if (typeof this.directory === 'undefined' || this.directory === null || this.directory === '') {
                    const route = this.$route;
                    this.$_console_log('Route: ', route);
                    if (typeof route.params.folder !== 'undefined') {
                        const splitPath = splitPathFromRoute(route.params.folder);
                        this.selectedDirectory = splitPath.base;
                        this.$store.dispatch('changeDirectory', this.selectedDirectory);

                        const subDirArray = getSubdirectoryArray(splitPath.subDirs);
                        this.$_console_log('Folder and subdir array:', splitPath, subDirArray);
                        if (subDirArray.length > 0) {
                            for (let i = 0; i < subDirArray.length; i++) {
                                this.$store.dispatch('goForwardDirectory', subDirArray[i]);
                            }
                        }
                    }
                    else {
                        // Load default folder
                        const defaultFolder = this.folders.find(x => x.default === true);
                        if (typeof defaultFolder !== 'undefined') {
                            this.selectedDirectory = defaultFolder.name;
                            this.$store.dispatch('changeDirectory', this.selectedDirectory);
                        }
                    }
                }
                else {
                    this.selectedDirectory = this.directory;
                }

                setTimeout(() => {
                    this.changing = false;
                }, 5);
            },
            async loadFromPath() {
                setTimeout(() => {
                    this.$store.dispatch('loadDirectory');
                    this.changing = false;                    
                }, 150);
            },

            open(item) {
                if (item.isFolder) {
                    this.$store.dispatch('goForwardDirectory', item.title);
                }
                else {
                    if (this.features.viewing) {
                        this.$_console_log('[FILE EXPLORER] Open Item: item, dl path', item, this.getFilePath(item))
                        setTimeout(() => {
                            this.$emit('loadFile', this.getFilePath(item));
                        }, 125);
                    }                    
                }
            },
            goBack() {
                this.$store.dispatch('goBackDirectory');
            },



            getDownloadPath(item) {
                return `${path}/api/directory/download/file/${encodeURI(this.fullPath)}/${encodeURIComponent(item.title)}?token=${this.$store.state.auth.accessToken}`;
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

            async deleteItem(file) {
                if (this.deleteEnabled === false) {
                    return this.$_console_log("Non Admins are not allowed to delete files");
                }

                if (typeof file === 'undefined' || file === null || file === '')
                    return this.$_console_log("Invalid file info, can't delete");

                this.$_console_log(`Delete: ${file.title}`);

                await service.deleteFile(file.title, this.directory, getSubdirectoryString(this.subDirectories)).then(resp => {
                    this.$_console_log('Successfully deleted the file');
                    this.$store.dispatch('deleteFile', file.title);
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

    .hide-extra {
        white-space: nowrap;
    }
</style>

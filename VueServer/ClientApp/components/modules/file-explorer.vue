<template>
    <div>
        <v-container>
            <v-form v-if="selectable">
                <v-layout row wrap>
                    <v-flex xs12>
                        <v-select v-model="selectedFolder" :items="folders" @change="loadDirectory" v-bind:label="`Select a folder`"></v-select>
                    </v-flex>
                </v-layout>
            </v-form>
            <v-card>
                <v-layout row wrap>
                    <v-container fluid>
                        <v-flex xs12>
                            <v-btn icon flat @click="goBack"><v-icon v-if="selectedFolder" class="mr-3">fas fa-arrow-left</v-icon></v-btn>
                            {{ getFullPath }}
                        </v-flex>
                    </v-container>
                </v-layout>
            </v-card>

            <v-list-tile v-for="item in dirContents" :key="item.id">
                <v-list-tile-action>
                    <a :href="getDownloadPath(item)" download><fa-icon icon="download" /></a>
                </v-list-tile-action>
                <v-list-tile-avatar>
                    <fa-icon :icon="getIcon(item)" size="2x" style="color: gray;" />
                </v-list-tile-avatar>
                <v-list-tile-content @click="openFolder(item)" :class="{ 'hide-extra': $vuetify.breakpoint.xsOnly ? true : false }">
                    <tooltip :value="item.title"></tooltip>
                </v-list-tile-content>
                <v-list-tile-action v-if="item.size > 0" class="hidden-xs-only">
                    {{ getFileSize(item.size) }}
                </v-list-tile-action>
            </v-list-tile>

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
                selectedFolder: '',
                childDirNames: [],
                mainDirContents: [],
                dirContents: [],

                loading: false,
                changing: false,
            }
        },
        components: {
            "tooltip": Tooltip,
        },
        props: {
            level: {
                type: Number,
                default: 0,
            },
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
        created() {
            //this.loadDirectory();
            this.openDir();
        },
        computed: {
            ...mapGetters({
                myState: 'getAllState'
            }),
            getFullPath() {
                let index = this.folders.findIndex(x => x.value === this.selectedFolder);
                this.$_console_log(`[FileExplorer] GetFullPath: ${index}`);
                if (index === -1)
                    return '';
                let path = this.folders[index].text;
                if (this.level === CONST.Roles.Level.Admin) {
                    //path += ':';
                    if (this.childDirNames.length === 0)
                        path += '\\';
                }

                for (let i = 0; i < this.childDirNames.length; i++) {
                    path += `\\${this.childDirNames[i]}`;
                }
                return path;
            },
        },
        watch: {
            '$route': {
                deep: true,
                handler: function (refreshPage) {
                    this.$_console_log('[FileExplorer] Router Watcher: Reloading page');
                    //this.checkFolder();
                    this.openDir();
                    //this.loadDirectory();
                }
            },
            selectedFolder: function () {
                if (!this.changing) {
                    this.$_console_log('[FileExplorer] SelectedFolder Watcher: Setting child dir names to empty array');
                    this.childDirNames = [];
                }
            }
        },
        methods: {
            setSelectedFolder(folder) {
                this.selectedFolder = folder;
            },
            getSubDirString() {
                //this.$_console_log('[FileExplorer] GetSubDirString: ');
                let path = '';
                if (this.childDirNames) {
                    if (this.childDirNames.length > 0) {
                        for (let i = 0; i < this.childDirNames.length; i++) {
                            path += this.childDirNames[i];
                            if (i < this.childDirNames.length - 1)
                                //path += '/';
                                path += '\\';
                        }
                        return path;
                    }
                }
                return '';
            },
            loadDirectory() {
                this.$_console_log('[FileExplorer] LoadDirectory: Route', this.$route);
                this.checkFolder();

                let path = '';
                let route = this.$route.fullPath;

                if (route.match(/(\/home\/([a-zA-Z-])*\/[0-9]\/[a-zA-Z0-9-_]*)/)) {
                    path = this.selectedFolder;
                }
                else {
                    path = `${route}/${this.level}/${this.selectedFolder}`;
                }

                if (this.childDirNames.length > 0) {
                    this.$_console_log('[FileExplorer] LoadDirectory: Child Dir names > 0');
                    this.$router.push(
                        {
                            path: path,
                            query: { dir: this.getSubDirString() }
                        }
                    );
                }
                else {
                    this.$router.push(path);
                }

                //this.openDir();
            },
            openDir() {
                if (this.checkPath() === false)
                    return false;
                else {
                    this.loadDirectory();
                    setTimeout(() => { this.changing = false }, 25);
                }
                    

                let dir = this.selectedFolder;
                let subDir = this.getSubDirString();

                this.$_console_log(`[FileExplorer] OpenDir: Loading beginning: dir=${dir} subDir=${subDir}`);
                this.loading = true;

                if (subDir === '') {
                    this.$_console_log("[FileExplorer] OpenDir: Sub dir is null");
                    this.dirContents = [];
                }

                service.loadDirectory(dir, subDir, this.level).then(resp => {
                    this.dirContents = resp.data;
                }).catch(() => {
                    // TODO: Determine how to handle this properly
                    this.$_console_log('[FileExplorer] OpenDir: Error getting the directory');
                    if (this.childDirNames.length > 0)
                        this.childDirNames.pop();
                }).then(() => {
                    this.loading = false;
                    this.changing = false;
                });
            },
            checkPath() {
                // Regex path matching. Setup to /home/[someword(with dashes)]/[0-9]/[someword&numbers&-_]
                // Ex: /home/Downloads/3/meMe-town_Land
                if (this.$route.fullPath.match(/(\/home\/([a-zA-Z-])*\/[0-9]\/[a-zA-Z0-9-_]*)/)) {
                    this.$_console_log('Matched');
                    this.childDirNames = [];
                    let route = this.$route.fullPath.slice();
                    while (route.includes('/')) {
                        route = route.slice(route.indexOf('/') + 1);
                    }
                    if (route.includes('?')) {
                        this.changing = true;
                        this.$_console_log('? detected');
                        
                        this.selectedFolder = route.slice(0, route.indexOf('?'));

                        let query = route.slice(route.indexOf('?'));
                        this.$_console_log(query);
                        if (query.includes('=')) {
                            this.$_console_log('= Detected');
                            let subDir = query.slice(query.indexOf('=') + 1);
                            subDir = decodeURIComponent(subDir);
                            if (subDir.includes('\\')) {
                                this.$_console_log('\\ Detected');
                                while (subDir.includes('\\')) {
                                    let name = subDir.slice(0, subDir.indexOf('\\'));
                                    this.childDirNames.push(name);
                                    subDir = subDir.slice(subDir.indexOf('\\') + 1)
                                }
                            }

                            this.childDirNames.push(subDir);
                        }
                    }
                    else {
                        this.selectedFolder = route;
                    }
                }
                else {
                    this.$_console_log('[FileExplorer] LoadDirectory: Route doesn\'t match \'/home/String/Number/String\', can\'t load directory.');
                    return false;
                }
            },
            checkFolder() {
                if (typeof this.selectedFolder === 'undefined' || this.selectedFolder == null || this.selectedFolder == '') {
                    this.$_console_log('[FileExplorer] LoadDirectory: Selected Folder isn\'t selected...');
                    this.checkPath();
                }
            },
            openFolder(item) {
                if (item.isFolder) {
                    this.childDirNames.push(item.title);
                    this.loadDirectory();
                    //this.openDir();
                }
            },
            goBack() {
                if (this.childDirNames.length === 0)
                    return this.$_console_log('[FileExplorer] GoBack: Nowhere to go back to!');
                this.$_console_log('[FileExplorer] GoBack: Going back');
                this.childDirNames.pop();
                this.$_console_log(this.childDirNames);

                this.loadDirectory();
            },
            getDownloadPath(item) {
                let fullFolderPath = this.getFullFolderPath();
                return `${path}/download/file/${(this.parentView === 'browser') ? CONST.Roles.Level.None : CONST.Roles.Level.General}/${encodeURI(fullFolderPath)}/${encodeURIComponent(item.title)}`;
                //return `${path}/api/directory/download?fileName=${encodeURI(item.title)}&folder=${encodeURI(fullFolderPath)}`;
            },
            getIcon(item) {
                if (item.isFolder)
                    return 'folder';
                else
                    return 'file';
            },
            getFullFolderPath() {
                let fullFolderPath = `${this.selectedFolder}`;
                let subDir = `${this.getSubDirString()}`;
                if (subDir !== '')
                    fullFolderPath += (`/${subDir}`);
                //this.$_console_log(`[FileExplorer] GetFullFolderPath: Full folder path: ${fullFolderPath}`);

                return fullFolderPath;
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
        }
    }
</script>

<style>
    .loading-container {
        position: fixed;
        left: 50%;
        top: 50%;
    }

    .small {
        height: 80px;
    }

    .hide-extra {
        white-space: nowrap;
    }
</style>

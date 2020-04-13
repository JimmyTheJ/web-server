<template>
    <v-container>
        <v-card>
            <v-card-title>
                Manage User's modules
            </v-card-title>
            <v-card-text>
                <v-layout row>
                    <v-flex xs12 sm6>
                        <v-list-item-group v-model="selectedUserPosition">
                            <v-list-item v-for="(item, index) in userList" :key="index">
                                <v-list-item-content>
                                    {{ item.id }}
                                </v-list-item-content>
                            </v-list-item>
                        </v-list-item-group>
                    </v-flex>
                    <v-flex xs12 sm6>
                        <v-list>
                            <template v-for="(item, index) in moduleList">
                                <v-list-item :key="index" @click="addModuleToUser(item)">
                                    <v-list-item-icon>
                                        <fa-icon icon="check" v-if="userHasModule(item)"></fa-icon>
                                        <fa-icon icon="times" v-else></fa-icon>
                                    </v-list-item-icon>
                                    <v-list-item-content>
                                        {{ item.name }}
                                    </v-list-item-content>
                                </v-list-item>
                            </template>
                        </v-list>
                    </v-flex>
                </v-layout>
            </v-card-text>
        </v-card>
    </v-container>
</template>

<script>
    import authService from '../../services/auth'
    import moduleService from '../../services/modules'
import modules from '../../services/modules';

    export default {
        data() {
            return {
                selectedUser: null,
                selectedUserPosition: null,
                moduleList: [],
                userList: [],
                usersHaveModuleList: [],
                selectedUserModuleList: [],
            }
        },
        mounted() {
            this.getData();
        },
        watch: {
            selectedUser(newValue) {
                this.updateSelectedUserList(newValue);
            },
            selectedUserPosition(newValue) {
                if (typeof newValue === 'undefined' || newValue === null) {
                    this.selectedUser = null;
                    return;
                }

                this.selectedUser = this.userList[newValue];
            },
        },
        methods: {
            getData() {
                authService.getUsers().then(resp => {
                    this.$_console_log('[admin-tools] getData: Successfully got user list');
                    this.userList = resp.data;
                }).catch(() => this.$_console_log('[admin-tools] getData: Failed to get user list'));

                moduleService.getAllModules().then(resp => {
                    this.$_console_log('[admin-tools] getData: Successfully got module list');
                    this.moduleList = resp.data;
                }).catch(() => this.$_console_log('[admin-tools] getData: Failed to get module list'));

                moduleService.getAllModulesForAllUser().then(resp => {
                    this.$_console_log('[admin-tools] getData: Successfully got user has module list');
                    this.usersHaveModuleList = resp.data;
                }).catch(() => this.$_console_log('[admin-tools] getData: Failed to get module lists for users'));
            },
            deleteModuleFromUser(module) {
                if (typeof this.selectedUser === 'undefined' || this.selectedUser === null) {
                    this.$_console_log('[admin-tools] userHasModule: selected user is null');
                    return false;
                }

                if (typeof module === 'undefined' || module === null) {
                    this.$_console_log('[admin-tools] userHasModule: module is null');
                    return false;
                }

                let obj = {
                    userId: this.selectedUser.id,
                    moduleAddOnId: module.id
                }

                moduleService.deleteModuleFromUser(obj).then(() => {
                    this.$_console_log('[admin-tools] deleteModuleFromUser: Successfully deleted module from user');
                    const index = this.usersHaveModuleList.findIndex(x => x.userId === obj.userId && x.moduleAddOnId === obj.moduleAddOnId);
                    if (index < 0) {
                        // Failed somehow...
                    }
                    else {
                        this.usersHaveModuleList.splice(index, 1);
                        this.updateSelectedUserList(this.selectedUser);
                    }                    
                }).catch(() => this.$_console_log('[admin-tools] deleteModuleFromUser: Failed to delete module from user'));
            },
            addModuleToUser(module) {
                if (this.userHasModule(module)) {
                    this.deleteModuleFromUser(module);
                    return;
                }

                let obj = {
                    userId: this.selectedUser.id,
                    moduleAddOnId: module.id
                }

                moduleService.addModuleToUser(obj).then(resp => {
                    this.$_console_log('[admin-tools] addModuleToUser: Successfully added module to user');
                    this.usersHaveModuleList.push(obj);
                    this.updateSelectedUserList(this.selectedUser);
                }).catch(() => this.$_console_log('[admin-tools] addModuleToUser: Failed to get add module to user'));
            },
            userHasModule(module) {
                if (typeof this.selectedUser === 'undefined' || this.selectedUser === null) {
                    this.$_console_log('[admin-tools] userHasModule: selected user is null');
                    return false;
                }

                if (typeof module === 'undefined' || module === null) {
                    this.$_console_log('[admin-tools] userHasModule: module is null');
                    return false;
                }

                let index = this.selectedUserModuleList.findIndex(x => x.userId === this.selectedUser.id && x.moduleAddOnId === module.id);
                if (index < 0) {
                    return false;
                }

                return true;             
            },
            updateSelectedUserList(value) {
                this.$_console_log('Update Selected User List');
                if (typeof value === 'undefined' || value === null) {
                    this.selectedUserModuleList = [];
                    return;
                }

                this.selectedUserModuleList = this.usersHaveModuleList.filter(x => x.userId == value.id);
            }
        }
    }
</script>

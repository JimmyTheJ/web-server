<template>
    <v-dialog v-model="dialogOpen" :max-width="maxModalWidth">
        <v-card>
            <v-card-title>
                <span class="headline">Author editor</span>
            </v-card-title>

            <v-card-text>
                <v-layout row>
                    <v-flex xs12 sm6>
                        <v-list-item-group v-model="selectedAuthor">
                            <v-list-item v-for="(item, index) in authorList" :key="index">
                                <v-list-item-content>
                                    {{ item.fullName }}
                                </v-list-item-content>
                            </v-list-item>
                        </v-list-item-group>
                    </v-flex>
                    <v-flex xs12 sm6>
                        <template v-if="typeof activeAuthor !== 'undefined' && activeAuthor !== null">
                            <v-flex xs12 pl-2>
                                <v-text-field v-model="activeAuthor.firstName" label="First Name"></v-text-field>
                            </v-flex>
                            <v-flex xs12 pl-2>
                                <v-text-field v-model="activeAuthor.lastName" label="Last Name"></v-text-field>
                            </v-flex>
                            <v-flex xs12 pl-1>
                                <v-checkbox v-model="activeAuthor.deceased" label="Deceased"></v-checkbox>
                            </v-flex>
                            <v-flex xs12>
                                <v-btn @click="selectedAuthor = null">
                                    CANCEL
                                </v-btn>
                                <v-btn @click="saveChanges()">
                                    SAVE
                                </v-btn>
                            </v-flex>
                        </template>
                    </v-flex>
                </v-layout>
            </v-card-text>
        </v-card>
    </v-dialog>
 </template>

    <script>
        import { mapState } from 'vuex'

        export default {
            data() {
                return {
                    dialogOpen: false,
                    selectedAuthor: null,
                    activeAuthor: null,
                }
            },
            props: {
                open: {
                    type: Boolean,
                    default: false,
                },
            },
            computed: {
                ...mapState({
                    authorList: state => state.library.authors,
                    bookList: state => state.library.books,
                }),
                maxModalWidth() {
                    if (window.innerWidth > 1640)
                        return 1500;
                    else if (window.innerWidth > 1360)
                        return 1200;
                    else if (window.innerWidth > 960)
                        return 800;
                    else if (window.innerWidth > 720)
                        return 600;
                    else if (window.innerWidth > 600)
                        return 500;
                    else
                        return 420;
                },
            },
            watch: {
                open: function (newValue) {
                    this.$_console_log(`[Author Dialog] open = ${newValue}`);
                    this.dialogOpen = newValue;
                },
                dialogOpen: function (newValue) {
                    if (newValue === false) {
                        this.$_console_log('[Author Dialog] Dialog open = false');
                        this.$emit('closeDialog', false);
                    }
                    else {
                        this.$_console_log('[Author Dialog] Dialog open = true');
                    }
                },
                selectedAuthor: function (newValue) {
                    this.$_console_log('Selected author watcher', newValue);
                    if (newValue === null || typeof newValue !== 'number') {
                        this.selectedAuthor = null;
                        this.activeAuthor = null;
                    }

                    this.activeAuthor = Object.assign({}, this.authorList[newValue]);
                }
            },
            methods: {
                saveChanges() {
                    this.$store.dispatch('editAuthor', this.activeAuthor);
                }
            }
        }
    </script>

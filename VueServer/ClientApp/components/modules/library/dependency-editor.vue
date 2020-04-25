<template>
    <v-dialog v-model="dialogOpen" :max-width="maxModalWidth">
        <v-card>
            <v-card-title>
                <span class="headline center">Book dependency editor</span>
            </v-card-title>

            <v-card-text>
                <v-layout row>
                    <v-flex xs12 class="align-center">
                        <v-btn class="center" @click="addObject()">
                            <fa-icon icon="plus" />Add new Object<fa-icon icon="plus" />
                        </v-btn>
                    </v-flex>

                    <v-flex xs12>
                        <v-tabs v-model="activeType" fixed-tabs hide-slider>
                            <v-tab v-for="(item, index) in typeList">
                                {{ item }}
                            </v-tab>
                        </v-tabs>
                    </v-flex>

                    <v-flex xs12 sm6>
                        <v-list-item-group v-model="activeObject">
                            <v-list-item v-for="(item, index) in activeList" :key="index">
                                <v-list-item-content v-if="activeType === typeConsts.Author">
                                    {{ item.fullName }}
                                </v-list-item-content>
                                <v-list-item-content v-else-if="activeType === typeConsts.Bookcase">
                                    {{ item.name }}
                                </v-list-item-content>
                                <v-list-item-content v-else-if="activeType === typeConsts.Series">
                                    {{ item.name }}
                                </v-list-item-content>
                                <v-list-item-content v-else-if="activeType === typeConsts.Shelf">
                                    {{ item.name }}
                                </v-list-item-content>
                            </v-list-item>
                        </v-list-item-group>
                    </v-flex>
                    <v-flex xs12 sm6>
                        <template v-if="typeof activeObject !== 'undefined' && activeObject !== null && selectedObject !== null">
                            <!-- Author -->
                            <v-form v-model="validated" ref="objectForm" lazy-validation>
                                <template v-if="activeType === typeConsts.Author">
                                    <v-flex xs12 pl-2>
                                        <v-text-field v-model="selectedObject.firstName" label="First Name"></v-text-field>
                                    </v-flex>
                                    <v-flex xs12 pl-2>
                                        <v-text-field v-model="selectedObject.lastName" label="Last Name" :rules="[rules.required]"></v-text-field>
                                    </v-flex>
                                    <v-flex xs12 pl-1>
                                        <v-checkbox v-model="selectedObject.deceased" label="Deceased"></v-checkbox>
                                    </v-flex>
                                </template>

                                <!-- Bookcase -->
                                <template v-else-if="activeType === typeConsts.Bookcase">
                                    <v-flex xs12 pl-2>
                                        <v-text-field v-model="selectedObject.name" label="Name" :rules="[rules.required]"></v-text-field>
                                    </v-flex>
                                </template>

                                <!-- Series -->
                                <template v-else-if="activeType === typeConsts.Series">
                                    <v-flex xs12 pl-2>
                                        <v-text-field v-model="selectedObject.name" label="Name" :rules="[rules.required]"></v-text-field>
                                    </v-flex>
                                    <v-flex xs12 pl-2>
                                        <v-text-field v-model="selectedObject.number" label="Number"></v-text-field>
                                    </v-flex>
                                    <v-flex xs12 pl-1>
                                        <v-checkbox v-model="selectedObject.active" label="Active"></v-checkbox>
                                    </v-flex>
                                </template>

                                <!-- Shelf -->
                                <template v-else-if="activeType === typeConsts.Shelf">
                                    <v-flex xs12 pl-2>
                                        <v-select v-model="selectedObject.bookcaseId" :items="bookcaseList" label="Bookcase" item-value="id" item-text="name" :rules="[rules.required]"></v-select>
                                    </v-flex>
                                    <v-flex xs12 pl-2>
                                        <v-text-field v-model="selectedObject.name" label="Name" :rules="[rules.required]"></v-text-field>
                                    </v-flex>
                                </template>
                            </v-form>

                            <!-- Generic to all -->
                            <v-flex xs12>
                                <v-btn @click="selectedObject = null">
                                    CANCEL
                                </v-btn>
                                <v-btn class="primary" @click="updateObject()" :disabled="!validated">
                                    SAVE
                                </v-btn>
                                <v-btn class="float-right" @click="deleteObject()">
                                    DELETE
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
    import * as Helper from '../../../helpers'
    import { mapState } from 'vuex'

    const AUTHOR_LABEL = "Author";
    const BOOKCASE_LABEL = "Bookcase";
    const SERIES_LABEL = "Series";
    const SHELF_LABEL = "Shelf";

    const AUTHOR_VALUE = 0;
    const BOOKCASE_VALUE = 1;
    const SERIES_VALUE = 2;
    const SHELF_VALUE = 3;

    function getNewObject(type) {
        if (type === null || typeof type !== 'number') {
            return null;
        }

        switch (type) {
            case AUTHOR_VALUE:
                return Helper.getNewAuthor();
                break;
            case BOOKCASE_VALUE:
                return Helper.getNewBookcase();
                break;
            case SERIES_VALUE:
                return Helper.getNewSeries();
                break;
            case SHELF_VALUE:
                return Helper.getNewShelf();
                break;
            default:
                break;
        }
    }

    export default {
        data() {
            return {
                dialogOpen: false,
                selectedObject: null,
                activeObject: null,
                activeType: null,
                activeList: [],
                typeList: [
                    AUTHOR_LABEL,
                    BOOKCASE_LABEL,
                    SERIES_LABEL,
                    SHELF_LABEL,
                ],
                typeConsts: {
                    Author: AUTHOR_VALUE,
                    Bookcase: BOOKCASE_VALUE,
                    Series: SERIES_VALUE,
                    Shelf: SHELF_VALUE,
                },
                rules: {
                    required: value => !!value || 'Value is required'
                },
                validated: false,
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
                bookcaseList: state => state.library.bookcases,
                seriesList: state => state.library.series,
                shelfList: state => state.library.shelves,
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
            open(newValue) {
                this.$_console_log(`[Dependency Editor Open] open = ${newValue}`);
                this.dialogOpen = newValue;
            },
            dialogOpen(newValue) {
                if (newValue === false) {
                    this.$_console_log('[Dependency Editor Dialog Open] Dialog open = false');
                    this.$emit('closeDialog', false);
                }
                else {
                    this.$_console_log('[Dependency Editor Dialog OpenAuthor Dialog] Dialog open = true');
                }
            },
            activeType(newValue, oldValue) {
                this.$_console_log(`[Dependency Editor Active Type] New value is: ${newValue}`);

                if (newValue === null || typeof newValue !== 'number') {                    
                    this.activeList = [];
                    this.activeObject = -1;
                    return;
                }

                if (oldValue !== newValue) {
                    this.activeList = [];
                    this.activeObject = -1;
                }
                else {
                    return;
                }

                this.getActiveList(newValue);
            },
            activeObject(newValue) {
                this.$_console_log('Selected object watcher', newValue);
                if (newValue === null || typeof newValue !== 'number') {
                    this.selectedObject = null;
                    this.activeObject = -1;
                    return;
                }

                if (newValue === -1) {
                    this.selectedObject = null;
                    return;
                }

                this.selectedObject = Object.assign({}, this.activeList[newValue]);
            }
        },
        methods: {
            getActiveList(value) {
                if (typeof value === 'undefined' || value === null) {
                    value = this.activeType;
                }

                switch (value) {
                    case 0:
                        this.activeList = this.authorList.slice(0);
                        break;
                    case 1:
                        this.activeList = this.bookcaseList.slice(0);
                        break;
                    case 2:
                        this.activeList = this.seriesList.slice(0);
                        break;
                    case 3:
                        this.activeList = this.shelfList.slice(0);
                        break;
                    default:
                        break;
                }
            },
            addObject() {
                if (this.activeType === null || typeof this.activeType !== 'number') {
                    this.$_console_log(`[Dependency Editor] Add Object: Active type is null or not a number: (${this.activeType})`);
                    return;
                }

                const obj = getNewObject(this.activeType);
                if (obj === null) {
                    this.$_console_log(`[Dependency Editor] Add Object: New object is null. Not making a new object`);
                    return;
                }

                this.activeList.push(obj);
                this.activeObject = this.activeList.length - 1;
            },
            updateObject() {
                if (this.activeType === null || typeof this.activeType !== 'number') {
                    this.$_console_log(`[Dependency Editor] Update Object: Active type is null or not a number: (${this.activeType})`);
                    return;
                }

                if (this.selectedObject.id === 0) {
                    if (this.$refs.objectForm.validate()) {
                        this.$store.dispatch(`add${this.typeList[this.activeType]}`, this.selectedObject).then(resp => {
                            this.$_console_log(`[Dependency Editor] Update Object: Successfully added new object of type (${this.typeList[this.activeType]}). Removing temp object.`);
                            this.getActiveList();
                            this.selectedObject = this.activeList.find(x => x.id === resp.id);
                        });
                    }
                }
                else {
                    if (this.$refs.objectForm.validate()) {
                        this.$store.dispatch(`edit${this.typeList[this.activeType]}`, this.selectedObject);
                        this.getActiveList();
                    }
                }
            },
            deleteObject() {
                if (this.activeType === null || typeof this.activeType !== 'number') {
                    this.$_console_log(`[Dependency Editor] Delete Object: Active type is null or not a number: (${this.activeType})`);
                    return;
                }

                if (this.selectedObject.id <= 0) {
                    this.activeObject = -1;
                    this.getActiveList();
                }
                else {
                    this.$store.dispatch(`delete${this.typeList[this.activeType]}`, this.selectedObject.id).then(resp => {
                        // If we successfully delete the object, let's unselect that entry as it won't be the same entry anymore
                        this.activeObject = -1;
                        this.getActiveList();
                    });
                }

            }
        }
    }
</script>

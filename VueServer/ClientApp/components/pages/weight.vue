<template>
    <div>
        <v-dialog v-model="dialogOpen" max-width="500px">
            <v-card>
                <v-card-title>
                    <span class="headline">{{ headerText }}</span>
                </v-card-title>

                <v-card-text>
                    <v-container>
                        <v-layout wrap>
                            <v-flex xs12 class="text-xs-center">
                                <v-date-picker v-model="weight.created"></v-date-picker>
                            </v-flex>
                            <v-flex xs12>
                                <v-text-field v-model="weight.value" label="Value" suffix="lbs" class="weight-input"></v-text-field>
                            </v-flex>
                            <v-flex xs12>
                                <v-text-field v-model="weight.notes" label="Notes"></v-text-field>
                            </v-flex>
                            <v-flex xs12 class="text-xs-center">
                                <v-btn @click="addOrEditWeight()">{{ submitText }}</v-btn>
                            </v-flex>
                        </v-layout>
                    </v-container>
                </v-card-text>
            </v-card>
        </v-dialog>

        <v-container fluid>
            <v-layout row wrap px-2>
                <v-flex xs12 sm6 md4>
                    Create new weight:
                    <v-btn icon @click="openAddOrEditWeight(null, null)" class="green--text">
                        <fa-icon icon="plus"></fa-icon>
                    </v-btn>
                </v-flex>

                <v-flex xs12 sm6>
                    <div class="text-sm-left text-md-right headline font-weight-bold">Average weight loss / week:</div>
                    <!--<div>{{ avgWeightLoss + " lbs" }}</div>-->
                    <div class="text-sm-left text-md-right headline" :class="weightLossCss">{{ avgWeightLossWeek + " lbs" }}</div>
                </v-flex>

                <v-flex xs12>
                    <v-data-table :headers="getHeaders"
                                  :items="weightList"
                                  class="elevation-1">
                        <template v-slot:body="{ items }">
                            <tbody>
                                <tr v-for="(item, index) in weightList" @click="openAddOrEditWeight(item, $event)" :key="index">
                                    <td>
                                        {{ getDate(item.created) }}
                                    </td>
                                    <td>
                                        {{ item.value }}
                                    </td>
                                    <td>
                                        {{ item.notes }}
                                    </td>
                                    <td>
                                        <v-btn icon @click="deleteItem(item.id)"><fa-icon size="lg" icon="window-close" /></v-btn>
                                    </td>
                                </tr>
                            </tbody>
                        </template>
                        <template v-slot:no-data>
                            NO DATA HERE!
                        </template>
                        <template v-slot:no-results>
                            NO RESULTS HERE!
                        </template>
                    </v-data-table>
                </v-flex>
                <!--</v-layout>-->
                <!--</v-flex>-->
            </v-layout>
        </v-container>
    </div>
</template>

<script>
    import { setTimeout } from 'core-js';
    import weightService from '../../services/weight'

    import { padTwo } from '../../helpers'

    function getNewWeight() {
        return {
            id: -1,
            created: new Date().toISOString().substr(0, 10),
            value: '',
            userId: '',
            notes: null
        }
    }

    export default {
        data() {
            return {
                weightList: [],
                weight: null,
                tableOptions: {
                    sortBy: ['created'],
                    sortDesc: [true],
                    itemsPerPage: 25
                },
                dialogOpen: false,
            }
        },
        created() {
            this.weight = getNewWeight();
            this.getData();
        },
        computed: {
            headerText() {
                if (typeof this.weight === 'undefined' || this.weight === null) {
                    return '';
                }

                if (this.weight.id > 0) {
                    return 'Edit Existing Weight';
                }
                else {
                    return 'Add New Weight';
                }
            },
            submitText() {
                if (typeof this.weight === 'undefined' || this.weight === null) {
                    return 'Submit';
                }

                if (this.weight.id > 0) {
                    return 'Update';
                }
                else {
                    return 'Add';
                }
            },
            getHeaders() {
                return [
                    { text: 'Date', align: 'start', value: 'date', sortable: true, width: '31%', sort: (a, b) => { console.log(a); console.log(b); return 1; } },
                    { text: 'Weight', align: 'start', value: 'value', sortable: true, width: '31%' },
                    { text: 'Notes', align: 'start', value: 'notes', sortable: false, width: '31%' },
                    { text: '', align: 'start', value: 'actions', sortable: false, width: '7%' },
                ];
            },
            // TODO: Do this on the server and return it
            avgWeightLoss() {
                let weightLoss = 0;

                if (typeof this.weightList === 'undefined' || this.weightList == null || this.weightList.length === 0) {
                    return weightLoss;
                }

                const dates = this.weightList.map(x => new Date(x.created));
                const weights = this.weightList.map(x => x.value);

                const maxDate = new Date(Math.max.apply(null, dates));
                const minDate = new Date(Math.min.apply(null, dates));
                const diffTime = Math.abs(maxDate - minDate);
                const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
                if (diffDays === 0) {
                    console.log('Difference in days is 0, can\'t divide by 0');
                    return weightLoss;
                }

                console.log(this.weightList[0].created);
                console.log(new Date(this.weightList[0].created));

                const maxWeight = this.weightList.find(x => new Date(x.created).getTime() == minDate.getTime()).value;
                const minWeight = this.weightList.find(x => new Date(x.created).getTime() == maxDate.getTime()).value;

                //const maxWeight = Math.max.apply(null, weights); //.map(x => x.value);
                //const minWeight = Math.min.apply(null, weights); //.map(x => x.value);


                console.log(minDate);
                console.log(maxDate);
                console.log(diffDays);

                console.log(maxWeight);
                console.log(minWeight);
                
                weightLoss = ((maxWeight - minWeight) / diffDays);

                console.log(weightLoss);
                return weightLoss;
            },
            avgWeightLossWeek() {
                return (this.avgWeightLoss * 7).toFixed(2);
            },
            weightLossCss() {
                console.log(this.avgWeightLossWeek);
                if (this.avgWeightLossWeek >= 2) {
                    return 'green--text';
                }
                else if (this.avgWeightLossWeek >= 1) {
                    return 'yellow--text';
                }
                else if (this.avgWeightLossWeek >= 0) {
                    return 'orange--text';
                }
                else if (this.avgWeightLossWeek < 0) {
                    return 'red--text';
                }
            }
        },
        methods: {
            async getData() {
                this.$_console_log("[Weight] Get weights");
                weightService.getWeightList().then(resp => {
                    this.$_console_log('[Weight] Success getting weights');
                    this.$_console_log(resp.data);
                    if (resp.data !== '')
                        this.weightList = resp.data;
                }).catch(() => this.$_console_log('[Weight] Error getting weights') );
            },
            openAddOrEditWeight(item, evt) {
                this.$_console_log("[Weight] Creating or editing a weight object");

                if (typeof item === 'undefined') {
                    this.$_console_log('[Weight] Open Add Or Edit Weight: Weight is undefined or empty')
                    return;
                }

                if (evt !== null && (evt.target.classList.contains('v-btn__content')
                    || evt.target.parentNode.classList.contains('v-btn__content')
                    || evt.target.parentNode.parentNode.classList.contains('v-btn__content'))
                ) {
                    this.$_console_log('[Weight] Some button was pressed, don\'t open the dialog.');
                    return;
                }

                if (item === null) {
                    this.weight = getNewWeight();
                }
                else {
                    let newObj = getNewWeight();
                    
                    newObj.id = item.id;
                    newObj.value = item.value;
                    newObj.notes = item.notes;
                    newObj.created = item.created.substr(0, item.created.indexOf('T'));

                    this.weight = Object.assign({}, newObj);
                }

                this.dialogOpen = true;
            },
            addOrEditWeight() {
                if (typeof this.weight === 'undefined' || this.weight === null) {
                    this.$_console_log('[Weight] Add or Edit Weight: Weight is null or empty, exiting');
                    return;
                }

                // add
                if (this.weight.id <= 0) {
                    this.addNew(this.weight);
                }
                // edit
                else {
                    this.editWeight(this.weight);
                }
            },
            async addNew(item) {
                this.$_console_log("[Weight] Create weight");
                this.$_console_log(item);
                if (item.value === "") {
                    this.$_console_log('[Weight] Weight is empty, not going to create it');
                }
                else {
                    await weightService.addWeight(item).then(resp => {
                        this.$_console_log('[Weight] Success creating weight');
                        this.weight = getNewWeight();
                        this.weightList.push(resp.data);
                        this.dialogOpen = false;
                    }).catch(() => this.$_console_log('[Weight] Error creating weight'));
                }
            },
            async editWeight(item) {
                this.$_console_log("[Weight] Edit weight");
                this.$_console_log(item);
                if (typeof item === 'undefined' || item === null || item.id <= 0 || item.value === "") {
                    this.$_console_log('[Weight] Weight is empty, null, the id is less than or equal to 0, or the value is empty. Not going to edit it');
                    return;
                }

                await weightService.editWeight(item).then(resp => {
                    this.$_console_log('[Weight] Success editing weight');
                    const index = this.weightList.findIndex(x => x.id === item.id);
                    if (index === -1) {
                        this.$_console_log('[Weight] Index is -1. Weight doesn\t exist in list. Can\'t update local list.');
                    }
                    else {
                        this.weightList.splice(index, 1, resp.data)
                    }
                }).catch(() => this.$_console_log('[Weight] Error editing weight'))
                    .then(() => this.dialogOpen = false );
            },
            getDate(date) {
                if (date == null || date.length < 11)
                    return 'Unknown';

                return date.substr(0, date.indexOf('T'));
            },
            async deleteItem(id) {
                this.$_console_log(`[Weight] Delete weight with id: ${id}`);
                await weightService.deleteWeight(id).then(resp => {
                    this.$_console_log('[Weight] Success deleting weight');

                    let weightIndex = this.weightList.findIndex(x => x.id === id);
                    this.weightList.splice(weightIndex, 1);
                }).catch(() => this.$_console_log('[Weight] Error creating weight'));
            },
        },
    }
</script>

<style scoped>
    .weight-list-container {
        max-width: 500px;
    }
</style>

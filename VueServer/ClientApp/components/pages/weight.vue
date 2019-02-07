<template>
    <div>
        <v-dialog v-model="dialogOpen" max-width="500px">
            <v-card>
                <v-card-title>
                    <span class="headline">Add New Weight</span>
                </v-card-title>

                <v-card-text>
                    <v-container>
                        <v-layout wrap>
                            <v-flex xs12 class="text-xs-center">
                                <date-picker v-model="weight.created"
                                             valueType="format"
                                             :default-value="weight.created"
                                             :editable="true"
                                             :clearable="false"></date-picker>
                            </v-flex>
                            <v-flex xs3></v-flex>
                            <v-flex xs6>
                                <v-text-field v-model="weight.value" label="Value" suffix="lbs" class="weight-input"></v-text-field>
                            </v-flex>
                            <v-flex xs3></v-flex>
                            <v-flex xs12 class="text-xs-center">
                                <v-btn @click="addNew(weight)">Submit</v-btn>
                            </v-flex>
                        </v-layout>
                    </v-container>
                </v-card-text>
            </v-card>
        </v-dialog>

        <v-container>
            <v-layout row wrap>
                <v-flex xs12 md6>
                    <v-layout row wrap>
                        <v-flex xs12>
                            Create new weight:
                            <v-btn icon @click="create" class="green--text">
                                <fa-icon icon="plus"></fa-icon>
                            </v-btn>
                        </v-flex>
                    </v-layout>

                    <v-layout row wrap class="weight-list-container">
                        <v-flex xs5 class="font-weight-bold">
                            Date
                        </v-flex>
                        <v-flex xs5 class="font-weight-bold">
                            Weight
                        </v-flex>
                        <template v-for="(item, index) in weightList">
                            <v-flex xs5>
                                {{ getDate(item.created) }}
                            </v-flex>
                            <v-flex xs5>
                                {{ item.value }}
                            </v-flex>
                            <v-flex xs1>
                                <v-btn icon @click="deleteItem(item.id)"><fa-icon size="lg" icon="window-close" /></v-btn>
                            </v-flex>
                        </template>
                    </v-layout>
                </v-flex>
                <v-flex xs12 md6>
                    <div class="text-xs-center headline font-weight-bold">Average weight loss / week:</div>
                    <!--<div>{{ avgWeightLoss + " lbs" }}</div>-->
                    <div class="text-xs-center headline" :class="weightLossCss">{{ avgWeightLossWeek + " lbs" }}</div>
                </v-flex>
            </v-layout>
        </v-container>
    </div>
</template>

<script>
    import { setTimeout } from 'core-js';
    import weightService from '../../services/weight'

    import DatePicker from 'vue2-datepicker';
    import 'vue2-datepicker/index.css';

    function getNewWeight() {
        return {
            id: -1,
            created: new Date().toLocaleString(),
            value: '',
            userId: '',
        }
    }

    export default {
        data() {
            return {
                weightList: [],
                weight: {},
                //pagination: {
                //    sortBy: 'created',
                //    descending: true,
                //    rowsPerPage: 10
                //},
                dialogOpen: false,
            }
        },
        components: { DatePicker },
        created() {
            this.weight = getNewWeight();
            this.getData();
        },
        computed: {
            getHeaders() {
                return [
                    { text: 'Date', align: 'left', value: 'date' },
                    { text: 'Value', align: 'left', value: 'value' },
                ];
            },
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
            create() {
                this.$_console_log("[Weight] Creating a new open weight object");
                this.dialogOpen = true;
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
            getDate(date) {
                if (date == null || date.length < 11)
                    return 'Unknown';

                let year = date.substring(0, 4);
                let month = date.substring(5, 7);
                let day = date.substring(8, 10);

                return (`${year}/${month}/${day}`);
            },
            async deleteItem(id) {
                this.$_console_log(`[Weight] Delete weight with id: ${id}`);
                await weightService.deleteWeight(id).then(resp => {
                    this.$_console_log('[Weight] Success deleting weight');

                    let weightIndex = this.weightList.findIndex(x => x.id === id);
                    this.weightList.splice(weightIndex, 1);
                }).catch(() => this.$_console_log('[Weight] Error creating weight'));
            }
        },
    }
</script>

<style scoped>
    .weight-list-container {
        max-width: 500px;
    }
</style>

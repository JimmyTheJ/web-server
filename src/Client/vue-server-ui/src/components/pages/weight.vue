<template>
  <div>
    <generic-dialog
      :open="dialogOpen"
      :maxWidth="newDialogWidth"
      @dialog-close="dialogOpen = false"
    >
      <v-card>
        <v-card-text>
          <v-container>
            <v-layout wrap>
              <v-flex xs12 class="text-xs-center">
                <v-date-picker v-model="weight.created"></v-date-picker>
              </v-flex>
              <v-flex xs12>
                <v-text-field
                  v-model="weight.value"
                  label="Value"
                  suffix="lbs"
                  class="weight-input"
                ></v-text-field>
              </v-flex>
              <v-flex xs12>
                <v-text-field
                  v-model="weight.notes"
                  label="Notes"
                ></v-text-field>
              </v-flex>
              <v-flex xs12 class="text-xs-center">
                <v-btn @click="addOrEditWeight()">{{ submitText }}</v-btn>
              </v-flex>
            </v-layout>
          </v-container>
        </v-card-text>
      </v-card>
    </generic-dialog>

    <v-container fluid>
      <v-layout row wrap px-2>
        <v-flex xs12 sm6 md4>
          Create new weight:
          <v-btn
            icon
            @click="openAddOrEditWeight(null, null)"
            class="green--text"
          >
            <fa-icon icon="plus"></fa-icon>
          </v-btn>
        </v-flex>

        <v-flex xs12 sm6>
          <div class="text-sm-left text-md-right headline font-weight-bold">
            Average weight loss / week:
          </div>
          <div
            class="text-sm-left text-md-right headline"
            :class="weightLossCss"
          >
            {{ avgWeightLossWeek + ' lbs' }}
          </div>

          <div class="text-sm-left text-md-right headline font-weight-bold">
            Total Weight Loss:
          </div>
          <div
            class="text-sm-left text-md-right headline"
            :class="weightLossCss"
          >
            {{ totalWeightLoss + ' lbs' }}
          </div>
        </v-flex>

        <!-- Filtering by dates -->
        <v-flex xs12>
          <div class="headline">Filtering:</div>
        </v-flex>
        <v-flex xs12 sm6>
          <v-menu
            v-model="filterStartDateMenu"
            :close-on-content-click="false"
            :nudge-right="40"
            transition="scale-transition"
            offset-y
            min-width="290px"
          >
            <template v-slot:activator="{ on, attrs }">
              <v-text-field
                v-model="filterStartDate"
                label="Filter Start Date"
                prepend-icon="event"
                readonly
                v-bind="attrs"
                v-on="on"
              >
                <template v-slot:append-outer>
                  <v-btn @click="filterStartDate = null" text icon>
                    <fa-icon icon="window-close"></fa-icon>
                  </v-btn>
                </template>
              </v-text-field>
            </template>
            <v-date-picker
              v-model="filterStartDate"
              @input="filterStartDateMenu = false"
            ></v-date-picker>
          </v-menu>
        </v-flex>
        <v-flex xs12 sm6>
          <v-menu
            v-model="filterEndDateMenu"
            :close-on-content-click="false"
            :nudge-right="40"
            transition="scale-transition"
            offset-y
            min-width="290px"
          >
            <template v-slot:activator="{ on, attrs }">
              <v-text-field
                v-model="filterEndDate"
                label="End Date"
                prepend-icon="event"
                readonly
                v-bind="attrs"
                v-on="on"
              >
                <template v-slot:append-outer>
                  <v-btn @click="filterEndDate = null" text icon>
                    <fa-icon icon="window-close"></fa-icon>
                  </v-btn>
                </template>
              </v-text-field>
            </template>
            <v-date-picker
              v-model="filterEndDate"
              @input="filterEndDateMenu = false"
            ></v-date-picker>
          </v-menu>
        </v-flex>

        <!-- Filtered list results -->
        <v-flex xs12>
          <v-data-table
            :headers="getHeaders"
            :items="filteredWeightList"
            class="elevation-1"
          >
            <template v-slot:body="{ items }">
              <tbody>
                <tr
                  v-for="item in items"
                  :key="item.id"
                  @click="openAddOrEditWeight(item, $event)"
                >
                  <td>{{ getDate(item.created) }}</td>
                  <td>{{ item.value }}</td>
                  <td>{{ item.notes }}</td>
                  <td class="action-td">
                    <fa-icon
                      size="lg"
                      icon="window-close"
                      @click="deleteItem(item.id)"
                    />
                  </td>
                </tr>
              </tbody>
            </template>
            <template v-slot:no-data> NO DATA HERE! </template>
            <template v-slot:no-results> NO RESULTS HERE! </template>
          </v-data-table>
        </v-flex>
      </v-layout>
    </v-container>
  </div>
</template>

<script>
import { mapState } from 'vuex'
import { setTimeout } from 'core-js'
import weightService from '@/services/weight'
import Dispatcher from '@/services/ws-dispatcher'
import genericDialog from '@/components/modules/generic-dialog.vue'

function getNewWeight(user) {
  let usr = null
  if (typeof user !== 'undefined') usr = user

  return {
    id: -1,
    created: new Date().toISOString().substr(0, 10),
    value: '',
    userId: usr,
    notes: null,
  }
}

export default {
  components: { genericDialog },
  data() {
    return {
      filteredWeightList: [],
      filterStartDate: null,
      filterEndDate: null,
      filterStartDateMenu: false,
      filterEndDateMenu: false,
      weightList: [],
      weight: null,
      tableOptions: {
        sortBy: ['created'],
        sortDesc: [true],
        itemsPerPage: 25,
      },
      startDate: null,
      endDate: null,
      startWeight: 0,
      endWeight: 0,
      dialogOpen: false,
      newDialogWidth: 500,
    }
  },
  created() {
    this.weight = getNewWeight()
    this.getData()
  },
  computed: {
    ...mapState({
      user: state => state.auth.user,
    }),
    headerText() {
      if (typeof this.weight === 'undefined' || this.weight === null) {
        return ''
      }

      if (this.weight.id > 0) {
        return 'Edit Existing Weight'
      } else {
        return 'Add New Weight'
      }
    },
    submitText() {
      if (typeof this.weight === 'undefined' || this.weight === null) {
        return 'Submit'
      }

      if (this.weight.id > 0) {
        return 'Update'
      } else {
        return 'Add'
      }
    },
    getHeaders() {
      return [
        {
          text: 'Date',
          align: 'start',
          value: 'created',
          sortable: true,
          width: '21%',
        },
        {
          text: 'Weight',
          align: 'start',
          value: 'value',
          sortable: true,
          width: '21%',
        },
        {
          text: 'Notes',
          align: 'start',
          value: 'notes',
          sortable: false,
          width: '51%',
        },
        {
          text: 'Actions',
          align: 'start',
          value: 'actions',
          sortable: false,
          width: '7%',
        },
      ]
    },
    totalWeightLoss() {
      if (
        typeof this.startWeight !== 'number' ||
        typeof this.endWeight !== 'number'
      )
        return 0

      return (this.startWeight - this.endWeight).toFixed(2)
    },
    avgWeightLossPerDay() {
      let weightLoss = 0

      if (!Array.isArray(this.weightList) || this.weightList.length === 0) {
        this.$_console_log('[AverageWeightLoss] No weights in list')
        return weightLoss
      }

      const diffTime = Math.abs(this.endDate - this.startDate)
      const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24))
      if (diffDays === 0) {
        this.$_console_log(
          "[AverageWeightLoss] Difference in days is 0, can't divide by 0"
        )
        return weightLoss
      }

      weightLoss = (this.startWeight - this.endWeight) / diffDays

      this.$_console_log('[AverageWeightLoss] Weight loss: ' + weightLoss)
      return weightLoss
    },
    avgWeightLossWeek() {
      return (this.avgWeightLossPerDay * 7).toFixed(2)
    },
    weightLossCss() {
      this.$_console_log(this.avgWeightLossWeek)
      if (this.avgWeightLossWeek >= 2) {
        return 'green--text'
      } else if (this.avgWeightLossWeek >= 1) {
        return 'yellow--text'
      } else if (this.avgWeightLossWeek >= 0) {
        return 'orange--text'
      } else if (this.avgWeightLossWeek < 0) {
        return 'red--text'
      }
    },
  },
  watch: {
    weightList: {
      handler(newValue) {
        if (this.filterStartDate === null || this.filterEndDate === null) {
          this.filteredWeightList = newValue
        }
      },
      deep: true,
    },
    filteredWeightList: {
      handler(newValue) {
        if (!Array.isArray(newValue) || newValue.length === 0) {
          this.startDate = null
          this.endDate = null
          this.startWeight = 0
          this.endWeight = 0

          return
        }

        const dates = newValue.map(x => new Date(x.created))

        this.endDate = new Date(Math.max.apply(null, dates))
        this.startDate = new Date(Math.min.apply(null, dates))

        this.$_console_log('End Date: ', this.endDate)

        // TODO: Fix this.. This currently won't necessary give correct values if we have multiple weights on the same day,
        // there is no guarantee it picks the right value
        this.endWeight = newValue.find(
          x => new Date(x.created).getTime() == this.endDate.getTime()
        ).value
        this.startWeight = newValue.find(
          x => new Date(x.created).getTime() == this.startDate.getTime()
        ).value
      },
      deep: true,
    },
    filterStartDate(newValue) {
      this.filteredWeightList = this.filterWeight(newValue, this.filterEndDate)
    },
    filterEndDate(newValue) {
      this.filteredWeightList = this.filterWeight(
        this.filterStartDate,
        newValue
      )
    },
  },
  methods: {
    async getData() {
      this.$_console_log('[Weight] Get weights')
      Dispatcher.request(() => {
        weightService
          .getWeightList()
          .then(resp => {
            this.$_console_log('[Weight] Success getting weights')
            this.$_console_log(resp.data)
            if (Array.isArray(resp.data)) {
              this.weightList = resp.data
            } else {
              this.weightList = []
            }
          })
          .catch(() => this.$_console_log('[Weight] Error getting weights'))
      })
    },
    filterWeight(start, end) {
      let startDate = null
      let endDate = null

      if (start !== null) {
        startDate = new Date(start).getTime()
      }
      if (end !== null) {
        endDate = new Date(end).getTime() + 86000000 // End date + 1 day
      }

      if (startDate === null && endDate === null) {
        return this.weightList.slice(0)
      }

      const filteredWeightList = this.weightList.slice(0).filter(x => {
        var date = new Date(x.created).getTime()

        if (startDate === null) return date <= endDate ? true : false
        else if (endDate === null) return date >= startDate ? true : false
        else return date >= startDate && date <= endDate ? true : false
      })

      return filteredWeightList
    },
    openAddOrEditWeight(item, evt) {
      this.$_console_log('[Weight] Creating or editing a weight object')

      if (typeof item === 'undefined') {
        this.$_console_log(
          '[Weight] Open Add Or Edit Weight: Weight is undefined or empty'
        )
        return
      }

      if (
        evt !== null &&
        (evt.target.classList.contains('action-td') ||
          evt.target.parentNode.classList.contains('action-td') ||
          evt.target.parentNode.parentNode.classList.contains('action-td'))
      ) {
        this.$_console_log(
          "[Weight] Some button was pressed, don't open the dialog."
        )
        return
      }

      if (item === null) {
        this.weight = getNewWeight(this.user.id)
      } else {
        let newObj = getNewWeight(this.user.id)

        newObj.id = item.id
        newObj.value = item.value
        newObj.notes = item.notes
        newObj.created = item.created.substr(0, item.created.indexOf('T'))

        this.weight = Object.assign({}, newObj)
      }

      this.dialogOpen = true
    },
    addOrEditWeight() {
      if (typeof this.weight === 'undefined' || this.weight === null) {
        this.$_console_log(
          '[Weight] Add or Edit Weight: Weight is null or empty, exiting'
        )
        return
      }

      // add
      if (this.weight.id <= 0) {
        this.addNew(this.weight)
      }
      // edit
      else {
        this.editWeight(this.weight)
      }
    },
    async addNew(item) {
      this.$_console_log('[Weight] Create weight')
      this.$_console_log(item)
      if (item.value === '') {
        this.$_console_log('[Weight] Weight is empty, not going to create it')
      } else {
        Dispatcher.request(() => {
          weightService
            .addWeight(item)
            .then(resp => {
              this.$_console_log('[Weight] Success creating weight')
              this.weight = getNewWeight(this.user.id)
              this.weightList.push(resp.data)
              this.dialogOpen = false
            })
            .catch(() => this.$_console_log('[Weight] Error creating weight'))
        })
      }
    },
    async editWeight(item) {
      this.$_console_log('[Weight] Edit weight')
      this.$_console_log(item)
      if (
        typeof item === 'undefined' ||
        item === null ||
        item.id <= 0 ||
        item.value === ''
      ) {
        this.$_console_log(
          '[Weight] Weight is empty, null, the id is less than or equal to 0, or the value is empty. Not going to edit it'
        )
        return
      }

      Dispatcher.request(() => {
        weightService
          .editWeight(item)
          .then(resp => {
            this.$_console_log('[Weight] Success editing weight')
            const index = this.weightList.findIndex(x => x.id === item.id)
            if (index === -1) {
              this.$_console_log(
                "[Weight] Index is -1. Weight doesn\t exist in list. Can't update local list."
              )
            } else {
              this.weightList.splice(index, 1, resp.data)
            }
          })
          .catch(() => this.$_console_log('[Weight] Error editing weight'))
          .then(() => (this.dialogOpen = false))
      })
    },
    getDate(date) {
      if (date == null || date.length < 11) return 'Unknown'

      return date.substr(0, date.indexOf('T'))
    },
    async deleteItem(id) {
      this.$_console_log(`[Weight] Delete weight with id: ${id}`)
      Dispatcher.request(() => {
        weightService
          .deleteWeight(id)
          .then(resp => {
            this.$_console_log('[Weight] Success deleting weight')

            let weightIndex = this.weightList.findIndex(x => x.id === id)
            if (weightIndex >= 0) this.weightList.splice(weightIndex, 1)
          })
          .catch(() => this.$_console_log('[Weight] Error creating weight'))
      })
    },
  },
}
</script>

<style scoped>
.weight-list-container {
  max-width: 500px;
}
</style>

<template>
  <div>
    <depedency-editor-dialog
      :open="dependencyEditorDialogOpen"
      @closeDialog="closeDependencyEditorDialogWindow"
    />
    <book-dialog
      :open="bookDialogOpen"
      :book="activeBook"
      :loading="loadingBook"
      @closeDialog="closeBookDialogWindow"
    />

    <v-container>
      <v-flex xs12>
        Dependency Editor
        <v-btn
          icon
          @click="dependencyEditorDialogOpen = true"
          class="green--text"
        >
          <fa-icon icon="plus"></fa-icon>
        </v-btn>
      </v-flex>

      <v-flex xs12>
        Create new book entry:
        <v-btn
          icon
          @click="openAddOrEditBookDialog(null, null)"
          class="green--text"
        >
          <fa-icon icon="plus"></fa-icon>
        </v-btn>
      </v-flex>

      <v-flex xs12>
        <v-text-field
          v-model="bookSearch"
          label="Search Book List"
        ></v-text-field>
      </v-flex>

      <v-data-table
        :headers="getHeaders"
        :items="bookList"
        :search="bookSearch"
        class="elevation-1"
      >
        <template v-slot:body="{ items }">
          <tbody>
            <tr
              v-for="item in items"
              :key="item.id"
              @click="openAddOrEditBookDialog(item, $event)"
            >
              <td>{{ getTitle(item) }}</td>
              <td v-if="item.bookAuthors !== null">
                <div
                  v-for="(bookAuthor, index) in item.bookAuthors"
                  :key="index"
                >
                  <template v-if="bookAuthor.author !== null">
                    <p>{{ bookAuthor.author.fullName }}</p>
                  </template>
                </div>
              </td>
              <td v-else></td>
              <td class="hidden-sm-and-down">{{ item.edition }}</td>
              <td class="hidden-xs-only">
                <div v-for="(bookGenre, index) in item.bookGenres" :key="index">
                  <template v-if="bookGenre.genre !== null">
                    <p>{{ bookGenre.genre.name }}</p>
                  </template>
                </div>
              </td>
              <td class="hidden-sm-and-down">
                {{ getPublicationDate(item.publicationDate) }}
              </td>
              <td class="hidden-xs-only" v-if="item.hardcover">
                <fa-icon icon="check"></fa-icon>
              </td>
              <td class="hidden-xs-only" v-else>
                <fa-icon icon="times"></fa-icon>
              </td>
              <td class="hidden-xs-only" v-if="item.isRead">
                <fa-icon icon="check"></fa-icon>
              </td>
              <td class="hidden-xs-only" v-else>
                <fa-icon icon="times"></fa-icon>
              </td>
              <td class="hidden-sm-and-down" v-if="item.boxset">
                <fa-icon icon="check"></fa-icon>
              </td>
              <td class="hidden-sm-and-down" v-else>
                <fa-icon icon="times"></fa-icon>
              </td>
              <td class="hidden-sm-and-down" v-if="item.loaned">
                <fa-icon icon="check"></fa-icon>
              </td>
              <td class="hidden-sm-and-down" v-else>
                <fa-icon icon="times"></fa-icon>
              </td>
              <td>{{ getBookcaseName(item) }}</td>
              <td class="hidden-xs-only" v-if="hasSeriesInfo(item)">
                <fa-icon v-if="item.series.active" icon="minus"></fa-icon>
                <fa-icon v-else icon="ban"></fa-icon>
                {{ item.series.name }} ({{ item.seriesNumber }} /
                {{ item.series.number }})
              </td>
              <td class="hidden-xs-only" v-else></td>
              <td>
                <v-btn icon @click.prevent="deleteBook(item)">
                  <fa-icon size="md" icon="window-close"></fa-icon>
                </v-btn>
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
    </v-container>
  </div>
</template>

<script>
import dependencyEditor from '../modules/library/dependency-editor'
import bookDialog from '../modules/library/book-dialog'

import { mapState } from 'vuex'

import { padTwo } from '../../helpers'

import { NotificationActions, NotificationTypes } from '@/constants'

export default {
  components: {
    'book-dialog': bookDialog,
    'depedency-editor-dialog': dependencyEditor,
  },
  data() {
    return {
      activeBook: {},
      bookSearch: '',
      dependencyEditorDialogOpen: false,
      bookDialogOpen: false,
      loadingBook: false,
    }
  },
  computed: {
    ...mapState({
      authorList: state => state.library.authors,
      bookList: state => state.library.books,
      bookcaseList: state => state.library.bookcases,
      genreList: state => state.library.genres,
      seriesList: state => state.library.series,
      shelfList: state => state.library.shelves,
    }),
    getHeaders() {
      return [
        { value: 'title', alignment: 'left', text: 'Title' },
        { value: 'authors', alignment: 'left', text: 'Authors' },
        {
          value: 'edition',
          alignment: 'left',
          text: 'Edition',
          class: 'hidden-sm-and-down',
        },
        {
          value: 'genres',
          alignment: 'left',
          text: 'Genres',
          class: 'hidden-xs-only',
        },
        {
          value: 'publicationDate',
          alignment: 'left',
          text: 'Date',
          class: 'hidden-sm-and-down',
        },
        {
          value: 'hardcover',
          alignment: 'left',
          text: 'Hardcover',
          class: 'hidden-xs-only',
        },
        {
          value: 'isRead',
          alignment: 'left',
          text: 'Read',
          class: 'hidden-xs-only',
        },
        {
          value: 'boxset',
          alignment: 'left',
          text: 'Boxset',
          class: 'hidden-sm-and-down',
        },
        {
          value: 'loaned',
          alignment: 'left',
          text: 'Loaned Out',
          class: 'hidden-sm-and-down',
        },
        { value: 'bookcase', alignment: 'left', text: 'Bookcase' },
        {
          value: 'series',
          alignment: 'left',
          text: 'Series',
          class: 'hidden-xs-only',
        },
        { value: 'delete', alignment: 'left', text: '' },
      ]
    },
  },
  mounted() {
    this.getData()
  },
  methods: {
    async getData() {
      this.$_console_log('[Library] Get book list')

      this.$store.dispatch('getAuthors')
      this.$store.dispatch('getBooks')
      this.$store.dispatch('getBookcases')
      this.$store.dispatch('getGenres')
      this.$store.dispatch('getSeries')
      this.$store.dispatch('getShelves')
    },
    getCleanDate(value) {
      if (!value) return ''

      value = value.toString()
      this.$_console_log(value)
      if (value.endsWith('T00:00:00'))
        return value.substring(0, value.indexOf('T00:00:00'))

      return value
    },
    setActiveBook(obj) {
      this.activeBook = Object.assign({}, obj)
      this.activeBook.publicationDate = this.getCleanDate(
        this.activeBook.publicationDate
      )

      if (
        typeof this.activeBook.bookAuthors !== 'undefined' &&
        this.activeBook.bookAuthors !== null
      ) {
        this.activeBook.authors = []
        for (let i = 0; i < this.activeBook.bookAuthors.length; i++) {
          if (
            typeof this.activeBook.bookAuthors[i].author !== 'undefined' &&
            this.activeBook.bookAuthors[i].author !== null
          ) {
            let newAuthor = this.activeBook.bookAuthors[i].author
            newAuthor.fromAutocomplete = true
            //delete newAuthor.bookAuthors;
            this.activeBook.authors.push(newAuthor)
          }
        }
      }

      if (
        typeof this.activeBook.bookGenres !== 'undefined' &&
        this.activeBook.bookGenres !== null
      ) {
        this.activeBook.genres = []
        for (let i = 0; i < this.activeBook.bookGenres.length; i++) {
          if (
            typeof this.activeBook.bookGenres[i].genre !== 'undefined' &&
            this.activeBook.bookGenres[i].genre !== null
          ) {
            this.activeBook.genres.push(this.activeBook.bookGenres[i].genre)
          }
        }
      }
      delete this.activeBook.bookAuthors
      delete this.activeBook.bookGenres
    },
    openAddOrEditBookDialog(obj, evt) {
      this.$_console_log('[Library] Open add or edit dialog')
      this.$_console_log(obj)

      if (
        evt !== null &&
        (evt.target.classList.contains('v-btn__content') ||
          evt.target.parentNode.classList.contains('v-btn__content') ||
          evt.target.parentNode.parentNode.classList.contains('v-btn__content'))
      ) {
        this.$_console_log("Some button was pressed, don't open the dialog.")
        return
      }

      // add
      if (typeof obj === 'undefined' || obj === null) {
        this.$_console_log(
          '[Library] OpenAddOrEditBookDialog: Object is null or undefined'
        )
        this.activeBook = null
      }
      // edit
      else {
        const self = this
        this.loadingBook = true

        this.$nextTick(() => {
          self.setActiveBook(obj)
          this.$nextTick(() => {
            self.loadingBook = false
          })
        })
      }

      this.bookDialogOpen = true
    },
    getTitle(item) {
      if (typeof item === 'undefined' || item === null) return

      if (
        typeof item.subTitle === 'undefined' ||
        item.subTitle === null ||
        item.subTitle === ''
      )
        return item.title

      return `${item.title} : ${item.subTitle}`
    },
    deleteBook(item) {
      if (typeof item === 'undefined' || item === null) return

      this.$store
        .dispatch('deleteBook', item.id)
        .then(resp => {
          this.$store.dispatch('pushNotification', {
            text: 'Successfully deleted the book from the list',
            action: NotificationActions.Success,
            group: { type: NotificationTypes.LibraryBook, value: 'delete' },
          })
        })
        .catch(() => {
          this.$store.dispatch('pushNotification', {
            text: 'Failed to delete the book from the list',
            action: NotificationActions.Failed,
            group: { type: NotificationTypes.LibraryBook, value: 'delete' },
          })
        })
    },
    getBookcaseName(item) {
      if (
        typeof item === 'undefined' ||
        item === null ||
        item.bookcase === 'undefined' ||
        item.bookcase === null
      )
        return

      return item.bookcase.name
    },
    hasSeriesInfo(item) {
      if (
        typeof item === 'undefined' ||
        item === null ||
        item.series === 'undefined' ||
        item.series === null
      )
        return false

      return true
    },
    getPublicationDate(item) {
      if (item === null) return ''

      let date = new Date(item)
      return `${date.getFullYear()}-${padTwo(date.getMonth())}-${padTwo(
        date.getDate()
      )}`
    },
    closeDependencyEditorDialogWindow() {
      this.$_console_log(
        '[Close Depedency Editor Dialog Window] Reset value is:'
      )
      this.dependencyEditorDialogOpen = false
    },
    closeBookDialogWindow(reset) {
      this.$_console_log(`[Close Book Dialog Window] Reset value is: ${reset}`)
      this.bookDialogOpen = false

      this.activeBook = null
      //if (typeof reset === 'boolean' && reset) {
      //    this.activeBook = {};
      //}
    },
  },
}
</script>

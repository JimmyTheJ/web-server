<template>
    <div>
        <library-dialog :open="dialogOpen"
                        :book="activeBook"
                        :authorList="authorList"
                        :genreList="genreList"
                        :seriesList="seriesList"
                        :bookshelfList="bookshelfList"
                        @updateAuthors="updateAuthorList"
                        @updateBookshelves="updateBookshelfList"
                        @updateSeries="updateSeriesList"
                        @closeDialog="dialogOpen = false"
                        @addBook="addBookToList"
                        @editBook="editBookInList" />

        <v-container>
            <v-flex xs12>
                Create new book entry:
                <v-btn icon @click="openAddOrEditBookDialog(null, null)" class="green--text">
                    <fa-icon icon="plus"></fa-icon>
                </v-btn>
            </v-flex>

            <v-flex xs12>
                <v-text-field v-model="bookSearch" label="Search Book List"></v-text-field>
            </v-flex>

            <v-data-table :headers="getHeaders"
                          :items="bookList"
                          :search="bookSearch"
                          class="elevation-1">
                <template v-slot:body="{ items }">
                    <tbody>
                        <tr v-for="item in items" :key="item.id" @click="openAddOrEditBookDialog(item, $event)">
                            <td>{{ getTitle(item) }}</td>
                            <td v-if="item.bookAuthors !== null">
                                <template v-for="bookAuthor in item.bookAuthors">
                                    <template v-if="bookAuthor.author !== null">
                                        <p>{{ bookAuthor.author.fullName }}</p>
                                    </template>
                                </template>
                            </td>
                            <td v-else></td>
                            <td class="hidden-sm-and-down">{{ item.edition }}</td>
                            <td class="hidden-xs-only">{{ getGenre(item) }}</td>
                            <td class="hidden-sm-and-down">{{ getPublicationDate(item.publicationDate) }}</td>
                            <td class="hidden-xs-only" v-if="item.hardcover"><fa-icon icon="check"></fa-icon></td>
                            <td class="hidden-xs-only" v-else><fa-icon icon="times"></fa-icon></td>
                            <td class="hidden-xs-only" v-if="item.isRead"><fa-icon icon="check"></fa-icon></td>
                            <td class="hidden-xs-only" v-else><fa-icon icon="times"></fa-icon></td>
                            <td>{{ getBookshelfName(item) }}</td>
                            <td class="hidden-xs-only" v-if="hasSeriesInfo(item)">
                                <fa-icon v-if="item.series.active" icon="minus"></fa-icon>
                                <fa-icon v-else icon="ban"></fa-icon>
                                {{ item.series.name }} ({{ item.seriesNumber }} / {{ item.series.number }})
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
    import libraryService from '../../services/library'
    import libraryDialog from '../modules/library-dialog'

    import { mapState } from 'vuex'

    function padTwo(number) {
        return (number < 10 ? '0' : '') + number;
    }

    export default {
        components: {
            'library-dialog': libraryDialog
        },
        data() {
            return {
                activeBook: {},
                bookSearch: '',
                dialogOpen: false,
            }
        },
        computed: {
            ...mapState({
                authorList: state => state.library.authors,
                bookList: state => state.library.books,
                bookshelfList: state => state.library.bookshelves,
                genreList: state => state.library.genres,
                seriesList: state => state.library.series,
            }),
            getHeaders() {
                return [
                    { value: 'title', alignment: 'left', text: 'Title' },
                    { value: 'authors', alignment: 'left', text: 'Authors' },
                    { value: 'edition', alignment: 'left', text: 'Edition', class: 'hidden-sm-and-down' },
                    { value: 'genre', alignment: 'left', text: 'Genre', class: 'hidden-xs-only' },
                    { value: 'publicationDate', alignment: 'left', text: 'Date', class: 'hidden-sm-and-down' },
                    { value: 'hardcover', alignment: 'left', text: 'Hardcover', class: 'hidden-xs-only' },
                    { value: 'isRead', alignment: 'left', text: 'Read', class: 'hidden-xs-only' },
                    { value: 'bookshelf', alignment: 'left', text: 'Bookshelf' },
                    { value: 'series', alignment: 'left', text: 'Series', class: 'hidden-xs-only' },
                    { value: 'delete', alignment: 'left', text: '' },
                ];
            },
        },
        mounted() {
            this.getData();
        },
        methods: {
            async getData() {
                this.$_console_log("[Library] Get book list");

                this.$store.dispatch('getAuthors');
                this.$store.dispatch('getBooks');
                this.$store.dispatch('getBookshelves');
                this.$store.dispatch('getGenres');
                this.$store.dispatch('getSeries');
                

                //// Get list of books
                //libraryService.book.getList().then(resp => {
                //    this.$_console_log('[Library] Success getting book list');
                //    this.$_console_log(resp.data);
                //    if (typeof resp.data !== 'undefined' && resp.data !== null)
                //        this.bookList = resp.data;
                //}).catch(() => this.$_console_log('[Library] Error getting book list'));


                //// TODO: Convert all of these things to be stored in the VUEX store

                //// Get list of Genres
                //libraryService.genre.getList().then(resp => {
                //    this.$_console_log('[Library] Success getting genre list');
                //    this.$_console_log(resp.data);

                //    this.$_console_log(typeof resp.data);

                //    if (typeof resp.data !== 'undefined' && resp.data !== null) {
                //        this.genreList = resp.data;
                //        this.genreList.splice(0, 0, { id: -1, name: '' });
                //    }

                //}).catch(() => this.$_console_log('[Library] Error getting genre list'));

                //// Get list of bookshelves
                //libraryService.bookshelf.getList().then(resp => {
                //    this.$_console_log('[Library] Success getting bookshelf list');
                //    this.$_console_log(resp.data);
                //    if (typeof resp.data !== 'undefined' && resp.data !== null)
                //        this.bookshelfList = resp.data;
                //}).catch(() => this.$_console_log('[Library] Error getting bookshelf list'));

                //// Get list of Authors
                //libraryService.author.getList().then(resp => {
                //    this.$_console_log('[Library] Success getting author list');
                //    this.$_console_log(resp.data);
                //    if (typeof resp.data !== 'undefined' && resp.data !== null)
                //        this.authorList = resp.data;
                //}).catch(() => this.$_console_log('[Library] Error getting author list'));

                //// Get list of Series
                //libraryService.series.getList().then(resp => {
                //    this.$_console_log('[Library] Success getting series list');
                //    this.$_console_log(resp.data);
                //    if (typeof resp.data !== 'undefined' && resp.data !== null)
                //        this.seriesList = resp.data;
                //}).catch(() => this.$_console_log('[Library] Error getting series list'));
            },
            getCleanDate(value) {
                if (!value) return '';

                value = value.toString();
                console.log(value);
                if (value.endsWith('T00:00:00'))
                    return value.substring(0, value.indexOf('T00:00:00'))

                return value;
            },
            setActiveBook(obj) {
                this.activeBook = Object.assign({}, obj);
                this.activeBook.publicationDate = this.getCleanDate(this.activeBook.publicationDate);
                if (typeof this.activeBook.bookAuthors !== 'undefined' && this.activeBook.bookAuthors !== null) {
                    this.activeBook.authors = [];
                    for (let i = 0; i < this.activeBook.bookAuthors.length; i++) {
                        if (typeof this.activeBook.bookAuthors[i].author !== 'undefined' && this.activeBook.bookAuthors[i].author !== null) {
                            let newAuthor = this.activeBook.bookAuthors[i].author;
                            newAuthor.fromAutocomplete = true;
                            //delete newAuthor.bookAuthors;
                            this.activeBook.authors.push(newAuthor);
                        }
                    }
                }
                delete this.activeBook.bookAuthors;
            },
            openAddOrEditBookDialog(obj, evt) {
                this.$_console_log('[Library] Open add or edit dialog');
                this.$_console_log(obj);

                if (evt !== null && (evt.target.classList.contains('v-btn__content')
                    || evt.target.parentNode.classList.contains('v-btn__content')
                    || evt.target.parentNode.parentNode.classList.contains('v-btn__content'))
                ) {
                    this.$_console_log('Some button was pressed, don\'t open the dialog.');
                    return;
                }

                // add
                if (typeof obj === 'undefined' || obj === null) {
                    this.$_console_log('[Library] OpenAddOrEditBookDialog: Object is null or undefined');
                    this.activeBook = null;
                }
                // edit
                else {
                    this.setActiveBook(obj);
                }

                this.dialogOpen = true;
            },
            getTitle(item) {
                if (typeof item === 'undefined' || item === null)
                    return;

                if (typeof item.subTitle === 'undefined' || item.subTitle === null || item.subTitle === '')
                    return item.title;

                return `${item.title} : ${item.subTitle}`;
            },
            deleteBook(item) {
                if (typeof item === 'undefined' || item === null)
                    return;

                libraryService.book.delete(item.id).then(resp => {
                    this.$_console_log(`[Library] Success deleting book: ${item.title}`);
                    this.$_console_log(resp.data);

                    this.bookList.splice(this.bookList.findIndex(x => x.id === item.id));
                }).catch(() => {
                    // TODO: Indicate the book failed to be deleted somehow
                    this.$_console_log('[Library] Error deleting a book')
                });
            },
            getBookshelfName(item) {
                if (typeof item === 'undefined' || item === null)
                    return;
                if (item.bookshelf === 'undefined' || item.bookshelf === null)
                    return;

                return item.bookshelf.name;
            },
            hasSeriesInfo(item) {
                if (typeof item === 'undefined' || item === null)
                    return false;
                if (item.series === 'undefined' || item.series === null)
                    return false;

                return true;
            },
            getGenre(item) {
                if (typeof item === 'undefined' || item === null)
                    return '';
                if (item.genre === 'undefined' || item.genre === null)
                    return '';

                return item.genre.name;
            },
            getPublicationDate(item) {
                if (item === null)
                    return '';

                let date = new Date(item);
                return `${date.getFullYear()}-${padTwo(date.getMonth())}-${padTwo(date.getDate())}`;
            },
            updateBookshelfList(book) {
                this.$_console_log('[Update Bookshelf List] Update Bookshelf List');
                if (typeof book === 'undefined' || book === null || book.bookshelf === null) {
                    this.$_console_log('[Update Bookshelf List] Book or bookshelf is null');
                    return;
                }
                if (typeof this.bookshelfList === 'undefined' || this.bookshelfList === null) {
                    this.$_console_log('[Update Bookshelf List] Bookshelf list is null');
                    return;
                }

                const index = this.bookshelfList.findIndex(x => x.id === book.bookshelf.id)
                this.$_console_log(`Bookshelf index = ${index}`);
                if (index === -1)   // New bookshelf
                    this.bookshelfList.push(book.bookshelf);
            },
            updateSeriesList(book) {
                this.$_console_log('[Update Series List] Update Series List');
                if (typeof book === 'undefined' || book === null || book.series === null) {
                    this.$_console_log('[Update Series List] Book or series is null');
                    return;
                }
                if (typeof this.seriesList === 'undefined' || this.seriesList === null) {
                    this.$_console_log('[Update Series List] Series list is null');
                    return;
                }

                const index = this.seriesList.findIndex(x => x.id === book.series.id)
                this.$_console_log(`Series index = ${index}`);
                if (index === -1)   // New Series
                    this.seriesList.push(book.series);
            },
            updateAuthorList(book) {
                this.$_console_log('[Update Author List] Update Author List');
                if (typeof book === 'undefined' || book === null || book.bookAuthors === null) {
                    this.$_console_log('[Update Author List] Book or bookAuthors is null');
                    return;
                }
                if (typeof this.authorList === 'undefined' || this.authorList === null) {
                    this.$_console_log('[Update Author List] Author list is null');
                    return;
                }

                for (let i = 0; i < book.bookAuthors.length; i++) {
                    let index = this.authorList.findIndex(x => x.id === book.bookAuthors[i].authorId);
                    this.$_console_log(`Author index = ${index}`);
                    if (index === -1) // New Author
                        this.authorList.push(book.bookAuthors[i].author);
                }
            },
            addBookToList(book) {
                this.bookList.push(book);
                this.dialogOpen = false;
                this.activeBook = {};
            },
            editBookInList(book) {
                this.$_console_log('[Library] Edit Book in List', book);

                let index = this.bookList.findIndex(x => x.id === book.id);
                if (index < 0) {
                    // Indicate it failed somehow
                    this.$_console_log('Failed to splice edited book into list')
                }
                else {
                    this.bookList.splice(index, 1, book);
                    this.dialogOpen = false;
                    this.activeBook = {};
                }
                
            }
        }
    }
</script>

<template>
    <div>
        <v-dialog v-model="dialogOpen" :max-width="maxModalWidth">
            <v-card>
                <v-card-title>
                    <span class="headline">Add New Book</span>
                </v-card-title>

                <v-card-text>
                    <v-container>
                        <v-layout wrap>
                            <!-- Titles -->
                            <v-flex xs12>
                                <v-text-field v-model="activeBook.title" label="Title"></v-text-field>
                            </v-flex>
                            <v-flex xs12>
                                <v-text-field v-model="activeBook.subTitle" label="Sub Title"></v-text-field>
                            </v-flex>

                            <!-- Authors -->
                            <!-- TODO: Add autocomplete add author queryable -->
                            <v-flex xs12 v-if="activeBook.authors.length > 0">
                                Authors:
                            </v-flex>
                            <template v-for="(item, index) in activeBook.authors">
                                <v-flex xs5 pr-1>
                                    <v-text-field v-model="item.firstName" label="First Name"></v-text-field>
                                </v-flex>
                                <v-flex xs5 px-1>
                                    <v-text-field v-model="item.lastName" label="Last Name"></v-text-field>
                                </v-flex>
                                <v-flex xs1>
                                    <v-checkbox v-model="item.deceased" label="Deceased"></v-checkbox>
                                </v-flex>
                                <v-flex xs1 class="text-right">
                                    <v-btn icon @click="deleteAuthor(index)">
                                        <fa-icon size="md" icon="window-close" />
                                    </v-btn>
                                </v-flex>
                            </template>
                            <v-flex xs12>
                                <v-btn @click="addAuthor()">
                                    <fa-icon icon="plus"></fa-icon>&nbsp;Add Author
                                </v-btn>
                            </v-flex>

                            <!-- Other fields -->
                            <v-flex xs12>
                                <v-text-field v-model="activeBook.publicationDate" label="Publication Date"></v-text-field>
                            </v-flex>
                            <v-flex xs12>
                                <v-text-field v-model="activeBook.edition" label="Edition"></v-text-field>
                            </v-flex>
                            <v-flex xs12>
                                <v-checkbox v-model="activeBook.hardcover" label="Hardcover"></v-checkbox>
                                <v-checkbox v-model="activeBook.isRead" label="Read"></v-checkbox>
                            </v-flex>

                            <!-- TODO: Split or sort the genre list better -->
                            <v-flex xs12>
                                <v-select v-model="activeBook.genre" :items="genreList" label="Genres" item-value="id" item-text="name"></v-select>
                            </v-flex>

                            <!-- Series -->
                            <template v-if="activeBook.series != null">
                                <v-flex xs12>
                                    Series:
                                </v-flex>
                                <v-flex xs7 pr-1>
                                    <v-combobox v-model="activeBook.series"
                                                    :items="seriesList"
                                                    :loading="seriesIsLoading"
                                                    :search-input.sync="seriesSearch"
                                                    color="white"
                                                    hide-no-data
                                                    hide-selected
                                                    item-text="name"
                                                    item-value="id"
                                                    label="Series Search"
                                                    placeholder="Start typing to Search"
                                                    prepend-icon="mdi-database-search"
                                                    return-object></v-combobox>
                                    <!--<v-text-field v-model="activeBook.series.name" label="Name"></v-text-field>-->
                                </v-flex>
                                <v-flex xs3 px-1>
                                    <v-text-field v-model="activeBook.series.number" label="Total Number in Series"></v-text-field>
                                </v-flex>
                                <v-flex xs1>
                                    <v-checkbox v-model="activeBook.series.active" label="Active"></v-checkbox>
                                </v-flex>
                                <v-flex xs1 class="text-right">
                                    <v-btn icon @click="deleteSeries()">
                                        <fa-icon size="md" icon="window-close" />
                                    </v-btn>
                                </v-flex>
                                <v-flex xs12>
                                    <v-text-field v-model="activeBook.seriesNumber" label="Book's Series Number"></v-text-field>
                                </v-flex>
                            </template>
                            <v-flex xs12 mt-3 v-if="activeBook.series === null">
                                <v-btn @click="addSeries()">
                                    <fa-icon icon="plus"></fa-icon>&nbsp;Add Series
                                </v-btn>
                            </v-flex>

                            <!-- Bookshelf -->
                            <template v-if="activeBook.bookshelf != null">
                                <v-flex xs12 mt-3>
                                    Bookshelf:
                                </v-flex>
                                <v-flex xs11 pr-1>
                                    <v-combobox v-model="activeBook.bookshelf"
                                                :items="bookshelfList"
                                                :loading="bookshelfIsLoading"
                                                :search-input.sync="bookshelfSearch"
                                                color="white"
                                                hide-no-data
                                                hide-selected
                                                item-text="name"
                                                item-value="id"
                                                label="Bookshelf Search"
                                                placeholder="Start typing to Search"
                                                prepend-icon="mdi-database-search"
                                                return-object></v-combobox>
                                    <!--<v-text-field v-model="activeBook.bookshelf.name" label="Name"></v-text-field>-->
                                </v-flex>
                                <v-flex xs1 class="text-right">
                                    <v-btn icon @click="deleteBookshelf()">
                                        <fa-icon size="md" icon="window-close" />
                                    </v-btn>
                                </v-flex>
                            </template>
                            <v-flex xs12 my-3 v-if="activeBook.bookshelf === null">
                                <v-btn @click="addBookshelf()">
                                    <fa-icon icon="plus"></fa-icon>&nbsp;Add Bookshelf
                                </v-btn>
                            </v-flex>

                            <!-- Submit -->
                            <v-flex xs12 class="text-right">
                                <v-btn @click="addOrUpdateBook(null)">Submit</v-btn>
                            </v-flex>
                        </v-layout>
                    </v-container>
                </v-card-text>
            </v-card>
        </v-dialog>

        <v-container>
            <v-flex xs12 sm6 md4>
                Create new book entry:
                <v-btn icon @click="openAddOrEditBookDialog(null)" class="green--text">
                    <fa-icon icon="plus"></fa-icon>
                </v-btn>
            </v-flex>

            <v-data-table v-model="selected"
                          :headers="getHeaders"
                          :items="bookList"
                          class="elevation-1">
                <template v-slot:body="{ items }">
                    <tbody>
                        <tr v-for="item in items" :key="item.id">
                            <td>{{ getTitle(item) }}</td>
                            <td v-if="item.authors !== null">
                                <template v-for="author in items.authors">
                                    <p>{{ getAuthorName(author) }}</p>
                                </template>
                            </td>
                            <td v-else></td>
                            <td>{{ item.edition }}</td>
                            <td>{{ getGenre(item) }}</td>
                            <td>{{ item.publicationDate }}</td>
                            <td v-if="item.hardcover"><fa-icon icon="check"></fa-icon></td>
                            <td v-else><fa-icon icon="times"></fa-icon></td>
                            <td v-if="item.isRead"><fa-icon icon="check"></fa-icon></td>
                            <td v-else><fa-icon icon="times"></fa-icon></td>
                            <td>{{ getBookshelfName(item) }}</td>
                            <td v-if="hasSeriesInfo(item)">
                                <fa-icon v-if="item.series.active" icon="minus"></fa-icon>
                                <fa-icon v-else icon="asterisk"></fa-icon>
                                {{ item.series.name }} ({{ item.seriesNumber }} / {{ item.series.number }})
                            </td>
                            <td v-else></td>
                            <td><v-btn icon @click="deleteBook(item)"><fa-icon size="md" icon="window-close"></fa-icon></v-btn></td>
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

    function getNewBookshelf() {
        return {
            id: -1,
            name: '',
        }
    }

    function getNewSeries() {
        return {
            id: -1,
            name: '',
            number: 0,
            active: false,
        }
    }

    function getNewArtist() {
        return {
            id: -1,
            firstName: null,
            lastName: '',
            deceased: false
        }
    }

    function getNewBook() {
        return {
            id: -1,
            title: '',
            subTitle: null,
            publicationDate: null,
            edition: null,
            hardcover: false,
            isRead: false,
            seriesNumber: 0,
            userId: null,
            genreId: null,
            bookshelfId: null,
            user: null,
            authors: [],
            genre: null,
            series: null,
            bookshelf: null,
        }
    }

    function getRequest(book) {
        return {
            book: {
                id: book.id,
                title: book.title,
                subTitle: book.subTitle,
                publicationDate: book.publicationDate,
                edition: book.edition,
                hardcover: book.hardcover,
                isRead: book.isRead,
                seriesNumber: book.seriesNumber,
            },
            series: book.series,
            genreId: book.genre,
            bookshelf: book.bookshelf,
            authors: book.authors,
        }
    }

    export default {
        data() {
            return {
                bookList: [],
                seriesList: [],
                bookshelfList: [],
                authorList: [],
                genreList: [],
                activeBook: {},
                selected: {},
                dialogOpen: false,
                isEdit: false,
                seriesIsLoading: false,
                seriesSearch: '',
                bookshelfIsLoading: false,
                bookshelfSearch: '',
            }
        },
        computed: {
            getHeaders() {
                return [
                    { value: 'title', alignment: 'left', text: 'Title' },
                    { value: 'authors', alignment: 'left', text: 'Authors' },
                    { value: 'edition', alignment: 'left', text: 'Edition' },
                    { value: 'genre', alignment: 'left', text: 'Genre' },
                    { value: 'publicationDate', alignment: 'left', text: 'Date' },
                    { value: 'hardcover', alignment: 'left', text: 'Hardcover' },
                    { value: 'isRead', alignment: 'left', text: 'Read' },
                    { value: 'bookshelf', alignment: 'left', text: 'Bookshelf' },
                    { value: 'series', alignment: 'left', text: 'Series' },
                    { value: 'delete', alignment: 'left', text: '' },
                ];
            },
            maxModalWidth() {
                if (window.innerWidth > 1400)
                    return 1200;
                else if (window.innerWidth > 1000)
                    return 800;
                else if (window.innerWidth > 800)
                    return 600;
                else
                    return 500;
            }
        },
        created() {
            this.activeBook = getNewBook();
        },
        mounted() {
            this.getData();
        },
        methods: {
            async getData() {
                this.$_console_log("[Library] Get book list");

                // Get list of books
                libraryService.book.getList().then(resp => {
                    this.$_console_log('[Library] Success getting book list');
                    this.$_console_log(resp.data);
                    if (resp.data !== '')
                        this.bookList = resp.data;
                }).catch(() => this.$_console_log('[Library] Error getting book list'));


                // TODO: Convert all of these things to be stored in the VUEX store

                // Get list of Genres
                libraryService.genre.getList().then(resp => {
                    this.$_console_log('[Library] Success getting genre list');
                    this.$_console_log(resp.data);
                    if (resp.data !== '')
                        this.genreList = resp.data;
                }).catch(() => this.$_console_log('[Library] Error getting genre list'));

                // Get list of bookshelves
                libraryService.bookshelf.getList().then(resp => {
                    this.$_console_log('[Library] Success getting bookshelf list');
                    this.$_console_log(resp.data);
                    if (resp.data !== '')
                        this.bookshelfList = resp.data;
                }).catch(() => this.$_console_log('[Library] Error getting bookshelf list'));

                // Get list of Authors
                libraryService.author.getList().then(resp => {
                    this.$_console_log('[Library] Success getting author list');
                    this.$_console_log(resp.data);
                    if (resp.data !== '')
                        this.authorList = resp.data;
                }).catch(() => this.$_console_log('[Library] Error getting author list'));

                // Get list of Series
                libraryService.series.getList().then(resp => {
                    this.$_console_log('[Library] Success getting series list');
                    this.$_console_log(resp.data);
                    if (resp.data !== '')
                        this.seriesList = resp.data;
                }).catch(() => this.$_console_log('[Library] Error getting series list'));
            },
            openAddOrEditBookDialog(obj) {
                this.$_console_log('[Library] Open add or edit dialog');
                this.$_console_log(obj);

                // add
                if (typeof obj === 'undefined' || obj === null) {
                    //this.activeBook = getNewBook();
                }
                // edit
                else {
                    this.activeBook = Object.assign({}, obj);
                }

                this.dialogOpen = true;
            },
            addOrUpdateBook() {
                this.$_console_log("[Library] Add or update book");
                this.$_console_log(this.activeBook);

                if (this.activeBook.id > -1) {
                    // Edit book code
                }
                else {
                    let request = getRequest(this.activeBook);
                    this.$_console_log(request);
                    libraryService.book.add(request).then(resp => {
                        this.$_console_log('[Library] Success adding a book');
                        this.$_console_log(resp.data);

                        // Add book to local list
                        this.bookList.push(resp.data);

                        // Close dialog on successul adding of book
                        this.dialogOpen = false;
                    }).catch(() => {
                        // TODO: Indicate the book failed to be created somehow
                        this.$_console_log('[Library] Error adding a book')
                    });
                }
                
            },
            addAuthor() {
                if (typeof this.activeBook.authors === 'undefined' || this.activeBook.authors === null) {
                    this.activeBook.authors = [];
                }

                this.activeBook.authors.push(getNewArtist());
            },
            deleteAuthor(index) {
                this.activeBook.authors.splice(index, 1);
            },
            addSeries() {
                if (typeof this.activeBook.series === 'undefined' || this.activeBook.series === null) {
                    this.activeBook.series = getNewArtist();
                }
            },
            deleteSeries() {
                this.activeBook.series = null;
            },
            addBookshelf() {
                if (typeof this.activeBook.bookshelf === 'undefined' || this.activeBook.bookshelf === null) {
                    this.activeBook.bookshelf = getNewBookshelf();
                }
            },
            deleteBookshelf() {
                this.activeBook.bookshelf = null;
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
            getAuthorName(author) {
                if (typeof author === 'undefined' || author === null)
                    return '';

                if (typeof author.firstName === 'undefined' || author.firstName === null || author.firstName === '') {
                    return author.lastName;
                }

                return `${author.firstName} ${author.lastName}`;
            }
        }
    }
</script>

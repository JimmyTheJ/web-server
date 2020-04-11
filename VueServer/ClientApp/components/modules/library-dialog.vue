<template>
    <v-dialog v-model="dialogOpen" :max-width="maxModalWidth">
        <v-card>
            <v-card-title>
                <span class="headline">{{ addEditDialogTitle }}</span>
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
                        <v-flex xs12>
                            Authors:
                        </v-flex>
                        <v-flex xs10 pr-1>
                            <v-combobox v-model="authorToAdd"
                                        :items="filteredAuthorList"
                                        :loading="authorIsLoading"
                                        :search-input.sync="authorSearch"
                                        color="white"
                                        hide-no-data
                                        hide-selected
                                        item-text="fullName"
                                        item-value="id"
                                        label="Author Search"
                                        placeholder="Start typing to Search"
                                        prepend-icon="fas fa-book-reader"
                                        return-object
                                        @keyup.enter="addAuthorToBook()">

                            </v-combobox>
                        </v-flex>
                        <v-flex xs2>
                            <v-btn @click="addAuthorToBook()">Add</v-btn>
                        </v-flex>
                        <template v-if="typeof activeBook.authors !== 'undefined' && activeBook.authors !== null && activeBook.authors.length > 0 && !deletingAuthor">
                            <v-flex xs12 px-2 v-for="(item, index) in activeBook.authors" :key="'author' + index">
                                <v-layout row>
                                    <v-flex xs11 sm10 pr-1>
                                        <v-text-field v-model="item.fullName" label="Full Name" :disabled="item.fromAutocomplete"></v-text-field>
                                    </v-flex>
                                    <v-flex sm1 class="hidden-xs-only">
                                        <v-checkbox v-model="item.deceased" label="Deceased" :disabled="item.fromAutocomplete"></v-checkbox>
                                    </v-flex>
                                    <v-flex xs1 class="text-right">
                                        <v-btn icon @click="deleteAuthor(index)">
                                            <fa-icon size="md" icon="window-close" />
                                        </v-btn>
                                    </v-flex>
                                </v-layout>
                            </v-flex>
                        </template>

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
                            <v-flex xs12 sm7 pr-1>
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
                                            prepend-icon="fas fa-book"
                                            return-object></v-combobox>
                                <!--<v-text-field v-model="activeBook.series.name" label="Name"></v-text-field>-->
                            </v-flex>
                            <v-flex xs7 sm3 px-1>
                                <v-text-field v-model="activeBook.series.number" label="Total Number in Series"></v-text-field>
                            </v-flex>
                            <v-flex xs4 sm1>
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
                                            prepend-icon="mdi-bookshelf"
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
</template>

<script>
    import libraryService from '../../services/library'

    function getNewBookshelf() {
        return {
            id: 0,
            name: '',
        }
    }

    function getNewSeries() {
        return {
            id: 0,
            name: '',
            number: 0,
            active: false,
        }
    }

    function getNewAuthor() {
        return {
            id: 0,
            firstName: null,
            lastName: null,
            fullName: null,
            deceased: false
        }
    }

    function getNewBook() {
        return {
            id: 0,
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

    export default {
        data() {
            return {
                dialogOpen: false,
                activeBook: {},
                seriesIsLoading: false,
                seriesSearch: '',
                bookshelfIsLoading: false,
                bookshelfSearch: '',
                authorIsLoading: false,
                authorSearch: '',
                authorToAdd: {},
                deletingAuthor: false,
                filteredAuthorList: [],
            }
        },
        props: {
            open: {
                type: Boolean,
                default: false,
            },
            book: {
                type: Object,
                required: false,
            },
            authorList: {
                type: Array,
                default: () => {
                    return [];
                }
            },
            bookshelfList: {
                type: Array,
                default: () => {
                    return [];
                }
            },
            genreList: {
                type: Array,
                default: () => {
                    return [];
                }
            },
            seriesList: {
                type: Array,
                default: () => {
                    return [];
                }
            },
        },
        computed: {
            maxModalWidth() {
                if (window.innerWidth > 1400)
                    return 1200;
                else if (window.innerWidth > 1000)
                    return 800;
                else if (window.innerWidth > 800)
                    return 600;
                else
                    return 500;
            },
            addEditDialogTitle() {
                if (this.activeBook.id > 0)
                    return 'Edit Book'
                else
                    return 'Add New Book'
            }
        },
        watch: {
            open: function (newValue) {
                this.$_console_log(`[Library Dialog] open = ${newValue}`);
                this.dialogOpen = newValue;
            },
            dialogOpen: function (newValue) {
                if (newValue === false) {
                    this.$_console_log('[Library Dialog] Dialog open = false');
                    this.$emit('closeDialog', newValue);
                }
                else {
                    this.$_console_log('[Library Dialog] Dialog open = true');
                }
            },
            book: function (newValue) {
                console.log('Book watcher. Book has changed.');
                if (typeof newValue === 'undefined' || newValue === null) {
                    this.activeBook = getNewBook();
                }
                else {
                    this.activeBook = newValue;
                }

                this.authorToAdd = getNewAuthor();
                this.updateFilteredAuthorList();
            },
            'activeBook.bookshelf': function (newValue) {
                if (typeof newValue === 'string') {
                    this.activeBook.bookshelf = getNewBookshelf();
                    this.activeBook.bookshelf.name = newValue;
                }
                else {
                    if (newValue !== null && newValue.id > 0) {
                        this.activeBook.bookshelfId = newValue.id;
                    }
                }
            },
            'activeBook.series': function (newValue) {
                if (typeof newValue === 'string') {
                    this.activeBook.series = getNewSeries();
                    this.activeBook.series.name = newValue;
                }
                else {
                    if (newValue !== null && newValue.id > 0) {
                        this.activeBook.seriesId = newValue.id;
                    }
                }
            },
        },
        methods: {
            updateFilteredAuthorList() {
                if (typeof this.activeBook === 'undefined' || typeof this.activeBook.authors === 'undefined' || this.activeBook.authors === null) {
                    console.log('Something is invalid.. breaking out of filtered author list');
                    return this.authorList;
                }

                let list = this.authorList.slice(0);
                //console.log('Starting list');
                //console.log(list.slice(0));
                for (let i = 0; i < this.activeBook.authors.length; i++) {
                    //console.log(`Active Book @ ${i} = ${this.activeBook.authors[i].fullName}`);
                    for (let j = 0; j < this.authorList.length; j++) {
                        //console.log(`Author List value @ ${j} = ${this.authorList[j].fullName}`);
                        if (this.authorList[j].id === this.activeBook.authors[i].id) {
                            let index = list.findIndex(x => x.id === this.activeBook.authors[i].id);
                            //console.log(`Index of ${this.authorList[j].fullName}`);
                            //console.log(index);
                            if (index !== -1) {
                                list.splice(index, 1);
                                //console.log('Current list after splicing');
                                //console.log(list.slice(0));
                                break;
                            }

                        }
                    }
                }

                this.filteredAuthorList = list.slice(0);
            },
            resetDialogFields() {
                this.authorToAdd = getNewAuthor();
                this.authorSearch = '';
                this.seriesIsLoading = false;
                this.seriesSearch = '';
                this.bookshelfIsLoading = false;
                this.bookshelfSearch = '';
                this.authorIsLoading = false;
            },
            addAuthorToBook() {
                if (typeof this.activeBook.authors === 'undefined' || this.activeBook.authors === null)
                    this.activeBook.authors = [];

                if (typeof this.authorToAdd === 'undefined' || this.authorToAdd === null)
                    this.authorToAdd = getNewAuthor();

                // Manual Add
                if (this.authorToAdd.id <= 0 && typeof this.authorSearch !== 'undefined' && this.authorSearch !== null && this.authorSearch.trim() !== '') {
                    let names = this.authorSearch.trim().split(' ');
                    if (names === null) return; // TODO: Error here somehow

                    if (names.length === 1) {
                        this.authorToAdd.lastName = names[0];
                        this.authorToAdd.fullName = names[0];
                    }
                    else {
                        this.authorToAdd.firstName = '';
                        this.authorToAdd.fullName = '';
                        for (let i = 0; i < names.length; i++) {
                            this.authorToAdd.fullName += `${names[i]} `;

                            if (i + 1 === names.length) {
                                this.authorToAdd.lastName = names[i];
                            }
                            else {
                                this.authorToAdd.firstName += `${names[i]} `;
                            }
                        }

                        this.authorToAdd.firstName = this.authorToAdd.firstName.trim();
                        this.authorToAdd.fullName = this.authorToAdd.fullName.trim();
                    }

                    this.authorToAdd.fromAutocomplete = false;
                }
                else {
                    this.authorToAdd.fromAutocomplete = true;
                }

                this.activeBook.authors.push(this.authorToAdd);
                this.authorToAdd = getNewAuthor();

                this.updateFilteredAuthorList();
            },
            deleteAuthor(index) {
                this.deletingAuthor = true;
                this.$_console_log(`[Library] Delete author at index ${index}`);

                // Trick to update the DOM
                this.$nextTick(() => {
                    this.activeBook.authors.splice(index, 1);
                    this.deletingAuthor = false;
                    this.updateFilteredAuthorList();
                });
            },
            addSeries() {
                if (typeof this.activeBook.series === 'undefined' || this.activeBook.series === null) {
                    this.activeBook.series = getNewSeries();
                }
            },
            deleteSeries() {
                this.activeBook.series = null;
                this.activeBook.seriesId = null;
            },
            addBookshelf() {
                if (typeof this.activeBook.bookshelf === 'undefined' || this.activeBook.bookshelf === null) {
                    this.activeBook.bookshelf = getNewBookshelf();
                }
            },
            deleteBookshelf() {
                this.activeBook.bookshelf = null;
                this.activeBook.bookshelfId = null;
            },
            addOrUpdateBook() {
                this.$_console_log("[Library] Add or update book");
                this.$_console_log(this.activeBook);

                // Update
                if (this.activeBook.id > 0) {
                    let request = this.getRequest(this.activeBook);
                    this.$_console_log(request);
                    libraryService.book.update(request).then(resp => {
                        this.$_console_log('[Library] Success editing a book');
                        this.$_console_log(resp.data);

                        // Fill in genre information in UI
                        if (resp.data.genreId !== null) {
                            resp.data.genre = this.getGenreFromId(resp.data.genreId);
                        }

                        // Add book to local list
                        this.$emit('editBook', resp.data);

                        // Update lists if we added new objects
                        this.updateBookshelfList(resp.data);
                        this.updateSeriesList(resp.data);
                        this.updateAuthorList(resp.data);
                    }).catch(() => {
                        // TODO: Indicate the book failed to be edited somehow
                        this.$_console_log('[Library] Error editing a book')
                    });
                }
                // Add
                else {
                    let request = this.getRequest(this.activeBook);
                    this.$_console_log(request);
                    libraryService.book.add(request).then(resp => {
                        this.$_console_log('[Library] Success adding a book');
                        this.$_console_log(resp.data);

                        // Fill in genre information in UI
                        if (resp.data.genreId !== null) {
                            resp.data.genre = this.getGenreFromId(resp.data.genreId);
                        }

                        // Add book to list
                        this.$emit('addBook', resp.data);

                        // Update lists if we added new objects
                        this.updateBookshelfList(resp.data);
                        this.updateSeriesList(resp.data);
                        this.updateAuthorList(resp.data);
                    }).catch(() => {
                        // TODO: Indicate the book failed to be created somehow
                        this.$_console_log('[Library] Error adding a book')
                    });
                }
            },
            updateBookshelfList(item) {
                this.$emit('updateBookshelves', item);
            },
            updateSeriesList(item) {
                this.$emit('updateSeries', item);
            },
            updateAuthorList(item) {
                this.$emit('updateAuthors', item);
            },
            getGenreFromId(id) {
                if (typeof id === 'undefined' || id === null || id === '') {
                    return null;
                }

                return this.genreList.find(x => x.id === id);
            },
            getRequest(book) {
                console.log(book);
                const bookRequest = {
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
                    authors: book.authors,
                };

                // Series
                if (typeof book.series !== 'undefined' && book.series !== null) {
                    // Existing series
                    if (book.series.id > 0) {
                        bookRequest.seriesId = book.series.id;
                    }
                    // New series
                    else {
                        bookRequest.series = book.series;
                    }
                }
                if (book.seriesId !== 'undefined' && book.seriesId !== null) {
                    bookRequest.seriesId = book.seriesId;
                }
                // Check if we don't have a series object, but we are trying to create a new object
                if ((typeof book.series === 'undefined' || book.series === null || (book.series.id === 0 && book.series.name === ''))
                        && typeof this.seriesSearch !== 'undefined' && this.seriesSearch !== null && this.seriesSearch !== '') {
                    if (typeof book.series === 'undefined' || book.series === null)
                        bookRequest.series = getNewSeries();
                    else {
                        bookRequest.series.active = book.series.active;
                        bookRequest.series.number = book.series.number;
                    }
                        
                    bookRequest.series.name = this.seriesSearch;                    
                }

                // Bookshelf
                if (typeof book.bookshelf !== 'undefined' && book.bookshelf !== null) {
                    // Existing bookshelf
                    if (book.bookshelf.id > 0) {
                        bookRequest.bookshelfId = book.bookshelf.id;
                    }
                    // New bookshelf
                    else {
                        bookRequest.bookshelf = book.bookshelf;
                    }
                }
                if (book.bookshelfId !== 'undefined' && book.bookshelfId !== null) {
                    bookRequest.bookshelfId = book.bookshelfId;
                }
                // Check if we don't have a bookshelf object, but we are trying to create a new object
                if ((typeof book.bookshelf === 'undefined' || book.bookshelf === null || (book.bookshelf.id === 0 && book.bookshelf.name === ''))
                        && typeof this.bookshelfSearch !== 'undefined' && this.bookshelfSearch !== null && this.bookshelfSearch !== '') {
                    bookRequest.bookshelf = getNewBookshelf();
                    bookRequest.bookshelf.name = this.bookshelfSearch;
                }

                // Genre
                if (typeof book.genre !== 'undefined' && book.genre !== null) {
                    if (book.genre > 0) {
                        bookRequest.genreId = book.genre;
                    }
                }
                if (typeof book.genreId !== 'undefined' && book.genreId !== null && (book.genre === null || book.genre !== -1)) {
                    bookRequest.genreId = book.genreId;
                }

                return bookRequest;
            }
        }
    }
</script>

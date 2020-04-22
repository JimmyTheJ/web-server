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
                        <v-flex xs12>Authors:</v-flex>
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

                        <!-- Checkboxes -->
                        <v-flex xs12 sm6 md3>
                            <v-checkbox v-model="activeBook.hardcover" label="Hardcover"></v-checkbox>
                        </v-flex>
                        <v-flex xs12 sm6 md3>
                            <v-checkbox v-model="activeBook.isRead" label="Read"></v-checkbox>
                        </v-flex>
                        <v-flex xs12 sm6 md3>
                            <v-checkbox v-model="activeBook.boxset" label="Boxset"></v-checkbox>
                        </v-flex>
                        <v-flex xs12 sm6 md3>
                            <v-checkbox v-model="activeBook.loaned" label="Loaned Out"></v-checkbox>
                        </v-flex>

                        <!-- TODO: Split or sort the genre list better -->
                        <v-flex xs10>
                            <v-select v-model="genreToAdd" :items="genreList" label="Genres" item-value="id" item-text="name"></v-select>
                        </v-flex>
                        <v-flex xs2>
                            <v-btn @click="addGenreToBook()">Add</v-btn>
                        </v-flex>
                        <template v-if="typeof activeBook.genres !== 'undefined' && activeBook.genres !== null && activeBook.genres.length > 0 && !deletingGenre">
                            <v-flex xs12>Genre List</v-flex>
                            <v-flex xs12 px-2 v-for="(item, index) in activeBook.genres" :key="'genre' + index">
                                <v-layout row>
                                    <v-flex xs10 pr-1>
                                        <v-text-field v-model="item.name" label="Name" disabled></v-text-field>
                                    </v-flex>
                                    <v-flex sm1 class="hidden-xs-only">
                                        <v-checkbox v-model="item.fiction" label="Fiction" disabled></v-checkbox>
                                    </v-flex>
                                    <v-flex xs1 class="text-right">
                                        <v-btn icon @click="deleteGenre(index)">
                                            <fa-icon size="md" icon="window-close" />
                                        </v-btn>
                                    </v-flex>
                                </v-layout>
                            </v-flex>
                        </template>

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

                        <!-- Bookcase -->
                        <template v-if="activeBook.bookcase != null">
                            <v-flex xs12 mt-3>
                                Bookcase:
                            </v-flex>
                            <v-flex xs11 pr-1>
                                <v-combobox v-model="activeBook.bookcase"
                                            :items="bookcaseList"
                                            :loading="bookcaseIsLoading"
                                            :search-input.sync="bookcaseSearch"
                                            color="white"
                                            hide-no-data
                                            hide-selected
                                            item-text="name"
                                            item-value="id"
                                            label="Bookcase Search"
                                            placeholder="Start typing to Search"
                                            prepend-icon="mdi-bookcase"
                                            return-object></v-combobox>
                            </v-flex>
                            <v-flex xs1 class="text-right">
                                <v-btn icon @click="deleteBookcase()">
                                    <fa-icon size="md" icon="window-close" />
                                </v-btn>
                            </v-flex>
                        </template>
                        <v-flex xs12 my-3 v-if="activeBook.bookcase === null">
                            <v-btn @click="addBookcase()">
                                <fa-icon icon="plus"></fa-icon>&nbsp;Add Bookcase
                            </v-btn>
                        </v-flex>

                        <!-- Shelf -->
                        <template v-if="activeBook.shelf != null">
                            <v-flex xs12 mt-3>
                                Shelf:
                            </v-flex>
                            <v-flex xs11 pr-1>
                                <v-combobox v-model="activeBook.shelf"
                                            :items="shelfList"
                                            :loading="shelfIsLoading"
                                            :search-input.sync="shelfSearch"
                                            color="white"
                                            hide-no-data
                                            hide-selected
                                            item-text="name"
                                            item-value="id"
                                            label="Shelf Search"
                                            placeholder="Start typing to Search"
                                            prepend-icon="mdi-bookcase"
                                            return-object></v-combobox>
                            </v-flex>
                            <v-flex xs1 class="text-right">
                                <v-btn icon @click="deleteShelf()">
                                    <fa-icon size="md" icon="window-close" />
                                </v-btn>
                            </v-flex>
                        </template>
                        <v-flex xs12 my-3 v-if="activeBook.shelf === null">
                            <v-btn @click="addShelf()">
                                <fa-icon icon="plus"></fa-icon>&nbsp;Add Shelf
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
    import { mapState } from 'vuex'

    function getRequest(book) {
        const bookRequest = {
            book: {
                id: book.id,
                title: book.title,
                subTitle: book.subTitle,
                publicationDate: book.publicationDate,
                edition: book.edition,
                hardcover: book.hardcover,
                isRead: book.isRead,
                loaned: book.loaned,
                boxset: book.boxset,
                notes: book.notes,
                seriesNumber: book.seriesNumber,
            },
            authors: book.authors,
            bookcase: book.bookcase,
            genres: book.genres,
            series: book.series,
            shelves: book.shelves
        };

        return bookRequest;
    }

    function getNewBookcase() {
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

    function getNewShelf() {
        return {
            id: 0,
            name: '',
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
            // Properties
            id: 0,
            boxset: false,
            edition: null,
            isRead: false,
            hardcover: false,
            loaned: false,
            notes: null,
            publicationDate: null,
            seriesNumber: 0,
            subTitle: null,
            title: '',

            // FKs
            bookcaseId: null,
            shelfId: null,
            seriesId: null,
            userId: null,

            // Objects
            user: null,            
            bookcase: null,
            series: null,
            shelf: null,
            authors: [],
            genres: [],
        }
    }

    export default {
        data() {
            return {
                dialogOpen: false,
                activeBook: {},
                seriesIsLoading: false,
                seriesSearch: '',
                bookcaseIsLoading: false,
                bookcaseSearch: '',
                shelfIsLoading: false,
                shelfSearch: '',
                authorIsLoading: false,
                authorSearch: '',
                authorToAdd: {},
                genreToAdd: -1,
                deletingAuthor: false,
                deletingGenre: false,
                filteredAuthorList: [],
                filteredGenreList: [],
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
                    this.$emit('closeDialog', false);
                }
                else {
                    this.$_console_log('[Library Dialog] Dialog open = true');
                }
            },
            book: function (newValue) {
                if (typeof newValue === 'undefined' || newValue === null) {
                    this.activeBook = getNewBook();
                }
                else {
                    this.activeBook = newValue;
                }

                this.authorToAdd = getNewAuthor();
                this.updateFilteredAuthorList();
            },
            'activeBook.bookcase': function (newValue) {
                if (typeof newValue === 'string') {
                    this.activeBook.bookcase = getNewBookcase();
                    this.activeBook.bookcase.name = newValue;
                }
                else if (typeof newValue === 'object') {
                    if (newValue !== null && newValue.id > 0) {
                        this.activeBook.bookcaseId = newValue.id;
                    }
                }
            },
            'activeBook.series': function (newValue) {
                if (typeof newValue === 'string') {
                    this.activeBook.series = getNewSeries();
                    this.activeBook.series.name = newValue;
                }
                else if (typeof newValue === 'object') {
                    if (newValue !== null && newValue.id > 0) {
                        this.activeBook.seriesId = newValue.id;
                    }
                }
            },
            'activeBook.shelf': function (newValue) {
                if (typeof newValue === 'string') {
                    this.activeBook.shelf = getNewShelf();
                    this.activeBook.shelf.name = newValue;
                }
                else if (typeof newValue === 'object') {
                    if (newValue !== null && newValue.id > 0) {
                        this.activeBook.shelfId = newValue.id;
                    }
                }
            },
        },
        methods: {
            updateFilteredAuthorList() {
                if (typeof this.activeBook === 'undefined' || typeof this.activeBook.authors === 'undefined' || this.activeBook.authors === null) {
                    this.$_console_log('Something is invalid.. breaking out of filtered author list');
                    return this.authorList;
                }

                let list = this.authorList.slice(0);
                for (let i = 0; i < this.activeBook.authors.length; i++) {
                    for (let j = 0; j < this.authorList.length; j++) {
                        if (this.authorList[j].id === this.activeBook.authors[i].id) {
                            let index = list.findIndex(x => x.id === this.activeBook.authors[i].id);
                            if (index !== -1) {
                                list.splice(index, 1);
                                break;
                            }

                        }
                    }
                }

                this.filteredAuthorList = list.slice(0);
            },
            updateFilteredGenreList() {
                if (typeof this.activeBook === 'undefined' || typeof this.activeBook.genres === 'undefined' || this.activeBook.genres === null) {
                    this.$_console_log('Something is invalid.. breaking out of filtered genre list');
                    return this.genreList;
                }

                let list = this.genreList.slice(0);
                for (let i = 0; i < this.activeBook.genres.length; i++) {
                    for (let j = 0; j < this.genreList.length; j++) {
                        if (this.genreList[j].id === this.activeBook.genres[i].id) {
                            let index = list.findIndex(x => x.id === this.activeBook.genres[i].id);
                            if (index !== -1) {
                                list.splice(index, 1);
                                break;
                            }

                        }
                    }
                }

                this.filteredGenreList = list.slice(0);
            },
            resetDialogFields() {
                this.authorToAdd = getNewAuthor();
                this.authorSearch = '';
                this.seriesIsLoading = false;
                this.seriesSearch = '';
                this.shelfIsLoading = false;
                this.shelfSearch = '';
                this.bookcaseIsLoading = false;
                this.bookcaseSearch = '';
                this.authorIsLoading = false;
            },
            addAuthorToBook() {
                if (!Array.isArray(this.activeBook.authors))
                    this.activeBook.authors = [];

                if (typeof this.authorToAdd !== 'object')
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
            addGenreToBook() {
                if (!Array.isArray(this.activeBook.genres))
                    this.activeBook.genres = [];

                if (typeof this.genreToAdd !== 'number')
                    return;

                const genre = this.genreList.find(x => x.id === this.genreToAdd);
                if (typeof genre === 'undefined') {
                    return;
                }

                this.activeBook.genres.push(genre);
                this.genreToAdd = -1;
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
            deleteGenre(index) {
                this.deletingGenre = true;
                this.$_console_log(`[Library] Delete genre at index ${index}`);

                // Trick to update the DOM
                this.$nextTick(() => {
                    this.activeBook.genres.splice(index, 1);
                    this.deletingGenre = false;
                    this.updateFilteredGenreList();
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
            addBookcase() {
                if (typeof this.activeBook.bookcase === 'undefined' || this.activeBook.bookcase === null) {
                    this.activeBook.bookcase = getNewBookcase();
                }
            },
            deleteBookcase() {
                this.activeBook.bookcase = null;
                this.activeBook.bookcaseId = null;
            },
            addShelf() {
                if (typeof this.activeBook.shelf === 'undefined' || this.activeBook.shelf === null) {
                    this.activeBook.shelf = getNewShelf();
                }
            },
            deleteShelf() {
                this.activeBook.shelf = null;
                this.activeBook.shelfId = null;
            },
            async addOrUpdateBook() {
                this.$_console_log("[Library] Add or update book");
                this.$_console_log(this.activeBook);

                // Update
                if (this.activeBook.id > 0) {
                    const request = getRequest(this.activeBook);
                    await this.$store.dispatch('editBook', request);

                    // TODO: Consider putting this in the success of the call above
                    this.$emit('closeDialog', true);
                }
                // Add
                else {
                    const request = getRequest(this.activeBook);
                    await this.$store.dispatch('addBook', request);

                    // TODO: Consider putting this in the success of the call above
                    this.$emit('closeDialog', true);
                }
            },
        }
    }
</script>

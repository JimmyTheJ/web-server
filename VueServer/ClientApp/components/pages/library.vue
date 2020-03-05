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
                            <v-flex xs12>
                                <v-text-field v-model="activeBook.title" label="Title"></v-text-field>
                            </v-flex>
                            <v-flex xs12>
                                <v-text-field v-model="activeBook.subTitle" label="Sub Title"></v-text-field>
                            </v-flex>
                            <v-flex xs12>
                                <v-text-field v-model="activeBook.publicationDate" label="Publication Date"></v-text-field>
                            </v-flex>
                            <v-flex xs12>
                                <v-text-field v-model="activeBook.edition" label="edition"></v-text-field>
                            </v-flex>
                            <v-flex xs12>
                                <v-checkbox v-model="activateBook.hardcover" label="Hardcover"></v-checkbox>
                            </v-flex>
                            <v-flex xs12>
                                <v-checkbox v-model="activateBook.isRead" label="Read"></v-checkbox>
                            </v-flex>
                            <!-- TODO: Add complex objects in here -->
                            <v-flex xs12 class="text-xs-center">
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
                <v-btn icon @click="openAddOrEditBookDialog(false)" class="green--text">
                    <fa-icon icon="plus"></fa-icon>
                </v-btn>
            </v-flex>

            <v-data-table v-model="selected"
                          :headers="getHeaders"
                          :items="bookList"
                          class="elevation-1">
            </v-data-table>
        </v-container>        
    </div>
</template>

<script>
    import libraryService from '../../services/library'

    function getNewBook() {
        return {
            id: -1,
            title: '',
            subTitle: null,
            publicationDate: null,
            edition: null,
            hardcover: false,
            isRead: false,
            userId: null,
            genreId: null,
            seriesItemId: null,
            bookshelfId: null,
            user: null,
            authors: null,
            genre: null,
            series: null,
            seriesItem: null,
            bookshelf: null,

            //authors: [
            //    {
            //        id: -1,
            //        firstName: null,
            //        lastName: null,
            //        deceased: false,
            //    },
            //],
            //genre: {
            //    id: -1,
            //    name: null,
            //    fiction: false,
            //},
            //series: {
            //    id: -1,
            //    name: null,
            //    number: 0,
            //    active: false,
            //},
            //seriesItem: {
            //    id: -1,
            //    number: 0,
            //    seriesId: -1,
            //},
            //bookshelf: {
            //    id: -1,
            //    name: null
            //}
        }
    }

    export default {
        data() {
            return {
                bookList: [],
                activeBook: {},
                selected: {},
                dialogOpen: false,
                isEdit: false,
            }
        },
        computed: {
            getHeaders() {
                return [
                    { value: 'title', alignment: 'left', text: 'Title' },
                    { value: 'sub-title', alignment: 'left', text: 'Sub Title' },
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
                        this.bookshelf = resp.data;
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
                this.dialogOpen = true;

                // add
                if (obj === null) {

                }
                // edit
                else {

                }
            },
            addOrUpdateBook(book) {
                this.$_console_log("[Library] Add or update book");
                this.$_console_log(book);

                if (book.id > -1) {
                    libraryService.book.add(book).then(resp => {
                        this.$_console_log('[Library] Success adding a book');
                        this.$_console_log(resp.data);
                    }).catch(() => this.$_console_log('[Library] Error adding a book'));
                }
                else {
                    // Edit book code
                }
                
            },
        }
    }
</script>

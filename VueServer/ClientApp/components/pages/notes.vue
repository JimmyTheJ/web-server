<template>
    <v-container>
        <h3>List of notes:</h3>
        <v-layout row wrap>
            <div v-for="note in noteList" :key="note.id">
                <note-editor :obj="note" @delete="deleteOld" @create="updateNote" @updateObj="modifyOld"></note-editor>
            </div>
        </v-layout>
        <v-layout row wrap class="note-container">
            <v-flex xs12>
                Create new note:
                <v-btn icon @click="create">
                    <fa-icon icon="plus"></fa-icon>
                </v-btn>
            </v-flex>
            
            <v-flex>
                <div v-for="(item, index) in newNotes" :key="item.id">
                    <note-editor :obj="item" @delete="deleteNew" @create="addNew" @updateObj="modifyNew"></note-editor>
                </div>
            </v-flex>
            
        </v-layout>
    </v-container>
</template>

<script>
    import noteService from '../../services/note'
    import DispatchFactory from '../../factories/dispatchFactory'
    import NoteEditor from '../modules/note-editor'
    import { setTimeout } from 'core-js';

    function initializeNewNote(id) {
        return {
            priority: 0,
            color: '',
            //created: '',
            id: id,
            text: '',
            type: 0,
            title: '',
            //updated: '',
            user: '',
        }
    }

    export default {
        data() {
            return {
                activeNote: {
                    priority: 0,
                    color: '',
                    created: '',
                    id: 0,
                    text: '',
                    type: 0,
                    updated: '',
                    user: '',
                },
                newNotes: [],
                noteList: [],
                idCount: -1,
            }
        },
        components: {
            "note-editor": NoteEditor
        },
        created() {
            this.newNotes.push(initializeNewNote(this.idCount--));
            this.getData();
        },
        methods: {
            async getData() {
                this.$_console_log("[Notes] Get notes");

                DispatchFactory.request(() => {
                    noteService.getNotes().then(resp => {
                        this.$_console_log('[notes] success getting notes', resp);
                        if (resp.data !== '')
                            this.noteList = resp.data;
                    }).catch(() => this.$_console_log('[notes] error getting notes'));
                });
            },
            create() {
                this.newNotes.push(initializeNewNote(this.idCount--));
            },
            async deleteOld(item) {
                DispatchFactory.request(() => {
                    noteService.deleteNote(item.id).then(resp => {
                        this.$_console_log('[Notes] Succesfully deleted note.');
                        let index = this.noteList.findIndex(x => x.id === item.id);
                        this.noteList.splice(index, 1);
                    }).catch(() => { this.$_console_log('[Notes] Failed to delete existing note') });
                });
            },
            deleteNew(note) {
                let index = this.newNotes.findIndex(x => x.id === note.id);
                this.newNotes.splice(index, 1);
            },
            async addNew(item) {
                this.$_console_log("[Notes] Create note");
                this.$_console_log(item);
                if (item.text === "") {
                    this.$_console_log('[Notes] Note is empty, not going to create it');
                }
                else {
                    DispatchFactory.request(() => {
                        noteService.createNote(item).then(resp => {
                            this.$_console_log('[Notes] Success creating note');
                            this.noteList.push(resp.data);

                            // Remove the new note and make a new one
                            if (this.newNotes.length === 1)
                                this.newNotes = [];
                            else {
                                this.newNotes.splice(index, 1);
                            }
                            setTimeout(() => {
                                if (this.newNotes.length === 0)
                                    this.newNotes.push(initializeNewNote(this.idCount--));
                            }, 10);

                        }).catch(() => this.$_console_log('[Notes] Error creating note'));
                    });
                }
            },
            modifyNew(note) {
                let index = this.newNotes.findIndex(x => x.id === note.id);
                this.newNotes[index].text = note.text;
                this.newNotes[index].title = note.title;
            },
            modifyOld(note) {
                let index = this.noteList.findIndex(x => x.id === note.id);
                this.noteList[index].text = note.text;
                this.noteList[index].title = note.title;
            },
            async updateNote(note) {
                this.$_console_log("[Notes] Update note");
                this.$_console_log(note);
                DispatchFactory.request(() => {
                    noteService.updateNote(note).then(resp => {
                        this.$_console_log('[Notes] Success updating note');
                        let index = this.noteList.findIndex(x => x.id === note.id);
                        //this.noteList[index] = Object.assign({}, resp.data)
                        this.noteList[index].updated = resp.data.updated;
                    }).catch(() => this.$_console_log('[Notes] Error creating note'));
                });
            },
        },
    }
</script>


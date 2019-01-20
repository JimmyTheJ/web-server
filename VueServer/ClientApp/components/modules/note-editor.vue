<template>
    <div class="note-container">
        <div class="note-card">
            <div class="nav-bar">
                <div class="nav-header left close" @click="deleteNote">
                    <div class="nav-bar-item left">X</div>
                </div>

                <div class="nav-header center">
                    <div class="textarea title" contenteditable="true" ref="title">

                    </div>
                </div>

                <div class="nav-header right create" @click="createNote">
                    <div class="nav-bar-item right">SAVE</div>
                </div>
            </div>
            <div class="note-inner-container">
                <div class="textarea main" contenteditable="true" ref="note">

                </div>
                <div class="nav-footer" ref="footer">

                </div>
            </div>
        </div>
    </div>

    <!--<v-toolbar dense>
        <v-overflow-btn :items="menu.text_size"
                        editable
                        label="Size"
                        hide-details
                        overflow></v-overflow-btn>

        <v-divider class="mr-2"
                   vertical></v-divider>

        <v-btn-toggle v-model="menu.text_styling"
                      class="transparent"
                      multiple>
            <v-btn :value="1" flat>
                <v-icon>format_bold</v-icon>
            </v-btn>

            <v-btn :value="2" flat>
                <v-icon>format_italic</v-icon>
            </v-btn>

            <v-btn :value="3" flat>
                <v-icon>format_underlined</v-icon>
            </v-btn>
        </v-btn-toggle>

        <v-divider class="mx-2" vertical></v-divider>

        <v-btn-toggle v-model="menu.alignment" class="transparent">
            <v-btn :value="1" flat>
                <v-icon>format_align_left</v-icon>
            </v-btn>

            <v-btn :value="2" flat>
                <v-icon>format_align_center</v-icon>
            </v-btn>

            <v-btn :value="3" flat>
                <v-icon>format_align_right</v-icon>
            </v-btn>

            <v-btn :value="4" flat>
                <v-icon>format_align_justify</v-icon>
            </v-btn>
        </v-btn-toggle>
    </v-toolbar>
    <v-card>
        <div id="note-editor-textarea" contenteditable="true" @keyup="setNotes">
            {{ note.text }}
        </div>
    </v-card>-->
</template>

<script>
    export default {
        data() {
            return {
                note: {
                    id: 0,
                    priority: 0,
                    type: 0,
                    color: 'yellow',
                    text: '',
                    title: '',
                    updated: '',
                },
                menu: {
                    text_styling: [1, 2, 3],
                },
            }
        },
        props: {
            obj: {
                type: Object,
                required: true,
            },
        },
        computed: {

        },
        watch: {
            'obj.updated': function () {
                this.note.updated = this.obj.updated;
                this.$refs.footer.innerText = this.getTime(this.obj.updated);
            },
        },
        created() {
            if (this.obj) {
                this.note = Object.assign({}, this.obj);
            }
        },
        mounted() {
            this.$refs.note.innerText = this.obj.text;
            this.$refs.title.innerText = this.obj.title;

            if (this.obj.updated !== '') {
                let time = this.getTime(this.obj.updated);
                if (time !== 'Invalid date')
                    this.$refs.footer.innerText = time;
            }

            this.$refs.note.addEventListener('keyup', this.updateText, false);
            this.$refs.title.addEventListener('keyup', this.updateTitle, false);
        },
        beforeDestroy() {
            this.$refs.note.removeEventListener('keyup', this.updateText, false);
            this.$refs.title.removeEventListener('keyup', this.updateTitle, false);
        },
        methods: {
            updateText() {
                setTimeout(() => {
                    this.$_console_log('Text: ' + this.$refs.note.innerText);
                    this.note.text = this.$refs.note.innerText;
                    this.$emit('updateObj', this.note);
                }, 1);
            },
            updateTitle() {
                setTimeout(() => {
                    this.$_console_log('Title: ' + this.$refs.title.innerText);
                    this.note.title = this.$refs.title.innerText;
                    this.$emit('updateObj', this.note);
                }, 1);
            },
            deleteNote() {
                this.$emit('delete', this.note);
            },
            createNote() {
                this.$emit('create', this.note);
            },
            getTime(time) {
                return this.$moment(new Date(time)).format('lll');
            },
        },
    }
</script>

<style scoped>
    .nav-header {
        width: 100%;
        padding: 3px;
        text-align: center;
    }

    .nav-header.center {
        /*text-align: center;*/
    }

    .nav-header.left {
        width: 25%;
        border-right: 3px solid black;
        /*text-align: center;*/
    }

    .nav-header.right {
        width: 25%;
        border-left: 3px solid black;
    }

    .nav-bar {
        display: flex;
        flex: auto;
        justify-content: center;
        width: 100%;
        height: 30px;
        /*margin-top: 2px;*/
    }

    .nav-bar-item.left {
        justify-content: flex-start;
        margin-left: 25px;
        padding: 2px;
    }

    .nav-bar-item.right {
        justify-content: flex-end;
        margin-right: 20px;
    }

    .nav-bar-item {
        display: inline-block;
        cursor: pointer;
        align-self: center;
        padding-right: 5px;
        padding-left: 5px;
        border-radius: 10px;

    }

    .nav-header.left:hover {
        background-color: #A0A0A0;
        color: #9C27B0;
        border-radius: 50px 0 0 0;
    }

    .nav-header.center:hover {
        background-color: #404040;
    }

    .nav-header.center:focus {
        background-color: #A0A0A0;
    }

    .nav-header.right:hover {
        background-color: #A0A0A0;
        color: #9C27B0;
        border-radius: 0 50px 0 0;
    }

    .nav-footer {
        border-top: 3px solid black;
        padding-top: 5px;
        text-align: center;
        font-style: italic;
    }

    .note-container {
        width: 400px;
        height: 300px;
        margin: 5px;
    }

    .note-card {
        border: 5px solid #000000;
        border-radius: 50px;
        height: 240px;
        width: 400px;
    }

    .note-inner-container {
        height: 200px;
        border-radius: 0 0 45px 45px;
    }

    .textarea:hover {
        background-color: #404040;
    }

    .textarea:focus {
        background-color: #404040;
    }

    .textarea {
        font: medium -moz-fixed;
        font: -webkit-small-control;
        outline: none;
        width: 100%;
    }

    .textarea.main {
        border-radius: 7px;
        border-top: 5px solid #000000;
        padding: 8px;
        height: 170px;
        overflow: auto;
    }

    .textarea.title {
        overflow: hidden;
        height: 24px;
        display: inline;
        white-space: nowrap;
    }

    .textarea.title br {
        display: none;
    }

    @media screen and (min-width: 240px) and (max-height: 767px) {
        .note-container {
            width: 320px;
        }

        .note-card {
            width: 320px;
        }
    }
</style>

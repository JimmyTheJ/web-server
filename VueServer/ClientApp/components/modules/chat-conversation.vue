<template>
    <div>
        <v-dialog v-model="deleteConversationDialog" max-width="400">
            <v-card>
                <v-card-title>
                    Delete Conversation
                </v-card-title>
                <v-card-text>
                    Are you sure you want to delete this conversation ?
                </v-card-text>
                <v-card-actions>
                    <v-btn @click="deleteConversation">Yes</v-btn>
                    <v-btn @click="deleteConversationDialog = false">No</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>

        <v-container>
            <v-layout row ma-2 style="max-width: 1100px">
                <v-toolbar>
                    <v-btn text @click="editingTitle = !editingTitle">
                        <v-icon>mdi-account-edit</v-icon>
                    </v-btn>
                    <v-toolbar-title v-if="!editingTitle">
                        {{ conversation.title }}
                    </v-toolbar-title>
                    <v-toolbar-title v-else style="width: 100%" class="ml-3 pl-2">
                        <div style="display: flex; flex-direction: row">
                            <v-text-field v-model="newTitle" @keyup.enter.prevent="updateTitle" autofocus></v-text-field>
                            <v-btn @click="updateTitle">SAVE</v-btn>
                        </div>
                    </v-toolbar-title>
                    <v-spacer></v-spacer>
                    <v-btn text v-if="isConversationDeletable" @click="deleteConversationDialog = true"><v-icon>mdi-delete</v-icon></v-btn>
                </v-toolbar>
                <v-flex xs12 v-for="(message, index) in conversation.messages" :key="index">
                    <chat-bubble :message="message" :currentTime="time" :owner="isOwner(message)" :color="getTextColor(message)" @deleteMessage="deleteMessage"></chat-bubble>
                </v-flex>
                <v-flex xs12 class="text-right">
                    <v-text-field v-model="newMessage.text" label="Message" ref="newMessage" @keyup.enter.prevent="sendMessage" autofocus></v-text-field>
                    <v-btn @click="sendMessage">Send</v-btn>
                </v-flex>
            </v-layout>
        </v-container>
    </div>
</template>
<script>
    import ChatBubble from './chat-bubble'
    import service from '../../services/chat'

    import { mapState } from 'vuex'

    export default {
        data() {
            return {
                newMessage: null,
                deleteConversationDialog: false,
                editingTitle: false,
                newTitle: null,
            }
        },
        components: {
            'chat-bubble': ChatBubble,
        },
        props: {
            conversation: {
                type: Object,
                required: true,
            },
            time: {
                type: Number,
                required: true,
            },
        },
        created() {
            this.newMessage = { text: '' };
            this.$chatHub.$on('message-received', this.onMessageReceived);
        },
        beforeDestroy() {
            this.$chatHub.$off('message-received', this.onMessageReceived);
        },
        computed: {
            ...mapState({
                user: state => state.auth.user,
                activeModules: state => state.auth.activeModules
            }),
            isConversationDeletable() {
                const chatModule = this.activeModules.find(x => x.id === 'chat');
                if (typeof chatModule !== 'undefined') {
                    const feature = chatModule.userModuleFeatures.find(x => x.moduleFeatureId === 'chat-delete-conversation')
                    if (typeof feature !== 'undefined') {
                        return true;
                    }
                }

                return false;
            },
        },
        watch: {
            editingTitle(newValue) {
                if (newValue === false) {
                    this.newTitle = null;
                }
                else {
                    this.newTitle = this.conversation.title;
                }
            },
        },
        methods: {
            onMessageReceived(message) {
                if (typeof message !== 'object' || message === null) {
                    this.$_console_log('OnMessageReceived: Null object returned');
                    return;
                }

                if (this.conversation.id !== message.conversationId) {
                    return;
                }

                this.$store.dispatch('addChatMessage', { conversationId: this.conversation.id, message: message })
            },
            async sendMessage() {
                this.newMessage.userId = this.user.id;
                this.newMessage.conversationId = this.conversation.id;
                await service.sendMessage(this.newMessage);

                this.newMessage = { text: '' };
                this.$refs.newMessage.focus();
            },
            getTextPosition(message) {
                if (message.userId === this.user.id) {
                    return 'text-right';
                }
                else {
                    return 'text-left';
                }
            },
            getTextColor(message) {
                if (message.userId === this.user.id) {
                    return 'blue';
                }
                else {
                    return 'green';
                }
            },
            deleteConversation() {
                this.$store.dispatch('deleteConversation', this.conversation.id)
                    .then(() => { }).catch(() => { }).then(() => this.deleteConversationDialog = false);
            },
            updateTitle() {
                this.$_console_log('New Title is: ' + this.newTitle);
                const title = this.newTitle;

                if (/^\s*$/.test(title)) {
                    this.editingTitle = false;
                    return;
                }

                this.$store.dispatch('updateConversationTitle', { conversationId: this.conversation.id, title: title })
                    .then(() => { }).catch(() => { }).then(() => this.editingTitle = false);
            },
            deleteMessage(id) {
                this.$store.dispatch('deleteChatMessage', { conversationId: this.conversation.id, messageId: id });
            },
            canDeleteMessage(message) {
                if (this.isMessageDeletable) {
                    if (message.userId === this.user.id) {
                        return true;
                    }
                }

                return false;
            },
            isOwner(message) {
                return this.user.id === message.userId;
            },
        }
    }
</script>

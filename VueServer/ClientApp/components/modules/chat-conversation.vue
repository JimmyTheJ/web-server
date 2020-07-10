<template>
    <div v-show="show">
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
        <v-dialog v-model="moreInfoDialog" max-width="640">
            <v-card>
                <v-card-title class="justify-center">
                    <h3>Message Info</h3>
                </v-card-title>
                <v-card-text>
                    <v-layout row>
                        <v-flex xs4 class="body-1 py-2">Message:</v-flex><v-flex xs8 class="caption py-2">{{ moreInfo.text }}</v-flex>
                        <v-flex xs4 class="body-1 py-2">Timestamp:</v-flex><v-flex xs8 class="caption py-2">{{ moreInfo.timestamp }}</v-flex>
                        <v-flex xs4 class="body-1 py-2">User:</v-flex><v-flex xs8 class="caption py-2">{{ moreInfo.username }}</v-flex>
                    </v-layout>
                </v-card-text>
                <v-card-actions>
                    <v-btn @click="moreInfoDialog = false">Close</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>

        <div id="chat-container">
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

            <div id="chat-body-container">
                <v-layout row class="px-2">
                    <v-flex xs12 v-for="(message, index) in conversation.messages" :key="index" @mouseover="message.hovering = true" @mouseleave="message.hovering = false" class="px-1">
                        <chat-bubble :message="message"
                                     :currentTime="time"
                                     :owner="isOwner(message)"
                                     :hover="message.hovering"
                                     :color="getTextColor(message)"
                                     @moreInfo="openMoreInfo"
                                     @deleteMessage="deleteMessage"></chat-bubble>
                    </v-flex>
                    <v-flex xs12 class="text-right px-1">
                        <v-layout row>
                            <v-flex xs12 class="px-2 mx-2">
                                <v-text-field v-model="newMessage.text"
                                              autofocus
                                              label="Message"
                                              ref="newMessage"
                                              @keyup.enter.prevent="sendMessage">
                                    <template v-slot:append-outer>
                                        <v-btn icon text @click="sendMessage"><fa-icon size="lg" icon="paper-plane"></fa-icon></v-btn>
                                    </template>
                                </v-text-field>
                            </v-flex>
                            <!--<v-flex xs2 class="pt-2 pr-2">
                            <v-btn @click="sendMessage" text><fa-icon size="2x" icon="paper-plane"></fa-icon></v-btn>
                        </v-flex>-->
                        </v-layout>
                    </v-flex>
                </v-layout>
            </div>
        </div>
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
                moreInfoDialog: false,
                editingTitle: false,
                newTitle: null,
                moreInfo: {},
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
            show: {
                type: Boolean,
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
            show(newValue) {
                if (newValue === true) {
                    setTimeout(() => {
                        this.scrollToBottom();
                    }, 10);                    
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

                this.$store.dispatch('addChatMessage', { conversationId: this.conversation.id, message: message }).then(() => {
                    this.scrollToBottom();
                })
            },
            async sendMessage() {
                this.newMessage.id = 0;
                this.newMessage.userId = this.user.id;
                this.newMessage.conversationId = this.conversation.id;
                console.log(this.newMessage);
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
                const title = this.newTitle;

                if (/^\s*$/.test(title)) {
                    this.editingTitle = false;
                    return;
                }

                this.$store.dispatch('updateConversationTitle', { conversationId: this.conversation.id, title: title })
                    .then(() => { }).catch(() => { }).then(() => this.editingTitle = false);
            },
            openMoreInfo(message) {
                this.moreInfo.text = message.text;
                this.moreInfo.timestamp = message.timestamp;
                this.moreInfo.username = message.userId;

                this.moreInfoDialog = true;
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
            scrollToBottom() {
                const chatWindow = document.getElementById('chat-body-container');
                if (chatWindow != null)
                    chatWindow.scrollTop = chatWindow.scrollHeight;
            }
        }
    }
</script>

<style>
    #chat-container {
        max-height: 800px;
        max-width: 1100px;
    }

    #chat-body-container {
        max-height: 780px;
        max-width: 1100px;
        overflow-y: scroll;
        overflow-x: hidden;
    }
</style>

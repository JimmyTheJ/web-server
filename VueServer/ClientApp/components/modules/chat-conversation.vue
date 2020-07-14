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
                    <v-flex xs12 v-for="(message, index) in conversation.messages" :key="index" @mouseover="setMessageHover(message, true)" @mouseleave="setMessageHover(message, false)" class="px-1">
                        <chat-bubble :message="message"
                                     :currentTime="time"
                                     :owner="isOwner(message)"
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
                chatWindow: null,
                scrollHeight: 0,
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
        mounted() {
            this.chatWindow = document.getElementById('chat-body-container');
            this.chatWindow.addEventListener('scroll', this.windowScroll)
        },
        beforeDestroy() {
            this.$chatHub.$off('message-received', this.onMessageReceived);
            this.chatWindow.removeEventListener('scroll', this.windowScroll)
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
            scrollWindowHeight() {
                return this.chatWindow.scrollTop;
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
            'conversation.messages.length': {
                handler(newValue, oldValue) {
                    if (typeof newValue === 'undefined' || newValue === null) {
                        this.$_console_log('ConversationMessages length watcher: New Value is null or undefined');
                        return;
                    }

                    if (typeof oldValue === 'undefined' || oldValue === null || newValue > oldValue) {
                        this.$_console_log('Old value is null or new value is greater than old value');
                        this.scrollToLastReadMessage();
                    }

                    //if (newValue > oldValue) {
                    //    this.$_console_log('Message list has grown');
                    //    setTimeout(() => {
                    //        this.scrollToBottom();
                    //    }, 50);
                    //}
                    
                },
                deep: true
            },
            show(newValue) {
                if (newValue === true) {
                    setTimeout(() => {
                        this.$_console_log('Show watcher: value is true');
                        if (Array.isArray(this.conversation.messages) && this.conversation.messages.length > 0) {
                            this.$_console_log('Show watcher: Message length is greater than 0. Scrolling to last read message');
                            this.scrollToLastReadMessage();
                        }
                    }, 10);                    
                }
            },
            'chatWindow.scrollTop': {
                handler(newValue) {
                    console.log(newValue);
                },
                deep: true
            }
        },
        methods: {
            windowScroll(event) {
                this.scrollHeight = event.target.scrollTop;
            },
            onMessageReceived(message) {
                if (typeof message !== 'object' || message === null) {
                    this.$_console_log('OnMessageReceived: Null object returned');
                    return;
                }

                if (this.conversation.id !== message.conversationId) {
                    return;
                }

                this.$store.dispatch('addChatMessage', { conversationId: this.conversation.id, message: message }).then(() => {
                    //this.scrollToBottom();

                    //if (message.userId !== this.user.id) {
                    //    this.$store.dispatch('readChatMessage', { conversationId: this.conversation.id, messageId: message.id });
                    //}
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
            isOwner(message) {
                if (this.user.id === message.userId) {
                    return true;
                }
                else {
                    return false;
                }
            },
            getTextPosition(message) {
                if (message.userId === this.user.id) {
                    return 'text-right';
                }
                else {
                    return 'text-left';
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
            scrollToBottom() {
                this.chatWindow.scrollTop = chatWindow.scrollHeight;
            },
            scrollToLastReadMessage() {
                let lastMessage = this.conversation.messages.find(x => this.user.id !== x.userId &&
                    (x.readReceipts.length === 0 || typeof x.readReceipts.find(y => y.userId !== this.user.id) !== 'undefined'));

                if (typeof lastMessage !== 'undefined') {
                    this.$nextTick(() => {
                        const ids = document.getElementsByClassName('bubble-id');

                        let correctElement = null;
                        let height = 0;
                        for (let i = 0; i < ids.length; i++) {
                            height += ids[i].parentNode.offsetHeight;
                            if (parseInt(ids[i].innerText, 10) === lastMessage.id) {
                                correctElement = ids[i];
                                break;
                            }
                        }

                        if (correctElement !== null) {
                            this.chatWindow.scrollTo(0, height - this.chatWindow.offsetTop);

                            this.$store.dispatch('highlightMessage', { messageId: lastMessage.id, conversationId: lastMessage.conversationId, on: true });
                            setTimeout(() => {
                                this.$store.dispatch('highlightMessage', { messageId: lastMessage.id, conversationId: lastMessage.conversationId, on: false });
                            }, 3000);
                        }
                    })
                }
                else {
                    this.$_console_log('ScrollToLastReadMessage: All messages are read.');
                }
            },
            readAllMessages() {
                if (!Array.isArray(this.conversation.messages)) {
                    return;
                }

                const newMessages = this.conversation.messages.filter(x => (!Array.isArray(x.readReceipts) || x.readReceipts.length === 0) && x.userId !== this.user.id);

                console.log(newMessages);
                //if (mesasage.userId !== this.user.id) {
                //    this.$store.dispatch('readChatMessage', { conversationId: this.conversation.id, messageId: this.message.id });
                //} 
            },
            setMessageHover(message, on) {
                if (on) {
                    this.$store.dispatch('setMessageHover', { messageId: message.id, conversationId: message.conversationId, on: true });
                }
                else {
                    this.$store.dispatch('setMessageHover', { messageId: message.id, conversationId: message.conversationId, on: false });
                }
            }
        }
    }

    function getPosition(element) {
        var xPosition = 0;
        var yPosition = 0;

        while (element) {
            xPosition += (element.offsetLeft - element.scrollLeft + element.clientLeft);
            yPosition += (element.offsetTop - element.scrollTop + element.clientTop);
            element = element.offsetParent;
        }

        return { x: xPosition, y: yPosition };
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

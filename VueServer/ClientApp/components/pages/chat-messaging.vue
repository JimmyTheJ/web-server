<template>
    <v-container>
        <v-layout>
            <v-flex xs12>
                <div class="headline">Chat System</div>
            </v-flex>
            </v-layout>

        <v-layout row>
            <v-flex xs12 sm10 md9>
                <v-autocomplete v-model="newConversation.users"
                                :items="userList"
                                prepend-icon="mdi-database-search"
                                label="Select user(s)"
                                :loading="isLoading"
                                :search-input.search="search"
                                multiple>
                </v-autocomplete>
                
            </v-flex>
            <v-flex xs12 sm2 md3>
                <v-btn @click="startConversation">Submit</v-btn>
            </v-flex>
        </v-layout>

        <v-layout row>
            <v-flex xs4 md3 lg2>
                <template v-if="Array.isArray(conversations) && conversations.length > 0">
                    <v-list shaped>
                        <v-list-item v-for="(convo, index) in conversations" :key="index" @click="selectedConversation = convo">
                            <v-list-item-icon>
                                <v-icon name="account"></v-icon>
                            </v-list-item-icon>
                            <v-list-item-content>
                                {{ convo.title }}
                            </v-list-item-content>
                        </v-list-item>
                    </v-list>
                </template>
            </v-flex>
            <v-flex xs8 md9 lg10>
                <template v-for="(conversation, index) in conversations">
                    <chat-conversation v-show="shouldShowConversation(conversation)"
                                       :conversation="conversation"
                                       :time="currentTime"
                                       @deleteConversation="deleteConversation"
                                       @updateTitle="updateTitle"
                                       @addMessage="addMessage">
                    </chat-conversation>
                </template>     
            </v-flex>
        </v-layout>
        
    </v-container>
</template>

<script>
    import Conversation from '../modules/chat-conversation'
    import chatService from '../../services/chat'
    import authService from '../../services/auth'

    import { mapState } from 'vuex';

    export default {
        data() {
            return {
                conversations: [],
                selectedConversation: null,
                newConversation: {
                    users: [],
                },
                userList: [],
                search: null,
                isLoading: false,
                currentTime: null
            }
        },
        components: {
            'chat-conversation': Conversation,
        },
        created() {
            this.getAllUsers();
            this.getAllConversations();
        },
        mounted() {
            const self = this;
            self.currentTime = Math.trunc(new Date().getTime() / 1000);

            setTimeout(() => {
                self.currentTime = Math.trunc(new Date().getTime() / 1000);
            }, 1000);
        },
        computed: {
            ...mapState({
                user: state => state.auth.user
            }),
        },
        methods: {
            async getAllUsers() {
                this.isLoading = true;
                authService.getUserIds().then(resp => {
                    if (Array.isArray(resp.data)) {
                        this.userList = resp.data;
                    }
                    else {
                        this.$_console_log('GetAllUsers: Data returned isn\'t an array');
                    }
                }).catch(() => this._console_log('GetAllUsers: Failed to get all users'))
                    .then(() => this.isLoading = false);
            },
            async getAllConversations() {
                chatService.getAllConversations(this.$store.state.auth.user.id).then(resp => {
                    if (Array.isArray(resp.data)) {
                        this.conversations = resp.data;

                        // Process the titles for all the conversations
                        this.conversations.forEach(conversation => {
                            conversation.title = this.getConversationTitle(conversation);
                        })
                    }
                    else {
                        this.$_console_log('GetAllConversations: Data returned isn\'t an array');
                    }
                }).catch(() => this.$_console_log('GetAllConversations: Error getting conversation lists'));
            },
            async startConversation() {
                chatService.startConversation(this.newConversation).then(resp => {
                    if (typeof resp.data === 'object' && resp.data !== null) {
                        resp.data.title = this.getConversationTitle(resp.data);
                        this.conversations.push(resp.data);
                    }
                    else {
                        this.$_console_log('StartConversation: Invalid data type returned');
                    }
                }).catch(() => this.$_console_log('StartConversation: Error starting conversation'))
                    .then(() => this.newConversation.users = []);
            },
            getConversationTitle(conversation) {
                if (conversation === null || typeof conversation !== 'object') {
                    return '';
                }


                if (typeof conversation.title === 'undefined' || conversation.title === null || conversation.title === '') {
                    const relevantUsers = conversation.conversationUsers.filter(x => x.userId != this.user.id);
                    if (Array.isArray(relevantUsers) && relevantUsers.length > 0) {
                        let title = '';
                        for (let i = 0; i < relevantUsers.length; i++) {
                            title += relevantUsers[i].userDisplayName;
                            if (i < relevantUsers.length - 1) {
                                title += ', '
                            }
                        }

                        return title;
                    }
                }

                return conversation.title;
            },
            deleteConversation(id) {
                const conversationIndex = this.conversations.findIndex(x => x.id === id);
                if (conversationIndex < 0) {
                    this.$_console_log('DeleteConversation: Can\'t find conversation to delete');
                    return;
                }

                chatService.deleteConversation(id).then(resp => {
                    if (typeof resp.data === 'boolean' && resp.data === true) {
                        if (typeof this.selectedConversation !== 'undefined' && this.selectedConversation !== 'null' && typeof this.selectedConversation.id === 'string') {
                            if (this.selectedConversation.id === id) {
                                this.selectedConversation = null;
                            }
                        }

                        this.conversations.splice(conversationIndex, 1);
                    } else {
                        this.$_console_log('DeleteConversation: Failed to delete conversation, response says false.')
                    }
                }).catch(() => this.$_console_log('DeleteConversation: Failed to delete conversation, API call failed.'));
            },
            updateTitle({ conversationId, title }) {
                chatService.updateConversationTitle(conversationId, title).then(resp => {
                    if (typeof resp.data === 'boolean' && resp.data === true) {
                        const conversationIndex = this.conversations.findIndex(x => x.id === conversationId);
                        if (conversationIndex >= 0) {
                            this.conversations[conversationIndex].title = title;
                        }
                        else {
                            this.$_console_log(`UpdateTitle: Failed to update title. Cannot find conversation with id (${conversationId})`)
                        }
                    }
                }).catch(() => {
                    this.$_console_log('UpdateTitle: Failed to update title. API call failed.')
                })
            },
            addMessage({ conversationId, message }) {
                const conversationIndex = this.conversations.findIndex(x => x.id === conversationId);
                if (conversationIndex >= 0) {
                    this.conversations[conversationIndex].messages.push(Object.assign({}, message));
                }
                else {
                    this.$_console_log(`addMessage: Failed to add message. Cannot find conversation with id (${conversationId})`)
                }
            },
            shouldShowConversation(conversation) {
                if (typeof conversation === 'object' && conversation !== null) {
                    if (typeof this.selectedConversation === 'object' && this.selectedConversation !== null) {
                        if (conversation.id === this.selectedConversation.id) {
                            return true;
                        }
                    }
                }

                return false;
            }
        },
    }
</script>

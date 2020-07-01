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
                                item-value="id"
                                item-text="displayName"
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
            <v-flex xs8 md9 lg10 class="chat-conversation-window">
                <template v-for="(conversation, index) in conversations">
                    <chat-conversation :conversation="conversation"
                                       :time="currentTime"
                                       :show="shouldShowConversation(conversation)">
                    </chat-conversation>
                </template>     
            </v-flex>
        </v-layout>
        
    </v-container>
</template>

<script>
    import Conversation from '../modules/chat-conversation'
    import authService from '../../services/auth'

    import { mapState } from 'vuex';

    export default {
        data() {
            return {
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
            this.countTime();
        },
        computed: {
            ...mapState({
                user: state => state.auth.user,
                conversations: state => state.chat.conversations,
            }),
        },
        watch: {
            selectedConversation: {
                handler(newValue) {
                    if (typeof newValue === 'object' && newValue !== null) {
                        this.$store.dispatch('getMessagesForConversation', newValue.id);
                    }
                },
                deep: true
            }
        },
        methods: {
            countTime() {
                this.currentTime = Math.trunc(new Date().getTime() / 1000);

                setTimeout(() => {
                    this.countTime();
                }, 1000);
            },
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
                this.$store.dispatch('getAllConversationsForUser');
            },
            async startConversation() {
                this.$store.dispatch('startNewConversation', this.newConversation)
                    .then(() => { }).catch(() => { }).then(() => this.newConversation.users = []);
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

<style scoped>
    .chat-conversation-window {
        max-height: 800px;
    }
</style>

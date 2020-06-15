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
            <template v-if="Array.isArray(conversations) && conversations.length > 0">
                <div v-for="(convo, index) in conversations" :key="index">
                    <chat-conversation :conversation="convo"></chat-conversation>
                </div>
            </template>
        </v-layout>
        
    </v-container>
</template>

<script>
    import Conversation from '../modules/chat-conversation'
    import chatService from '../../services/chat'
    import authService from '../../services/auth'

    export default {
        data() {
            return {
                conversations: [],
                newConversation: {
                    users: [],
                },
                userList: [],
                search: null,
                isLoading: false
            }
        },
        components: {
            'chat-conversation': Conversation,
        },
        created() {
            this.getAllUsers();
            this.getAllConversations();
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
                    }
                    else {
                        this.$_console_log('GetAllConversations: Data returned isn\'t an array');
                    }
                }).catch(() => this.$_console_log('GetAllConversations: Error getting conversation lists'));
            },
            async startConversation() {
                chatService.startConversation(this.newConversation).then(resp => {
                    if (typeof resp.data === 'object' && resp.data !== null) {
                        this.conversations.push(resp.data);
                    }
                    else {
                        this.$_console_log('StartConversation: Invalid data type returned');
                    }
                }).catch(() => this.$_console_log('StartConversation: Error starting conversation'))
                    .then(() => this.newConversation.users = []);
            }
        },
    }
</script>

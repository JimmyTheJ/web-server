<template>
    <v-container>
        <v-layout>
            <v-flex xs12>
                <div class="headline">Chat System</div>
            </v-flex>
            </v-layout>

        <v-layout row v-show="(isMobile && !hideMobile) || !isMobile">
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
            <v-flex xs12 md3 lg2 v-show="(isMobile && !hideMobile)|| !isMobile">
                <template v-if="Array.isArray(conversations) && conversations.length > 0">
                    <v-list shaped>
                        <v-list-item v-for="(convo, index) in conversations" :key="index" @click="selectedConversation = convo">
                            <v-list-item-icon>
                                <chat-badge :conversation="convo" />
                            </v-list-item-icon>
                            <v-list-item-content>
                                {{ convo.title }}
                            </v-list-item-content>
                        </v-list-item>
                    </v-list>
                </template>
            </v-flex>
            <v-flex xs12 md9 lg10 class="chat-conversation-window" v-if="(isMobile && hideMobile)|| !isMobile">
                <template v-for="(conversation, index) in conversations">
                    <chat-conversation :conversation="conversation"
                                       :time="currentTime"
                                       :show="shouldShowConversation(conversation)"
                                       :mobile="isMobile"
                                       @goBack="closeConversation" />
                </template>     
            </v-flex>
        </v-layout>
        
    </v-container>
</template>

<script>
    import Conversation from '../modules/chat/chat-conversation'
    import ChatBadge from '../modules/chat/chat-badge'

    import { mapState } from 'vuex';

    export default {
        data() {
            return {
                selectedConversation: null,
                newConversation: {
                    users: [],
                },
                search: null,
                isLoading: false,
                currentTime: null,
                isMobile: false,
                hideMobile: false,
            }
        },
        components: {
            'chat-conversation': Conversation,
            'chat-badge': ChatBadge,
        },
        created() {
            window.addEventListener('resize', this.updateScreenSize);
        },
        mounted() {
            this.countTime();
            this.isMobile = this.$vuetify.breakpoint.mobile;
        },
        beforeDestroy() {
            window.removeEventListener('resize', this.updateScreenSize);
        },
        computed: {
            ...mapState({
                user: state => state.auth.user,
                userList: state => state.auth.otherUsers,
                conversations: state => state.chat.conversations,
            }),
        },
        watch: {
            selectedConversation: {
                handler(newValue, oldValue) {
                    if (typeof newValue === 'object' && newValue !== null) {
                        if ((typeof oldValue === 'object' && oldValue !== null && newValue.id !== oldValue.id) || typeof oldValue !== 'object' || oldValue === null) {
                            this.$store.dispatch('getMessagesForConversation', newValue.id);
                        }                        
                    }

                    this.$_console_log('SelectedConversation', newValue);
                    if (newValue === null) {
                        this.hideMobile = false;
                    }
                    else {
                        this.hideMobile = true;
                    }
                },
                deep: true
            }
        },
        methods: {
            updateScreenSize() {
                this.$nextTick(() => {
                    if (this.$vuetify.breakpoint.mobile) {
                        this.isMobile = true;
                    }
                    else {
                        this.isMobile = false;
                    }
                });                
            },
            countTime() {
                this.currentTime = Math.trunc(new Date().getTime() / 1000);

                setTimeout(() => {
                    this.countTime();
                }, 1000);
            },
            closeConversation() {
                this.selectedConversation = null;
            },
            async getAllConversations() {
                this.$store.dispatch('getAllConversationsForUser').then(resp => {
                    const newMessages = this.conversation.messages.filter(x => (!Array.isArray(x.readReceipts) || x.readReceipts.length === 0) && x.userId !== this.user.id);
                    if (!Array.isArray(newMessages) || newMessages.length === 0) {
                        this.$_console_log('ReadAllMessages: NewMessages is null or empty');
                        return;
                    }
                });
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
            },
        },
    }
</script>

<style scoped>
    .chat-conversation-window {
        max-height: 800px;
    }
</style>

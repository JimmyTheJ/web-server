<template>
    <div>
        <main-menu :source="`home`"></main-menu>
        <router-view :key="$route.fullPath"></router-view>
        <notification-bar></notification-bar>
    </div>
</template>

<script>
    import Menu from './modules/menu'
    import NotificationBar from './modules/notification-bar'

    import { mapState } from 'vuex'

    export default {
        data() {
            return {

            }
        },
        components: {
            'main-menu': Menu,
            'notification-bar': NotificationBar,
        },
        computed: {
            ...mapState({
                user: state => state.auth.user,
                conversations: state => state.chat.conversations,
            })
        },
        created() {
            if (!this.$store.state.auth.isAuthorize)
                this.$router.push('/');

            if (this.$route.fullPath === '/home')
                this.$router.push({ name: 'start' });

            this.getUserConversations();
            this.$chatHub.$on('message-received', this.onMessageReceived);
        },
        beforeDestroy() {
            this.$chatHub.$off('message-received', this.onMessageReceived);
        },
        methods: {
            onMessageReceived(message) {
                if (typeof message !== 'object' || message === null) {
                    return;
                }

                let conversation = this.conversations.find(x => x.id === message.conversationId);
                if (message.userId === this.user.id || typeof conversation === 'undefined' || !Array.isArray(conversation.conversationUsers)
                        || typeof conversation.conversationUsers.find(x => x.userId === this.user.id) === 'undefined') {
                    return;
                }

                // Increment unread messages for this conversation as the message came from another user that was a member of your conversation
                this.$store.dispatch('incrementConversationUnreadMessageCount', conversation.id);
                this.$store.dispatch('pushNotification', { text: `You have new messages from ${conversation.title}`, type: 1, group: { type: 'chat', value: conversation.title } });
            },
            async getUserConversations() {
                // Get all conversations for this user
                await this.$store.dispatch('getAllConversationsForUser').then(resp1 => {
                    // Get all new messages for these conversations
                    this.$store.dispatch('getNewConversationNotifications')
                        .then(resp2 => {
                            this.$_console_log('New message notifications:', resp2.data);
                            this.buildNotificationList(resp2.data);
                        }).catch(() => this.$_console_log("No new message notifications"))
                })
            },
            buildNotificationList(data) {
                if (!Array.isArray(data) || data.length === 0) {
                    return;
                }

                data.forEach(value => {
                    if (Array.isArray(value.messages) && value.messages.length > 0) {
                        // There are new messages
                        const obj = {
                            text: `You have new messages from ${value.title}`,
                            type: 1,
                            group: {
                                type: 'chat',
                                value: value.title
                            }
                        }

                        this.$store.dispatch('pushNotification', obj);
                    }
                })
            },
        },
    }
</script>

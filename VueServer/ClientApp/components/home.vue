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

    export default {
        data() {
            return {

            }
        },
        components: {
            'main-menu': Menu,
            'notification-bar': NotificationBar,
        },
        created() {
            if (!this.$store.state.auth.isAuthorize)
                this.$router.push('/');

            if (this.$route.fullPath === '/home')
                this.$router.push({ name: 'start' });

            this.getUserConversations();
        },
        methods: {
            getUserConversations() {
                // Get all conversations for this user
                this.$store.dispatch('getAllConversationsForUser').then(resp1 => {
                    // Get all new messages for these conversations
                    this.$store.dispatch('getNewConversationNotifications')
                        .then(resp2 => {
                            this.$_console_log('New message notifications:', resp2.data);
                            this.buildNotificationList(resp2.data);
                        }).catch(() => ConMsgs.methods.$_console_log("No new message notifications"))
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
                            text: `New messages from ${value.title}`,
                            type: 1
                        }

                        this.$store.dispatch('pushNotification', obj);
                    }
                })
            },
        },
    }
</script>

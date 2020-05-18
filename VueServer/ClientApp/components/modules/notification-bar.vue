<template>
    <v-bottom-sheet v-model="sheet" inset dark>
        <v-sheet class="text-center" height="150px">
            <v-btn @click ="sheet = !sheet">Close</v-btn>

            <template v-for="message in messages">
                <v-banner>
                    <span @click="readMessage(message)">{{ message.text }}</span>
                    <template v-slot:actions>
                        <v-btn @click="deleteMessage(message)"><fa-icon icon="times"></fa-icon></v-btn>
                    </template>                    
                </v-banner>
            </template>
        </v-sheet>
    </v-bottom-sheet>
</template>

<script>
    import { mapState } from 'vuex'

    export default {
        data() {
            return {
                sheet: false,
                isLoading: false,
            }
        },
        computed: {
            ...mapState({
                numMessages: state => state.notifications.numMessages,
                messages: state => state.notifications.messages,
                opened: state => state.notifications.opened,
            }),
            numberOfMessagesText() {
                return `${this.numMessages} unread messages`;
            },
        },
        watch: {
            opened(newValue) {
                this.isLoading = true;
                this.sheet = newValue;

                setTimeout(() => {
                    this.isLoading = false;
                }, 5);
            },
            sheet(newValue) {
                if (!this.isLoading)
                    this.$store.dispatch('openNotifications', newValue)
            }
        },
        methods: {
            openMessages() {
                this.$_console_log('[Notification Bar] Open Messages!');
            },
            closeMessages() {
                this.$_console_log('[Notification Bar] Close Messages bar');
            },
            deleteMessage(item) {
                this.$_console_log(`[Notification Bar] Delete Message with id: ${item.id}`);

                this.$store.dispatch('popNotification', item.id);
            },
            readMessage(item) {
                this.$_console_log(`[Notification Bar] Read Message with id: ${item.id}`);
                
                this.$store.dispatch('readNotification', item.id);
            }
        }
    }
</script>

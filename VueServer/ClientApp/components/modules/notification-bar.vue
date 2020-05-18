<template>
    <v-bottom-sheet v-model="sheet" inset dark>
        <v-sheet class="text-center scrolling-y" height="160px">
            <v-btn @click ="sheet = !sheet">Close</v-btn>

            <div v-for="message in reversedMessages" :key="message.id" class="scroll-y">
                <div :class="[getMessageBackground(message.read), 'my-1']">
                    <v-banner :class="getMessageBorder(message.read)">
                        <span @click="readMessage(message)" :class="getTextColor(message.type)">{{ message.text }}</span>
                        <template v-slot:actions>
                            <fa-icon color="#555555" icon="times" @click="deleteMessage(message)" class="mr-2 mb-2"></fa-icon>
                        </template>
                    </v-banner>
                </div>
            </div>
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
                messages: state => state.notifications.messages,
                opened: state => state.notifications.opened,
            }),
            reversedMessages() {
                return this.messages.slice(0).reverse();
            }
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
            deleteMessage(item) {
                this.$_console_log(`[Notification Bar] Delete Message with id: ${item.id}`);

                this.$store.dispatch('popNotification', item.id);
            },
            readMessage(item) {
                this.$_console_log(`[Notification Bar] Read Message with id: ${item.id}`);
                
                this.$store.dispatch('readNotification', item.id);
            },
            getTextColor(type) {
                if (typeof type !== 'number') {
                    return 'white--text';
                }

                switch (type) {
                    case 0:
                        return 'green--text';
                    case 1:
                        return 'blue--text';
                    case 2:
                        return 'red--text';
                    default:
                        return 'white--text';
                }
            },
            getMessageBorder(isRead) {
                if (typeof isRead !== 'boolean' || isRead === false) {
                    return 'message-unread';
                }
                else {
                    return '';
                }                
            },
            getMessageBackground(isRead) {
                if (typeof isRead !== 'boolean' || isRead === false) {
                    return 'message-border-unread';
                }
                else {
                    return 'message-border-read';
                }
            }
        }
    }
</script>

<style scoped>
    .message-border-unread {
        border: 2px solid #929425 !important;
    }
    .message-border-read {
        border: 1px solid #fdfdfd !important;
    }

    .message-unread {
        background-color: #ffff00 !important;
    }

    .scrolling-y {
        overflow-y: scroll;
    }
</style>

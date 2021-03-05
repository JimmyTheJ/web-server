<template>
    <v-bottom-sheet v-model="sheet" inset dark>
        <v-sheet class="text-center scrolling-y" height="160px">
            <v-btn @click ="sheet = !sheet">Close</v-btn>

            <div v-for="message in reversedMessages" :key="message.id" class="scroll-y">
                <div :class="[getMessageBackground(message.read), 'my-1']">
                    <!--<v-banner :class="getMessageBorder(message.read)">-->
                    <v-banner>
                        <v-avatar v-if="messageHasGroupNum(message.group)" color="orange" size="40">
                            <span class="white--text headline">{{ message.group.num }}</span>
                        </v-avatar>
                        <span @click="readMessage(message)" :class="getTextColor(message.read)">{{ message.text }}</span>
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
                reversedMessages: [],
            }
        },
        computed: {
            ...mapState({
                messages: state => state.notifications.messages,
                opened: state => state.notifications.opened,
            }),
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
            },
            messages: {
                handler(newValue) {
                    this.$_console_log('Messages watcher:', newValue);
                    this.reversedMessages = newValue.slice(0).reverse();
                },
                deep: true
            },
        },
        created() {
            this.reversedMessages = this.messages.slice(0).reverse();
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
            getTextColor(isRead) {
                if (this.$vuetify.theme.dark === true) {
                    if (isRead)
                        return 'white--text';
                    else
                        return 'white--text font-weight-bold';
                }
                else {
                    if (isRead)
                        return 'black--text';
                    else
                        return 'black--text font-weight-bold'
                }
            },
            //getMessageBorder(isRead) {
            //    if (typeof isRead !== 'boolean' || isRead === false) {
            //        //return 'message-unread';
            //        return 'info accent-3'
            //    }
            //    else {
            //        return 'grey lighten-2';
            //    }                
            //},
            getMessageBackground(isRead) {
                if (this.$vuetify.theme.dark === true) {
                    if (isRead)
                        return 'grey darken-3';
                    else
                        return 'grey';
                }
                else {
                    if (isRead)
                        return 'grey lighten-3';
                    else
                        return 'grey'
                }
            },
            messageHasGroupNum(group) {
                if (group === null || group.num < 2) {
                    return false;
                }

                return true;
            },
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

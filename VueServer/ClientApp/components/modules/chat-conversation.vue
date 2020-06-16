<template>
    <v-container>
        <v-layout row ma-2 style="max-width: 1100px">
            <v-flex xs12 v-for="(message, index) in conversation.messages" :key="index" :class="getTextPosition(message)">
                <v-chip large :color="getTextColor(message)">
                    {{ message.text }}<br />
                    {{ getTimeSince(message.timestamp) }}
                </v-chip>
            </v-flex>
            <v-flex xs12 class="text-right">
                <v-text-field v-model="newMessage.text" label="Message"></v-text-field>
                <v-btn @click="sendMessage">Send</v-btn>
            </v-flex>
        </v-layout>
    </v-container>
    <!--<v-list>
        <v-list-item v-for="(message, index) in conversation.messages">
            <v-list-item-content>
                {{ message.text }}
            </v-list-item-content>
        </v-list-item>
    </v-list>
    <v-container>
        <v-layout row>
            <v-flex xs12>
                <v-card>
                    <v-card-title>
                        <template v-for="(user, index) in conversation.conversationUsers">
                            <span>{{ user.userId }}</span>
                            <span v-if="index < conversation.conversationUsers.length">, </span>
                        </template>
                    </v-card-title>
                    <template v-for="(message, index) in conversation.messages">
                        <template v-if="index === 0 || message.userId !== conversation.messages[index-1].userId">
                            <v-card-text :class="[getTextPosition(message), getTextColor(message)]">{{ message.userId }}</v-card-text>
                        </template>
                        <v-card-text :class="[getTextPosition(message), getTextColor(message)]">{{ message.text }}</v-card-text>
                    </template>
                    <v-card-text>
                        <v-text-field v-model="newMessage.text" label="Message"></v-text-field>
                    </v-card-text>
                    <v-card-text>
                        <v-btn @click="sendMessage">Send</v-btn>
                    </v-card-text>
                </v-card>
            </v-flex>
        </v-layout>
    </v-container>-->
</template>
<script>
    import service from '../../services/chat'

    function newMessage(conversationId, userId) {
        return {
            conversationId: conversationId,
            userId: userId,
            text: null
        }
    }

    export default {
        data() {
            return {
                newMessage: null,
            }
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
        },
        created() {
            this.newMessage = newMessage(this.conversation.id, this.$store.state.auth.user.id);
            this.$chatHub.$on('message-received', this.onMessageReceived)
        },
        beforeDestroy() {
            this.$chatHub.$off('message-received', this.onMessageReceived)
        },
        methods: {
            onMessageReceived(message) {
                if (typeof message !== 'object' || message === null) {
                    this.$_console_log('OnMessageReceived: Null object returned');
                    return;
                }

                if (this.conversation.id !== message.conversationId) {
                    return;
                }

                this.conversation.messages.push(Object.assign({}, message))
            },
            async sendMessage() {
                await service.sendMessage(this.newMessage);

                this.newMessage = newMessage(this.conversation.id, this.$store.state.auth.user.id);
            },
            getTimeSince(current) {
                let seconds = this.time - current;
                if (seconds < 60) {
                    return `${Math.trunc(seconds)}s`;
                }

                let minutes = seconds / 60;
                if (minutes < 60) {
                    return `${Math.trunc(minutes)}m`;
                }

                let hours = minutes / 60;
                if (hours < 24) {
                    return `${Math.trunc(hours)}h`;
                }

                let days = hours / 24;
                return `${Math.trunc(days)}d`;
            },
            getTextPosition(message) {
                if (message.userId === this.$store.state.auth.user.userName) {
                    return 'text-right';
                }
                else {
                    return 'text-left';
                }
            },
            getTextColor(message) {
                if (message.userId === this.$store.state.auth.user.userName) {
                    return 'blue';
                }
                else {
                    return 'green';
                }
            },
        }
    }
</script>

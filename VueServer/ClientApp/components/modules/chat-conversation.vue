<template>
    <v-container>
        <v-layout row>
            <v-flex xs12>
                <div v-for="(message, index) in conversation.messages" :key="index">
                    <v-flex xs3>{{ message.userId }}</v-flex>
                    <v-flex xs9>{{ message.text }}</v-flex>
                </div>
            </v-flex>
        </v-layout>

        <v-layout>
            <v-flex xs10>
                <v-text-field v-model="newMessage" label="Message"></v-text-field>
            </v-flex>
            <v-flex xs2>
                <v-btn @click="sendMessage">Send</v-btn>
            </v-flex>
        </v-layout>
    </v-container>
</template>

<script>
    import service from '../../services/chat'

    export default {
        data() {
            return {
                newMessage: null,
            }
        },
        props: {
            conversation: {
                type: Object,
                required: true
            }
        },
        created() {
            this.$chatHub.$on('message-received', this.onMessageReceived)
        },
        beforeDestroy() {
            this.$chatHub.$off('message-received', this.onMessageReceived)
        },
        methods: {
            onMessageReceived(message) {
                this.conversation.messages.push(Object.assign({}, message))
            },
            async sendMessage() {
                await service.sendMessage(this.newMessage);

                this.newMessage = null;
            },
        }
    }
</script>

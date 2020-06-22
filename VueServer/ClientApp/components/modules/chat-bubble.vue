<template>
    <div :class="['chat-container', getFlexDirection]">
        <v-menu absolute offset-y>
            <template v-slot:activator="{ on, attr }">
                <div v-on="on" class="order-2">
                    <span v-show="hover" class="pa-1" style="height: 100%; margin: 0 auto">...</span>
                </div>
            </template>
            <v-list>
                <v-list-item @click="moreInfo">
                    <v-list-item-title>
                        More info
                    </v-list-item-title>
                </v-list-item>
                <v-list-item v-if="isMessageDeletable" @click="deleteMessage">
                    <v-list-item-title>
                        Delete Message
                    </v-list-item-title>
                </v-list-item>
            </v-list>
        </v-menu>

        <div :class="['bubble-container', color, 'pa-2', 'order-1']">
            <div class="text-body-1">{{ message.text }}</div>
            <div class="text-caption text-right">{{ timeSince }}</div>
        </div>
    </div>
</template>

<script>
    import { mapState } from 'vuex'

    export default {
        data() {
            return {

            }
        },
        props: {
            message: {
                type: Object,
                required: true,
            },
            currentTime: {
                type: Number,
                required: false,
            },
            color: {
                type: String,
                required: false,
            },
            owner: {
                type: Boolean,
                required: true,
            },
            hover: {
                type: Boolean,
                required: false,
                default: false,
            }
        },
        computed: {
            ...mapState({
                user: state => state.auth.user,
                activeModules: state => state.auth.activeModules,
            }),
            timeSince() {
                if (typeof this.currentTime !== 'number' || typeof this.message.timestamp !== 'number') {
                    return '';
                }

                let seconds = this.currentTime - this.message.timestamp;
                if (seconds < 0) {
                    return '';
                }
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
            isMessageDeletable() {
                if (this.owner === true) {
                    const chatModule = this.activeModules.find(x => x.id === 'chat');
                    if (typeof chatModule !== 'undefined') {
                        const feature = chatModule.userModuleFeatures.find(x => x.moduleFeatureId === 'chat-delete-message')
                        if (typeof feature !== 'undefined') {
                            return true;
                        }
                    }
                }

                return false;
            },
            getFlexDirection() {
                if (this.owner) {
                    return 'flex-right';
                }
                else {
                    return 'flex-left';
                }
            },
        },
        methods: {
            deleteMessage() {
                this.$emit('deleteMessage', this.message.id);
                this.optionDialog = false;
            },
            moreInfo() {
                this.$emit('moreInfo', this.message);
                this.optionDialog = false;
            }
        }
    }
</script>

<style scoped>
    .chat-container {
        display: flex;
    }

    .bubble-container {
        border: 1px solid gray;
        border-radius: 15px;
        display: flex;
        flex-direction: column;
        max-width: 70%;
    }

    .flex-left {
        flex-direction: row;
    }

    .flex-right {
        flex-direction: row-reverse;
    }

    .order-1 {
        order: 1;
    }

    .order-2 {
        order: 2;
    }

    .green {
        background-color: green;
    }

    .blue {
        background-color: blue;
    }
</style>

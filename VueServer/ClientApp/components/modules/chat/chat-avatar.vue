<template>
    <div>
        Dawg
    </div>
</template>

<template>
    <div>
        <template v-if="conversation.conversationUsers.length > 2">
            <fa-icon icon="users" size="2x"></fa-icon>
        </template>
        <template v-else-if="friendHasAvatar(conversation) !== false">
            <v-avatar>
                <v-img :src="friendHasAvatar(conversation)"></v-img>
            </v-avatar>
        </template>
        <template v-else>
            <v-avatar :color="getFriendColor(conversation)">
                <span class="white--text headline">{{ getFriendAvatarText(conversation) }}</span>
            </v-avatar>
        </template>
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
            conversation: {
                type: Object,
                required: true,
            }
        },
        computed: {
            ...mapState({
                user: state => state.auth.user,
                userList: state => state.auth.otherUsers,
            }),
        },
        methods: {
            friendHasAvatar(conversation) {
                if (typeof conversation !== 'object' || conversation === null || !Array.isArray(conversation.conversationUsers)) {
                    return false;
                }

                if (!Array.isArray(this.userList)) {
                    return false;
                }

                var friend = conversation.conversationUsers.find(x => x.userId !== this.user.id);
                if (typeof friend === 'undefined') {
                    return false;
                }

                var user = this.userList.find(x => x.id === friend.userId);
                if (typeof user === 'undefined') {
                    return false;
                }

                if (typeof user.avatar === 'undefined') {
                    return false;
                }

                return `${process.env.API_URL}/public/${user.avatar}`;
            },
            getFriendColor(conversation) {
                const defaultColor = 'blue';
                if (typeof conversation !== 'object' || conversation === null || !Array.isArray(conversation.conversationUsers)) {
                    return defaultColor;
                }

                var friend = conversation.conversationUsers.find(x => x.userId !== this.user.id);
                if (typeof friend === 'undefined') {
                    return defaultColor;
                }

                if (friend.color === null) {
                    return defaultColor;
                }

                return friend.color;
            },
            getFriendAvatarText(conversation) {
                if (typeof conversation !== 'object' || conversation === null || !Array.isArray(conversation.conversationUsers)) {
                    return false;
                }

                if (!Array.isArray(this.userList)) {
                    return false;
                }

                var friend = conversation.conversationUsers.find(x => x.userId !== this.user.id);
                if (typeof friend === 'undefined') {
                    return false;
                }

                var user = this.userList.find(x => x.id === friend.userId);
                if (typeof user === 'undefined') {
                    return false;
                }

                return user.displayName.charAt(0);
            },
        },
    }
</script>

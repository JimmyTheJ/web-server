<template>
    <v-tooltip bottom color="grey lighten-3" open-delay="400">
        <span slot="activator">{{ value }}</span>
        <v-flex class="tooltip">
            <slot>{{ value }}</slot>
        </v-flex>
    </v-tooltip>
</template>

<script>
    export default {
        data() {
            return {

            }
        },
        props: {
            value: {
                type: String,
                required: true,
            }
        }
    }
</script>

<style>
    .tooltip {
        color: #424242;
    }
</style>

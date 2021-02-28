<template>
    <v-tooltip bottom color="grey lighten-3" open-delay="400">
        <template v-slot:activator="{ on }">
            <span v-on="on">{{ value }}</span>
        </template>
        
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

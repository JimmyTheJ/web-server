<template>
    <div class="text-xs-center">
        <h1 class="orange--text text--darken-3 display-2 mt-3">Welcome to {{ name }}</h1>
    </div>
</template>

<script>
    import { ServerName } from '../../constants'

    export default {
        data() {
            return {
                name: ServerName,
            }
        }
    }
</script>

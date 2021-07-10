<template>
  <div class="text-xs-center">
    <h1 class="orange--text text--darken-3 display-2 mt-3">
      Welcome to {{ name }}
    </h1>
  </div>
</template>

<script>
const serverName = process.env.VUE_APP_TITLE

export default {
  data() {
    return {
      name: serverName,
    }
  },
}
</script>

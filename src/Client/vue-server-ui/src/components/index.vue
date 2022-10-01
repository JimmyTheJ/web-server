<template>
  <div class="main-container">
    <h1 class="text-xs-center">{{ name }}</h1>
    <router-view></router-view>
  </div>
</template>

<script>
const FN = 'index'

import Auth from '@/mixins/authentication'

export default {
  data() {
    return {
      env: process.env.NODE_ENV,
      name: process.env.VUE_APP_TITLE,
    }
  },
  mixins: [Auth],
  async created() {
    await this.$store.dispatch('getEnabledModules')
    this.$router.push({ name: 'login' })
  },
}
</script>

<template>
  <div class="main-container">
    <h1 class="text-xs-center">{{ name }}</h1>
    <router-view></router-view>
  </div>
</template>

<script>
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
    this.$store.dispatch('getEnabledModules')

    let result = await this.$_auth_checkLogin(false)
    if (!result) {
      //if (this.$route.fullPath === '/') this.$router.push({ name: 'login' })
      this.$router.push({ name: 'login' })
    }
    else {
      
    }
    
  },
}
</script>

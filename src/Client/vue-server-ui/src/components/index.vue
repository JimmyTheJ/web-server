<template>
  <div class="main-container">
    <h1 class="text-xs-center">{{ name }}</h1>
    <router-view></router-view>
  </div>
</template>

<script>
export default {
  data() {
    return {
      env: process.env.NODE_ENV,
      name: process.env.VUE_APP_TITLE,
    }
  },
  created() {
    if (this.$route.fullPath === '/') this.$router.push({ name: 'login' })
  },
}
</script>

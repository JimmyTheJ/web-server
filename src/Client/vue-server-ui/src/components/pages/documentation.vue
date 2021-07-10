<template>
  <div>
    <v-layout row>
      <v-flex xs4 sm3 md2 lg1 class="left-menu">
        <v-layout column>
          <template v-for="(o, i) in obj">
            <router-link
              :to="{ name: 'documentation-item', params: { id: o } }"
              >{{ o }}</router-link
            >
          </template>
        </v-layout>
      </v-flex>

      <v-flex xs8 sm9 md10 lg11>
        <router-view></router-view>
      </v-flex>
    </v-layout>
  </div>
</template>

<script>
import Documentation from '../../documentation.json'
let doc = JSON.parse(JSON.stringify(Documentation))

export default {
  data() {
    return {
      obj: [],
    }
  },
  created() {
    this.obj = doc.map(x => x.id).slice(0)
  },
}
</script>

<style>
.left-menu {
  display: flex;
  /*flex-direction: column;
        flex-grow: 1;*/
  position: relative;
  top: 15px;
  width: 160px;
  height: 1000px;
  font-weight: bold;
  font-size: 2rem;
  overflow-y: hidden;
  margin-left: 10px;
  /*margin: 5px 2px 0 2px;*/
}
</style>

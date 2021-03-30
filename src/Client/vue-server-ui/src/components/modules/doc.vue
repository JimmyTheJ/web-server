<template>
  <div>
    <h1>{{ obj.title }}</h1>
    <h3>{{ obj.topic }}</h3>
    <template v-for="(section, i) in obj.sections">
      <div class="title">{{ section.title }}</div>
      <template v-for="(paragraph, j) in section.paragraphs">
        <p :class="paragraph.style">
          {{ paragraph.text }}
        </p>
      </template>
    </template>
  </div>
</template>

<script>
import Documentation from '../../documentation.json'
let doc = JSON.parse(JSON.stringify(Documentation))

export default {
  data() {
    return {
      obj: {},
    }
  },
  created() {
    this.obj = Object.assign(
      {},
      doc.filter(x => x.id === this.$route.params.id)[0]
    )
  },
}
</script>

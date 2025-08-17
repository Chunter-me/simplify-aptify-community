<template>
  <MDCRenderer v-if="parsed && parsed.body && parsed.data" :body="parsed.body" :data="parsed.data" />
</template>


<script setup lang="ts">
  const { markdown, mdc } = defineProps<{ markdown?: string, mdc?: any }>();

  const parsed = ref<any>();

  if (mdc && mdc.body && mdc.data) {
    parsed.value = mdc;
  }
  else if (!(mdc && mdc.body && mdc.data) && markdown) {
    parsed.value = await useMarkdownParser().parseMarkdown(markdown);
  }
</script>

<template>
  <section v-if="show" class="loading-wrapper">
    <div class="loading-spinner" />
    <div v-if="message" class="loading-message" v-html="message" />
  </section>
</template>

<script setup lang="ts">
  const { show, message } = usePageLoader();
</script>
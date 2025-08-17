<template>
  <UPopover v-if="label" mode="hover" :close-delay="200">
    <span class="cursor-help border-b border-dashed border-red-600 dark:border-red-400" v-html="label" />

    <template #content>
      <slot />
    </template>
  </UPopover>
</template>

<script setup lang="ts">
  const { label = '' } = defineProps<{ label: string }>();
</script>
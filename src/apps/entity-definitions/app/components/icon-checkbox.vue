<template>
  <span class="inline-flex items-center gap-2">
    <UIcon size="16"
      :name="checked ? 'fa6-regular:square-check' : 'fa6-regular:square'"
      :class="checked && !checkedWhite ? 'text-red-600 dark:text-red-400' : null"
    />

    <span v-if="label" v-html="label" />
  </span>
</template>


<script setup lang="ts">
  const { checked = false, label = null, checkedWhite = false } = defineProps<{ checked: boolean | number, label?: string | null, checkedWhite?: boolean | null }>();
</script>
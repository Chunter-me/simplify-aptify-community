<template>
  <strong v-if="label" class="flex items-center gap-2 font-semibold">
    <UIcon v-if="icon" :name="icon" />
    <span>{{ label }}</span>
  </strong>

  <span v-if="description" class="block group-hover:text-gray-200 group-hover:dark:text-muted dark:text-muted">{{ description }}</span>
</template>

<script setup lang="ts">
  const { label, description, icon } = defineProps<{ label?: string, description?: string, icon?: string }>();
</script>
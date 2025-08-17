<template>
  <div class="w-full flex items-center gap-3">
    <div class="grow"><slot>{{ label ?? column.columnDef.header ?? '' }}</slot></div>
    <BtnSort :direction="column.getIsSorted()" @click="tableColumnSortClick(column, $event)" />
  </div>
</template>

<script setup lang="ts">
  const { column, label } = defineProps<{ column: any, label?: string }>();
</script>
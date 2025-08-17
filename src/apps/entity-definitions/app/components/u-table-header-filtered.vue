<template>
  <UTableHeaderSort :column :label>
    <UInput v-model="keyword"
      type="search"
      :placeholder="label ?? column.columnDef.header ?? ''"
      size="xs"
      class="w-full"
    />
  </UTableHeaderSort>
</template>

<script setup lang="ts">
  const { column, label } = defineProps<{ column: any, label?: string }>();
  const emits = defineEmits([ 'filter' ])

  const keyword = ref<string>('');
  const { filterKeyword } = useKeywordTrigger(keyword);

  watch(filterKeyword, () => {
    emits('filter', filterKeyword.value)
  });
</script>
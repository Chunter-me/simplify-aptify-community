<template>
  <UTable v-if="allowRowSelection" as="article" sticky :data :columns @select="onRowSelect" />
  <UTable v-else as="article" sticky :data :columns :ui="{ tbody: '[&>tr]:hover:bg-red-600/75 [&>tr]:dark:hover:bg-red-600/25' }" />
</template>

<script setup lang="ts">
  import type { TableColumn, TableRow } from '@nuxt/ui';

  const { columns, data, allowRowSelection = true } = defineProps<{ columns: Array<TableColumn<any>>, data: Array<any>, allowRowSelection?: boolean }>();
  const emits = defineEmits([ 'update:selectedData', 'click-row' ]);

  const onRowSelect = (tblRow: TableRow<any>) => {
    emits('click-row', tblRow.original);
    emits('update:selectedData', tblRow.original);
  };
</script>
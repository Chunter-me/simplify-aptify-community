<template>
  <EntityDefinitionTable :columns :data="entityDefTableFiltered" :allow-row-selection="false" />
</template>


<script setup lang="ts">
  import { h } from 'vue';
  import type { TableColumn } from '@nuxt/ui';
  import {
    UTableHeaderFiltered,
    UTableHeaderSort,
    UTableCellLabelDesc,
    IconCheckbox
  } from '#components';

  const { entityDefReports } = useEntityDefinition();
  const { entityDefTableFiltered, entityDefTableFilterData } = useEntityDefTable(entityDefReports.value, [ 'Name', 'Description', 'Format' ]);

  const columns: TableColumn<any>[] = [
    {
      accessorKey: 'Name',
      header: ({ column }) => h(UTableHeaderFiltered, { column: column, label: 'Report', onFilter: entityDefTableFilterData }),
      cell: ({ row }) => h(UTableCellLabelDesc, { label: row.original.Name, description: row.original.Description })
    },
    {
      accessorKey: 'ReportFormat',
      header: ({ column }) => h(UTableHeaderSort, { column: column, label: 'Format' })
    },
    {
      accessorKey: 'Category',
      header: ({ column }) => h(UTableHeaderSort, { column: column, label: 'Category' })
    },
    {
      accessorKey: 'Status',
      header: ({ column }) => h(UTableHeaderSort, { column: column, label: 'Is Active' }),
      cell: ({ row }) => h(IconCheckbox, { checked: row.original.Status == 'Active' }),
      meta: uTableHeaderMetaCenter
    },
    {
      accessorKey: 'Object',
      header: ({ column }) => h(UTableHeaderSort, { column: column, label: 'Object' })
    }
  ];
</script>
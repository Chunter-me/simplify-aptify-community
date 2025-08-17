<template>
  <EntityDefinitionTable :columns :data="entityDefTableFiltered" v-model:selected-data="selectedData" />
  <EntityDefinitionPartFieldDetails v-model="selectedData" />
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

  const { entityDefFields } = useEntityDefinition();
  const { entityDefTableFiltered, entityDefTableFilterData } = useEntityDefTable(entityDefFields.value);

  const columns: TableColumn<any>[] = [
    {
      accessorKey: 'DisplayName',
      header: ({ column }) => h(UTableHeaderFiltered, { column: column, label: 'Field', onFilter: entityDefTableFilterData }),
      cell: ({ row }) => h(UTableCellLabelDesc, { label: row.original.DisplayName, description: row.original.Name }),
    },
    {
      accessorKey: 'SQLTypeLabel',
      header: ({ column }) => h(UTableHeaderSort, { column: column, label: 'Type' }),
      meta: uTableHeaderMetaCenter
    },
    {
      accessorKey: 'IsRequired',
      header: ({ column }) => h(UTableHeaderSort, { column: column, label: 'Is Required' }),
      cell: ({ row }) => h(IconCheckbox, { checked: row.original.IsRequired }),
      meta: uTableHeaderMetaCenter
    },
    {
      accessorKey: 'IsVirtual',
      header: ({ column }) => h(UTableHeaderSort, { column: column, label: 'Is Virtual' }),
      cell: ({ row }) => h(IconCheckbox, { checked: row.original.IsVirtual }),
      meta: uTableHeaderMetaCenter
    }
  ];

  const selectedData = ref<any>();
</script>
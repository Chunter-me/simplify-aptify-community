<template>
  <EntityDefinitionTable :columns :data="entityDefTableFiltered" v-model:selected-data="selectedData" />
  <EntityDefinitionPartViewDetails v-model="selectedData" />
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

  const { entityDefViews } = useEntityDefinition();
  const { entityDefTableFiltered, entityDefTableFilterData } = useEntityDefTable(entityDefViews.value, [ 'Name', 'DisplayName', 'BaseView' ]);

  const columns: TableColumn<any>[] = [
    {
      accessorKey: 'Name',
      header: ({ column }) => h(UTableHeaderFiltered, { column: column, label: 'Views', onFilter: entityDefTableFilterData }),
      cell: ({ row }) => h(UTableCellLabelDesc, { label: row.original.Name, description: row.original.BaseView, icon: row.original.ViewTypeIconHTMLClass }),
    },
    {
      accessorKey: 'IsShared',
      header: ({ column }) => h(UTableHeaderSort, { column: column, label: 'Is Shared' }),
      cell: ({ row }) => h(IconCheckbox, { checked: row.original.IsShared }),
      meta: uTableHeaderMetaCenter
    },
    {
      accessorKey: 'OwnerID',
      header: ({ column }) => h(UTableHeaderSort, { column: column, label: 'Owner' }),
    },
    {
      accessorKey: 'IsMine',
      header: ({ column }) => h(UTableHeaderSort, { column: column, label: 'Is Mine' }),
      cell: ({ row }) => h(IconCheckbox, { checked: row.original.IsMine }),
      meta: uTableHeaderMetaCenter
    },
    {
      accessorKey: 'Description',
      header: 'Description',
      meta: {
        class: {
          td: '!whitespace-normal min-w-[600px]'
        }
      }
    }
  ];

  const selectedData = ref<any>(null);
</script>
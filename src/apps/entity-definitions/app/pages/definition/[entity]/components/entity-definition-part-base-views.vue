<template>
  <EntityDefinitionTable :columns :data="entityDefTableFiltered" v-model:selected-data="selectedData" />
  <EntityDefinitionPartBaseViewDetails v-model="selectedData" />
</template>


<script setup lang="ts">
  import { h } from 'vue';
  import type { TableColumn } from '@nuxt/ui';
  import {
    UTableHeaderFiltered,
    UTableHeaderSort,
    UTableCellLabelDesc
  } from '#components';

  const { entityDefBaseViews } = useEntityDefinition();
  const { entityDefTableFiltered, entityDefTableFilterData } = useEntityDefTable(entityDefBaseViews.value);

  const columns: TableColumn<any>[] = [
    {
      accessorKey: 'DisplayName',
      header: ({ column }) => h(UTableHeaderFiltered, { column: column, label: 'Base View', onFilter: entityDefTableFilterData }),
      cell: ({ row }) => h(UTableCellLabelDesc, { label: row.original.DisplayName, description: row.original.Name }),
    },
    {
      accessorKey: 'Fields',
      header: ({ column }) => h(UTableHeaderSort, { column: column, label: 'Fields' }),
      cell: ({ row }) => row.original.Fields.length,
      meta: uTableHeaderMetaCenter
    },
    {
      accessorKey: 'Description',
      header: 'Description',
      meta: {
        class: {
          td: '!whitespace-normal'
        }
      }
    }
  ];

  const selectedData = ref<any>();
</script>
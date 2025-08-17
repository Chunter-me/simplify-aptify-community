<template>
  <EntityDefinitionTable :columns :data="entityDefTableFiltered" @click-row="clickRow" />
  <EntityDefinitionPartPipelinesDetails v-model="selectedData" />
</template>


<script setup lang="ts">
  import type { TableColumn } from '@nuxt/ui';
  import { h } from 'vue';
  import {
    UTableHeaderFiltered,
    UTableHeaderSort,
    UTableCellLabelDesc,
    IconCheckbox
  } from '#components';

  const { entityDefPipeline } = useEntityDefinition();
  const { entityDefTableFiltered, entityDefTableFilterData } = useEntityDefTable(entityDefPipeline.value, [ 'DefinitionName', 'ProcessFlowName' ]);

  const columns: TableColumn<any>[] = [
    {
      accessorKey: 'DefinitionName',
      header: ({ column }) => h(UTableHeaderFiltered, { column: column, label: 'Pipelines', onFilter: entityDefTableFilterData }),
      cell: ({ row }) => h(UTableCellLabelDesc, { label: row.original.DefinitionName, description: row.original.DefinitionDescription }),
      meta: {
        class: {
          td: '!whitespace-normal !min-w-[360px]'
        }
      }
    },
    {
      accessorKey: 'EventSequence',
      header: ({ column }) => h(UTableHeaderSort, { column: column, label: 'Sequence' }),
      meta: uTableHeaderMetaCenter
    },
    {
      accessorKey: 'EventIsActive',
      header: ({ column }) => h(UTableHeaderSort, { column: column, label: 'Is Active' }),
      cell: ({ row }) => h(IconCheckbox, { checked: row.original.EventIsActive }),
      meta: uTableHeaderMetaCenter
    },
    {
      accessorKey: 'ProcessFlowName',
      header: ({ column }) => h(UTableHeaderSort, { column: column, label: 'Process Flow' }),
      cell: ({ row }) => h(UTableCellLabelDesc, { label: row.original.ProcessFlowName, description: row.original.ProcessFlowDescription }),meta: {
        class: {
          td: '!whitespace-normal !min-w-[600px]'
        }
      }
    }
  ];

  const selectedData = ref<any>();
  const clickRow = (row: any) => {
    selectedData.value = row;
  };
</script>
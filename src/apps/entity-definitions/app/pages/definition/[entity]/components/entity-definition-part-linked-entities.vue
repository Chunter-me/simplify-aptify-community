<template>
  <EntityDefinitionTable :columns :data="entityDefTableFiltered" v-model:selected-data="selectedData" />
</template>


<script setup lang="ts">
  import { h } from 'vue';
  import type { TableColumn } from '@nuxt/ui';
  import {
    UTableHeaderFiltered,
    UTableHeaderSort
  } from '#components';

  const { entityDefLinkedEntities, entityDefEntity } = useEntityDefinition();
  const { entityDefTableFiltered, entityDefTableFilterData } = useEntityDefTable(entityDefLinkedEntities.value, [ 'LinkedEntityField', 'Entity', 'Name' ]);

  const columns: TableColumn<any>[] = [
    {
      accessorKey: 'EntityField',
      header: ({ column }) => h(UTableHeaderFiltered, { column: column, label: 'Links', onFilter: entityDefTableFilterData }),
      cell: ({ row }) => `${entityDefEntity.value.Name}.${row.original.LinkedEntityField}`,
      meta: {
        class: {
          th: 'min-w-[260px] whitespace-nowrap'
        }
      }
    },
    {
      accessorKey: 'LinkedEntity',
      header: ({ column }) => h(UTableHeaderSort, { column: column, label: 'Links To' }),
      cell: ({ row }) => `${row.original.Entity}.${row.original.Name}`,
      meta: {
        class: {
          th: 'whitespace-nowrap'
        }
      }
    },
    {
      accessorKey: 'Description',
      header: 'Description'
    }
  ];

  const selectedData = ref<any>();
  watch(selectedData, () => {
    if (selectedData.value) {
      useRouter().push(`/definition/${selectedData.value.Entity.toLowerCase().replaceAll(' ', '-')}/overview`);
    }
  });
</script>
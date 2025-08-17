<template>
  <EntityDefinitionTable :columns :data="entityDefTableFiltered" :allow-row-selection="false" />
</template>


<script setup lang="ts">
  import { h } from 'vue';
  import type { TableColumn } from '@nuxt/ui';
  import {
    UTableHeaderFiltered,
    UTableHeaderSort
  } from '#components';

  const { entityDefPlugins } = useEntityDefinition();
  const { entityDefTableFiltered, entityDefTableFilterData } = useEntityDefTable(entityDefPlugins.value, [ 'PlugInTypeText', 'ObjectName', 'AssemblyName', 'ClassName' ]);

  const columns: TableColumn<any>[] = [
    {
      accessorKey: 'PlugInTypeText',
      header: ({ column }) => h(UTableHeaderFiltered, { column: column, label: 'Plugin', onFilter: entityDefTableFilterData })
    },
    {
      accessorKey: 'ObjectName',
      header: ({ column }) => h(UTableHeaderSort, { column: column, label: 'Object' })
    },
    {
      accessorKey: 'AssemblyName',
      header: ({ column }) => h(UTableHeaderSort, { column: column, label: 'Assembly' })
    },
    {
      accessorKey: 'ClassName',
      header: ({ column }) => h(UTableHeaderSort, { column: column, label: 'Class' })
    },
  ];
</script>
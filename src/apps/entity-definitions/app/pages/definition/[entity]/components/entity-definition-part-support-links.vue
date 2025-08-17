<template>
  <EntityDefinitionTable :columns :data="entityDefTableFiltered" />
</template>


<script setup lang="ts">
  import { h } from 'vue';
  import type { TableColumn } from '@nuxt/ui';
  import { Markdown } from '#components';

  const { entityDefHelpLinks } = useEntityDefinition();
  const { entityDefTableFiltered } = useEntityDefTable(entityDefHelpLinks.value);

  const columns: TableColumn<any>[] = [
    {
      accessorKey: 'Title',
      header: 'Link',
      cell: ({ row }) => h('a', { href: row.original.Url, target: '_blank' }, [ row.original.Title ]),
    },
    {
      accessorKey: 'Description',
      header: 'Description',
      cell: ({ row }) => h(Markdown, { markdown: row.original.Description }),
    }
  ];
</script>
<template>
  <EntityDefinitionTable :columns :data="entityDefTableFiltered" v-model:selected-data="selectedData" />
</template>


<script setup lang="ts">
  import { h } from 'vue';
  import type { TableColumn } from '@nuxt/ui';
  import { UTableHeaderFiltered } from '#components';

  const { entityDefSubTypes } = useEntityDefinition();
  const { entityDefTableFiltered, entityDefTableFilterData } = useEntityDefTable(entityDefSubTypes.value);

  const columns: TableColumn<any>[] = [
    {
      accessorKey: 'DisplayName',
      header: ({ column }) => h(UTableHeaderFiltered, { column: column, label: 'Sub-Type', onFilter: entityDefTableFilterData }),
      meta: {
        class: {
          td: 'min-w-[280px]'
        }
      }
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
  watch(selectedData, () => {
    if (selectedData.value) {
      useRouter().push(`/definition/${selectedData.value.Name.toLowerCase().replaceAll(' ', '-')}/overview`);
    }
  });
</script>
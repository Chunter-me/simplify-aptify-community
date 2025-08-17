<template>
  <EntityDefinitionTable :columns :data="entityDefTableFiltered" v-model:selected-data="selectedData" />

  <EntityDefinitionDrawer v-model="open">
    <template #header>
      Filter Relation
    </template>

    <div class="min-w-[400px]">
      <Markdown :markdown="markdownWrapSqlCode(selectedData?.DboSql)" />
    </div>

  </EntityDefinitionDrawer>
</template>


<script setup lang="ts">
  import { h } from 'vue';
  import type { TableColumn } from '@nuxt/ui';
  import { UTableHeaderFiltered } from '#components';

  const { entityDefFilterRelations } = useEntityDefinition();
  const { entityDefTableFiltered, entityDefTableFilterData } = useEntityDefTable(entityDefFilterRelations.value, [ 'RelationTitle' ]);

  const columns: TableColumn<any>[] = [
    {
      accessorKey: 'RelationTitle',
      header: ({ column }) => h(UTableHeaderFiltered, { column: column, label: 'Relation', onFilter: entityDefTableFilterData }),
    },
    {
      accessorKey: 'Description',
      header: 'Description'
    }
  ];

  const selectedData = ref<any>(null);
  const open = ref<boolean>(false);

  watch(selectedData, () => {
    if (!selectedData.value) {
      open.value = false;
      return;
    }

    open.value = true;
  });

  watch(open, () => {
    if (!open.value) {
      selectedData.value = null;
    }
  });
</script>
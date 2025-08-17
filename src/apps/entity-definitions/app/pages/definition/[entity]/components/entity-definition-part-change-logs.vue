<template>
  <EntityDefinitionTable :columns :data="entityDefTableFiltered" v-model:selected-data="selectedData" />
  <EntityDefinitionDrawer v-model="open">
    <template #header>Changes</template>

    <div v-if="selectedData" class="size-full flex flex-col gap-6 overflow-hidden">
      <section class="shrink-0 grid grid-cols-2 gap-4">
        <div>
          <p class="text-muted">Date Updated</p>
          {{ selectedData.DateUpdated }}
        </div>

        <div>
          <p class="text-muted">Who Updated</p>
          {{ selectedData.WhoUpdated }}
        </div>
      </section>

      <div class="grow overflow-y-auto">
        <Markdown :markdown="markdownWrapFormalXmlCode(selectedData.Changes)" />
      </div>
    </div>
  </EntityDefinitionDrawer>
</template>


<script setup lang="ts">
  import { h } from 'vue';
  import type { TableColumn } from '@nuxt/ui';
  import { UTableHeaderSort } from '#components';

  const { entityDefRecordHistory } = useEntityDefinition();
  const { entityDefTableFiltered } = useEntityDefTable(entityDefRecordHistory.value);

  const columns: TableColumn<any>[] = [
    {
      accessorKey: 'DateUpdated',
      header: ({ column }) => h(UTableHeaderSort, { column: column, label: 'Date Updated' }),
      meta: {
        class: {
          th: 'whitespace-nowrap',
          td: 'whitespace-nowrap'
        }
      }
    },
    {
      accessorKey: 'WhoUpdated',
      header: ({ column }) => h(UTableHeaderSort, { column: column, label: 'Who Updated' }),
      meta: {
        class: {
          th: 'whitespace-nowrap',
          td: 'whitespace-nowrap'
        }
      }
    },
    {
      accessorKey: 'Changes',
      header: 'Changes'
    }
  ];

  const open = ref<boolean>(false);
  const selectedData = ref<any>();

  watch(selectedData, () => {
    if (selectedData.value) {
      open.value = true;
    }
  });

  watch(open, () => {
    if (!open.value) {
      selectedData.value = null;
    }
  });
</script>
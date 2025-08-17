<template>
  <EntityDefinitionTable :columns :data="entityDefTableFiltered" v-model:selected-data="selectedData" />
  <EntityDefinitionDrawer v-model="open">
    <template #header>{{ selectedData.AttributeName }}</template>

    <div v-if="selectedData" class="size-full flex flex-col overflow-hidden">
      <section class="shrink-0">
        {{ selectedData.Description }}
      </section>

      <strong class="block mt-6 text-muted font-semibold">Value</strong>
      <div class="grow overflow-y-auto">
        <Markdown :markdown="formattedValue" />
      </div>
    </div>
  </EntityDefinitionDrawer>
</template>


<script setup lang="ts">
  import { h } from 'vue';
  import type { TableColumn } from '@nuxt/ui';
  import { UTableHeaderFiltered } from '#components';

  const { entityDefAttributes } = useEntityDefinition();
  const { entityDefTableFiltered, entityDefTableFilterData } = useEntityDefTable(entityDefAttributes.value);

  const columns: TableColumn<any>[] = [
    {
      accessorKey: 'AttributeName',
      header: ({ column }) => h(UTableHeaderFiltered, { column: column, label: 'Attribute', onFilter: entityDefTableFilterData }),
      meta: {
        class: {
          th: 'whitespace-nowrap',
          td: 'whitespace-nowrap'
        }
      }
    },
    {
      accessorKey: 'AttributeValue',
      header: 'Value',
      cell: ({ row }) => h('div', { class: 'w-[300px] overflow-hidden text-ellipsis' }, row.original.AttributeValue),
      meta: {
        class: {
          th: 'whitespace-nowrap',
          td: 'whitespace-nowrap'
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

  const open = ref<boolean>(false);
  const selectedData = ref<any>();
  const formattedValue = ref<string>('');

  watch(selectedData, () => {
    if (selectedData.value) {
      formattedValue.value = selectedData.value.AttributeValue.startsWith('<')
        ? markdownWrapFormalXmlCode(selectedData.value.AttributeValue)
        : selectedData.value.AttributeValue;

      open.value = true;
    }
  });

  watch(open, () => {
    if (!open.value) {
      formattedValue.value = '';
    }
  });
</script>
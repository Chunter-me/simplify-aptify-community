<template>
  <UTable sticky v-if="tableData && tableData.length > 0" :data="filteredTableData" :columns="tableColumns">
    <template #key-header>
      <div class="w-full flex items-center gap-2">
        <UInput v-model="keyword"
          type="search"
          placeholder="Property"
          size="xs"
          class="w-full"
        />
      </div>
    </template>

    <template #value-cell="{ row }">
      <IconCheckbox v-if="row.original.value.toLowerCase() == 'true' || row.original.value.toLowerCase() == 'false'"
        :checked="row.original.value == 'true'"
        :checkedWhite="true"
      />
      <template v-else>{{ row.original.value }}</template>
    </template>
  </UTable>
</template>

<script setup lang="ts">
  import type { TableColumn } from '@nuxt/ui';

  const { data } = defineProps<{ data?: any }>();
  //logDev('All Props Data: ', data);

  const tableData = ref<Array<{key: string, value: string}>>([]);
  const keyword = ref<string>('');

  if (data) {
    tableData.value = objectToPropsKeyValue(data);
  }

  const tableColumns: TableColumn<any>[] = [
    {
      accessorKey: 'key',
      header: 'Property',
    },
    {
      accessorKey: 'value',
      header: 'Value'
    }
  ];

  const filteredTableData = computed(() => {
    const filter = keyword.value.toLowerCase().trim();
    if (tableData.value && tableData.value.length > 0 && filter.length > 2) {
      return tableData.value?.filter((prop: {key: string, value: string}) => {
        return prop.key.toLowerCase().includes(filter);
      });
    }

    return tableData.value;
  });
</script>
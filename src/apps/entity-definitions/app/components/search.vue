<template>
  <UModal v-model:open="open"
    :ui="{
      overlay: '',
      content: 'size-[calc(100%-80px)] !max-w-[1200px] !max-h-[600px] mx-auto border dark:border-zinc-500'
    }"
  >
    <UButton
      icon="fa6-solid:magnifying-glass"
      color="neutral"
      variant="outline"
      class="shrink-0 h-[34px] w-full max-w-[360px] py-0 px-2.5 flex items-center justify-start gap-2"
    >
      <span class="grow !text-left">Search</span>
      <UKbd size="sm" class="shrink-0">Ctrl + K</UKbd>
    </UButton>

    <template #content>
      <div class="size-full flex flex-col gap-2 p-6 overflow-hidden">
        <header class="shrink-0 w-full flex items-center gap-2">
          <UInput v-model="keyword" icon="fa6-solid:magnifying-glass" class="grow" size="xl" placeholder="Search Entities" />

          <UButton icon="fa6-solid:xmark"
            color="neutral"
            variant="outline"
            class="shrink-0 size-[30px] p-0 flex items-center justify-center"
            @click="open = false"
          />
        </header>

        <UTable sticky :data="searchResults" :columns="columns" @select="onSelect">
          <template #DisplayName-cell="{ row }">
            {{ row.original.DisplayName }}
            <template v-if="row.original.DisplayName.replaceAll(' ', '').trim().toLowerCase() != row.original.Name.replaceAll(' ', '').trim().toLowerCase()">
              <small>({{ row.original.Name.trim() }})</small>
            </template>
          </template>
        </UTable>
      </div>
    </template>
  </UModal>
</template>


<script setup lang="ts">
  import type { TableColumn, TableRow } from '@nuxt/ui';
  import { onKeyStroke } from '@vueuse/core';
  import { UTableHeaderSort, IconCheckbox } from '#components';

  const { soaEntityList } = useAptifySoa();

  const open = ref<boolean>(false);
  const keyword = ref<string>('');
  const searchResults = ref<Array<any>>([]);

  const searchShortcut = (e: KeyboardEvent) => {
    if (e.key === 'k' && (e.ctrlKey || e.metaKey)) {
      e.preventDefault();
      open.value = !open.value;
    }
  }

  onKeyStroke('k', searchShortcut, { dedupe: true })

  watch(open, (newVal) => {
    if (!newVal) {
      keyword.value = '';
      searchResults.value = [];
    }
  });

  watchDebounced(
    keyword,
    async () => {
      const key = keyword.value?.trim();
      if (key.length > 4 || (key.length > 0 &&!isNaN(Number(key)))){
        searchResults.value = await soaEntityList(key);
      }
    },
    { debounce: 500, maxWait: 600 },
  );

  const columns: TableColumn<any>[] = [
    {
      accessorKey: 'DisplayName',
      header: ({ column }) => h(UTableHeaderSort, { column: column, label: 'Entity' })
    },
    {
      accessorKey: 'IsSubType',
      header: ({ column }) => h(UTableHeaderSort, { column: column, label: 'Sub-Type' }),
      cell: ({ row }) => h(IconCheckbox, { checked: row.original.ParentID > 0 }),
      meta: uTableHeaderMetaCenter
    },
    {
      accessorKey: 'IsVirtualEntity',
      header: ({ column }) => h(UTableHeaderSort, { column: column, label: 'Virtual' }),
      cell: ({ row }) => h(IconCheckbox, { checked: row.original.IsVirtualEntity }),
      meta: uTableHeaderMetaCenter
    },
    {
      accessorKey: 'IsFrameworkEntity',
      header: ({ column }) => h(UTableHeaderSort, { column: column, label: 'Framework' }),
      cell: ({ row }) => h(IconCheckbox, { checked: row.original.IsFrameworkEntity }),
      meta: uTableHeaderMetaCenter
    },
    {
      accessorKey: 'ID',
      header: 'ID'
    }
  ];

  const onSelect = (row: TableRow<any>) => {
    const currentDef = useEntityDefinition().entityDefEntity.value;

    if (currentDef != undefined && currentDef != null && currentDef.ID == row.original.ID) {
      open.value = false;
      return;
    }

    setTimeout(async () => {
      usePageLoader().showHidePageLoader(true, `Loading: ${row.original.DisplayName}`);
      useRouter().push(`/definition/${row.original.LookupName}/overview`);
      open.value = false;
    }, 50);
  }
</script>
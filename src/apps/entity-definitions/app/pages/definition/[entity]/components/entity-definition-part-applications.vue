<template>
  <EntityDefinitionTable :columns :data="entityDefTableFiltered" v-model:selected-data="selectedData" />
  <EntityDefinitionDrawer v-model="open">
    <template #header>{{ selectedData?.DisplayName }} Entities</template>
    <EntityDefinitionTable v-if="open" :columns="entityColumns" :data="selectedData.Entities" v-model:selected-data="goToEntity" />
  </EntityDefinitionDrawer>
</template>


<script setup lang="ts">
  import { h } from 'vue';
  import type { TableColumn } from '@nuxt/ui';
  import {
    UTableHeaderFiltered,
    UTableHeaderSort,
    IconCheckbox
  } from '#components';

  const { entityDefApplications } = useEntityDefinition();
  const { entityDefTableFiltered, entityDefTableFilterData } = useEntityDefTable(entityDefApplications.value);

  const columns: TableColumn<any>[] = [
    {
      accessorKey: 'DisplayName',
      header: ({ column }) => h(UTableHeaderFiltered, { column: column, label: 'Application', onFilter: entityDefTableFilterData })
    },
    {
      accessorKey: 'InMyFolderList',
      header: ({ column }) => h(UTableHeaderSort, { column: column, label: 'In My Folder List' }),
      cell: ({ row }) => h(IconCheckbox, { checked: row.original.InMyFolderList }),
      meta: uTableHeaderMetaCenter
    },
    {
      accessorKey: 'Permissions',
      header: ({ column }) => h(UTableHeaderSort, { column: column, label: 'Permissions' }),
      cell: ({ row }) => {
        return h(
          'div', { class: 'w-full flex flex-col gap-2' }, [
            h('p', { class: 'flex items-center gap-2' }, [
              h('span', { class: 'text-muted' }, [ 'Groups:' ]),
              h('span', {}, row.original.PermissionsGroupDisplay)
            ]),
            h('p', { class: 'flex items-center gap-2' }, [
              h('span', { class: 'text-muted' }, [ 'Users:' ]),
              h('span', {}, row.original.PermissionsUserDisplay)
            ]),
          ]
        );
      }
    }
  ];

  const open = ref<boolean>(false);
  const selectedData = ref<any>();

  watch(selectedData, (newVal) => {
    if (newVal) {
      open.value = true;
    }
  });

  watch(open, (newVal) => {
    if (!newVal) {
      selectedData.value = null;
    }
  });

  const entityColumns: TableColumn<any>[] = [
    {
      accessorKey: 'DisplayName',
      header: ({ column }) => h(UTableHeaderFiltered, { column, label: 'Entity', onFilter: entityDefTableFilterData })
    },
    {
      accessorKey: 'InMyFolderList',
      header: ({ column }) => h(UTableHeaderSort, { column, label: 'In My Folder List' }),
      cell: ({ row }) => h(IconCheckbox, { checked: row.original.InMyFolderList }),
      meta: uTableHeaderMetaCenter
    },
    {
      accessorKey: 'ID',
      header: 'ID'
    }
  ];

  const goToEntity = ref<any>();
  watch(goToEntity, () => {
    logDev('Go to Entity: ', goToEntity.value);
    if (goToEntity.value) {
      useRouter().push(`/definition/${goToEntity.value.Name.toLowerCase().replaceAll(' ', '-')}/overview`);
    }
  });
</script>
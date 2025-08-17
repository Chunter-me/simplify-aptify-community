<template>
  <EntityDefinitionDrawer v-if="view" v-model="open" :use-all-props="true" :all-props="allProps">
    <template #header>
      <nuxt-link :to="`${aptifyWebUrl}/#/views/${view.ID}`" external target="_blank">
        {{ view.Name }} <small>(#{{ view.ID }})</small>
      </nuxt-link>
    </template>

    <div v-if="isLoading">
      Loading View Details...
    </div>
    <div v-else class="size-full flex flex-col gap-4">
      <div class="shrink-0 grid grid-cols-2 gap-4">
        <div>
          <p class="text-muted">Base View</p>
          {{ view.BaseView }}
        </div>

        <div>
          <p class="text-muted">Type</p>
          <div class="flex items-center gap-2">
            <UIcon :name="view.ViewTypeIconHTMLClass" />
            <span>{{ view.ViewType }}</span>
          </div>
        </div>

        <div>
          <p class="text-muted">Owner</p>
          {{ view.OwnerID }}
        </div>

        <div>
          <p class="text-muted">Folder</p>
          {{ view.ViewCategory }}
        </div>
      </div>

      <div class="shrink-0">
        <p class="text-muted">Description</p>
        {{ view.Description }}
      </div>

      <UTabs v-model="selectedTab" :items="tabs" :content="false" variant="link" />

      <div class="grow overflow-y-auto">
        <div v-if="selectedTab == '0'" class="size-full flex flex-col gap-4 overflow-hidden">
          <div class="shrink-0 pt-2">
            <span class="text-muted">Logic String:</span> {{ view.LogicString }}
          </div>

          <UTable v-if="selectedTab == '0'" sticky :data="viewData?.Filters" :columns="filterColumns" />
        </div>

        <UTable v-else-if="selectedTab == '1'" sticky :data="viewData?.Fields" :columns="fieldColumns" />

        <div v-else-if="selectedTab == '2'" class="mdc-only-wrapper">
          <Markdown :mdc="viewData?.SqlParsed" />
        </div>
      </div>
    </div>
  </EntityDefinitionDrawer>
</template>

<script setup lang="ts">
  import type { TableColumn, TabsItem } from '@nuxt/ui';

  const { aptifyWebUrl, soaViewDetails } = useAptifySoa();

  const view = defineModel<any>();
  watch(view, () => {
    open.value = view.value != undefined && view.value != null;
  });

  const selectedTab = ref<string>('0');
  const tabs = ref<TabsItem[]>([
    { label: 'Filters' },
    { label: 'Fields' },
    { label: 'SQL' }
  ]);

  const viewData = ref<ViewData | null>(null);
  const open = ref<boolean>(false);
  watch(open, () => {
    selectedTab.value = '0';

    if (open.value) {
      getViewData();
    }
    else {
      view.value = null;
    }
  });

  const isLoading = ref<boolean>(false);
  const getViewData = async () => {
    if (view.value && open.value) {
      isLoading.value = true;

      viewData.value = await soaViewDetails(view.value.ID);
      logDev('View Data: ', viewData.value);

      setTimeout(() => {
        isLoading.value = false;
      }, 50);
    }
  };

  const filterColumns: TableColumn<any>[] = [
    {
      accessorKey: 'Sequence',
      header: 'Sequence',
    },
    {
      accessorKey: 'Field',
      header: 'Field',
    },
    {
      accessorKey: 'Operator',
      header: 'Operator',
      meta: {
        class: {
          th: 'text-center',
          td: 'text-center'
        }
      }
    },
    {
      accessorKey: 'Value',
      header: 'Value',
    }
  ];

  const fieldColumns: TableColumn<any>[] = [
    {
      accessorKey: 'DisplayName',
      header: 'Display',
    },

    {
      accessorKey: 'Field',
      header: 'Field',
    }
  ];

  const allProps = computed(() => {
    return open.value ? view.value : null;
  });
</script>
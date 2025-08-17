<template>
  <EntityDefinitionDrawer v-if="details" v-model="open">
    <template #header>
      {{details?.ProcessFlowName}} - ID: {{details?.ProcessFlowID}}
    </template>

    <div class="size-full flex flex-col gap-4">
      <section class="shrink-0">
        <strong class="block text-muted">Description</strong>
        <p class="text-sm">{{ details.ProcessFlowDescription }}</p>
      </section>

      <nav class="shrink-0">
        <UTabs v-model="pfSelectedTab" :items="pfTabs" :content="false" />
      </nav>

      <div class="grow size-full overflow-y-auto">
        <div class="size-full flex items-center justify-center" v-if="pfSelectedTab === 'steps'">
          <Mermaid :diagram="details.ProcessFlow.Diagram" @node-click="onNodeClick" />
        </div>

        <section v-show="pfSelectedTab === 'inputs'">
          <UTable :columns="pfInputColumns" :data="details.ProcessFlow.Inputs" empty="Process Flow has no inputs." />
        </section>

        <section v-show="pfSelectedTab === 'outputs'">
          <UTable :columns="pfOutputsColumns" :data="details.ProcessFlow.Outputs" empty="Process Flow has no outputs." />
        </section>

        <section v-show="pfSelectedTab === 'results'">
          <UTable :columns="pfResultsColumns" :data="details.ProcessFlow.Results" empty="Process Flow has no result codes." />
        </section>
      </div>
    </div>

    <EntityDefinitionPartPipelinesStep v-model="selectedStep" />
  </EntityDefinitionDrawer>
</template>


<script setup lang="ts">
  import type { TableColumn, TabsItem } from '@nuxt/ui';
  import { h } from 'vue';
  import { IconCheckbox } from '#components';

  const details = defineModel<any>({ default: null });
  logDev('Details:', details);

  const open = ref<boolean>(false);
  const pfSelectedTab = ref<string>('steps');

  watch(details, () => {
    if (details.value) {
      open.value = true;
    }
  });

  watch(open, () => {
    pfSelectedTab.value = 'steps';

    if (!open.value) {
      details.value = null;
    }
  });

  const pfTabs: TabsItem[] = [
    {
      label: 'Steps',
      value: 'steps'
    },
    {
      label: 'Inputs',
      value: 'inputs'
    },
    {
      label: 'Results',
      value: 'results'
    },
    {
      label: 'Outputs',
      value: 'outputs'
    }
  ];

  const pfInputColumns: TableColumn<any>[] = [
    {
      accessorKey: 'Name',
      header: 'Input'
    },
    {
      accessorKey: 'Type',
      header: 'Type'
    },
    {
      accessorKey: 'IsRequired',
      header: 'Required',
      cell: ({ row }) => h(IconCheckbox, { checked: row.original.IsRequired }),
      meta: uTableHeaderMetaCenter
    },
    {
      accessorKey: 'DefaultValue',
      header: 'Default Value'
    }
  ];

  const pfOutputsColumns: TableColumn<any>[] = [
    {
      accessorKey: 'Name',
      header: 'Input'
    },
    {
      accessorKey: 'Type',
      header: 'Type'
    },
    {
      accessorKey: 'Description',
      header: 'Description'
    }
  ];

  const pfResultsColumns: TableColumn<any>[] = [
    {
      accessorKey: 'Code',
      header: 'Code'
    },
    {
      accessorKey: 'IsSuccess',
      header: 'Is Success',
      cell: ({ row }) => h(IconCheckbox, { checked: row.original.IsSuccess }),
      meta: uTableHeaderMetaCenter
    },
    {
      accessorKey: 'Description',
      header: 'Description'
    }
  ];

  const selectedStep = ref<any>(null);
  const onNodeClick = (node: { id: string, node: any }) => {
    if (node) {
      const step = details.value?.ProcessFlow?.Steps?.find((s: any) => s.ID.toString() === node.id);
      selectedStep.value = step;
    }
  };
</script>
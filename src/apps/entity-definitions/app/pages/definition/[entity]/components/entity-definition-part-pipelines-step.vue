<template>
  <UModal v-model:open="open"
    :title="step?.Name ?? ''"
    :fullscreen="fullscreen"
    :ui="{
      content: `!w-full ${(fullscreen ? '!max-w-full' : '!max-w-[1400px]')} !h-full`,
      body: 'size-full flex flex-col gap-4 !overflow-hidden !pt-0'
    }"
  >
    <template #header>
      <div class="w-full flex items-center justify-between">
        <span class="shrink-0 flex items-center gap-2">
          <UIcon :name="typeIcon" size="16" />
          <strong class="text-lg font-semibold">{{ step?.Name ?? '' }}</strong>
        </span>

        <div class="shrink-0 flex items-center gap-2">
          <UButton icon="fa6-solid:expand"
            size="sm"
            color="neutral"
            variant="outline"
            @click="fullscreen = !fullscreen"
          />

          <UButton icon="i-lucide:x"
            size="sm"
            color="neutral"
            variant="outline"
            @click="open = false"
          />
        </div>
      </div>
    </template>

    <template #body v-if="step">
      <div class="size-full flex flex-col gap-4 overflow-hidden">
        <div class="shrink-0" v-if="step.Description || step.Component?.Description">
          <strong class="block text-muted">Description</strong>
          <p class="text-sm">{{ step.Description || step.Component?.Description }}</p>
        </div>

        <nav class="shrink-0">
          <UTabs v-model="pfNodeSelectedTab" :items="pfNodeTabs" :content="false" />
        </nav>

        <div class="grow size-full px-2 overflow-y-auto">
          <section v-show="pfNodeSelectedTab === 'details' && step.Type == 'Rule'" class="grow flex flex-col gap-4 overflow-hidden text-sm">
            <p class="shrink-0 mb-3">
              <strong class="text-muted font-semibold">Rule Script:</strong> {{ step.RuleType }}
            </p>

            <div class="grow size-full overflow-auto">
              <Markdown :markdown="markdownWrapCode(step.RuleType.replace('Script', '').trim().toLowerCase(), step.RuleTextScript)" />
            </div>
          </section>

          <section v-show="pfNodeSelectedTab === 'details' && step.Type == 'Component'">
            <UTable :columns="pfComponentColumns" :data="componentDetails ?? []" empty="Step has no inputs." />
          </section>

          <section v-show="pfNodeSelectedTab === 'inputs'">
            <UTable :columns="pfInputColumns" :data="step.Inputs ?? []" empty="Step has no inputs." />
          </section>

          <section v-show="pfNodeSelectedTab === 'outputs'">
            <UTable :columns="pfOutputsColumns" :data="step.OutputMaps ?? []" empty="Step has no outputs." />
          </section>

          <section v-show="pfNodeSelectedTab === 'results'">
            <UTable :columns="pfResultsColumns" :data="step.Component?.Results ?? []" empty="Component has no result codes" />
          </section>

          <section v-show="pfNodeSelectedTab === 'actions'">
            <UTable :columns="pfActionColumns" :data="step.Actions ?? []" empty="Step has no action." />
          </section>
        </div>
      </div>
    </template>
  </UModal>
</template>


<script setup lang="ts">
  import type { TableColumn, TabsItem } from '@nuxt/ui';
  import { h } from 'vue';
  import { IconCheckbox } from '#components';

  const step = defineModel<any>({ default: null });
  logDev('Step:', step);

  const open = ref<boolean>(false);
  const fullscreen = ref<boolean>(false);

  const emits = defineEmits(['close']);

  const pfNodeTabsRaw: Array<TabsItem> = [
    { label: 'Details', value: 'details' },
    { label: 'Inputs', value: 'inputs' },
    { label: 'Component Results', value: 'results' },
    { label: 'Outputs', value: 'outputs' },
    { label: 'Actions', value: 'actions' }
  ];
  const pfNodeTabs = ref<Array<TabsItem>>(pfNodeTabsRaw);
  const pfNodeSelectedTab = ref<string>('details');

  const componentDetailProps = [ 'AssemblyName', 'Class', 'Description', 'Name', 'Object' ];
  const componentDetails = ref<Array<any> | null>(null);

  const typeIcon = ref<string>('');

  watch(step, () => {
    if (step.value) {
      pfNodeSelectedTab.value = 'details';
      fullscreen.value = false;

      if (step.value.Type == 'Rule') {
        pfNodeTabs.value = pfNodeTabsRaw.filter((tab: TabsItem) => tab.value != 'inputs' && tab.value != 'results');
      }
      else if (step.value.Type == 'Sub-Process') {
        pfNodeTabs.value = pfNodeTabsRaw.filter((tab: TabsItem) => tab.value != 'details' && tab.value != 'results');
        pfNodeSelectedTab.value = 'inputs';
      }
      else {
        pfNodeTabs.value = pfNodeTabsRaw;
      }

      if (step.value.Type == 'Component') {
        componentDetails.value = objectToPropsKeyValue(step.value.Component)
          .filter((props: any) => componentDetailProps.includes(props.key));
      }

      if (step.value?.Type == 'Component') {
        typeIcon.value = 'fa6-solid:puzzle-piece';
      }
      else if (step.value?.Type == 'Sub-Process') {
        typeIcon.value = 'fa6-solid:chart-diagram';
      }
      else {
        typeIcon.value = 'fa6-solid:code';
      }

      open.value = true;
    }
    else {
      open.value = false;
      fullscreen.value = false;
      pfNodeTabs.value = pfNodeTabsRaw;
      componentDetails.value = null;
      pfNodeSelectedTab.value = 'details';
    }
  });

  watch(open, () => {
    logDev('Step: ', step.value);

    if (!open.value) {
      step.value = null;
    }
  });

  const pfComponentColumns: TableColumn<any>[] = [
    {
      accessorKey: 'key',
      header: 'Property',
      cell: ({ row }) => h('strong', { class: 'font-semibold' }, row.original.key)
    },
    {
      accessorKey: 'value',
      header: 'Value'
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
      accessorKey: 'OutputProperty',
      header: 'Output Property'
    },
    {
      accessorKey: 'Destination',
      header: 'Destination Property'
    },
    {
      accessorKey: 'DestinationType',
      header: 'Destination Type'
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

  const pfActionColumns: TableColumn<any>[] = [
    {
      accessorKey: 'ResultCode',
      header: 'Component Result'
    },
    {
      accessorKey: 'Action',
      header: 'Action'
    },
    {
      accessorKey: 'GoToStepName',
      header: 'Go to Step'
    },
    {
      accessorKey: 'ProcessFlowResultCode',
      header: 'Flow Result'
    },
    {
      accessorKey: 'IsSuccess',
      header: 'Is Success',
      cell: ({ row }) => h(IconCheckbox, { checked: row.original.IsSuccess }),
      meta: uTableHeaderMetaCenter
    }
  ];

</script>
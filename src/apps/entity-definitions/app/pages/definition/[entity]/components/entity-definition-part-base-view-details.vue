<template>
  <EntityDefinitionDrawer v-if="model" v-model="open" :all-props="model">
    <template #header>
      {{ model.DisplayName }}
    </template>

    <section>
      <strong class="font-semibold">{{ model.Name }}</strong><template v-if="model.Description"> - {{ model.Description }}</template>
    </section>

    <UTabs v-model="selectedTab" :items="tabs" :content="false" variant="link" />

    <UTable v-if="selectedTab == '0'" sticky :data="fieldData" :columns="fieldColumns">
      <template #DisplayName-header>
        <UInput v-model="fieldsKeyword"
          type="search"
          placeholder="Field"
          size="xs"
          class="w-full"
        />
      </template>

      <template #DisplayName-cell="{ row }">
        <strong class="block font-semibold">{{ row.original.DisplayName }}</strong>
        <span class="text-muted">{{ row.original.Name }}</span>
      </template>

      <template #EntityField-cell="{ row }">
        <template v-if="row.original.CalculatedFieldExpression != ''">
          <code>{{ row.original.CalculatedFieldExpression }}</code>
        </template>
        <template v-else>
          <span class="text-muted">{{ row.original.Entity }}</span>.{{ row.original.EntityField }}
        </template>
      </template>
    </UTable>

    <div v-else class="mdc-only-wrapper">
      <Markdown :markdown="baseViewSQL" />
    </div>

    <template #footer>
      <nuxt-link href="https://aptifysupport.zendesk.com/hc/en-us/articles/5280102190861-About-the-Fields-Form" target="_blank">
        Aptify KB: Entity Fields Form
      </nuxt-link>
    </template>
  </EntityDefinitionDrawer>
</template>

<script setup lang="ts">
  import { h } from 'vue';
  import type { TableColumn, TabsItem } from '@nuxt/ui';
  import { SqlDataType } from '#components';

  const model = defineModel<any>();

  const open = ref<boolean>(false);
  const fieldsKeyword = ref<string>('');

  const tabs = ref<TabsItem[]>([
    { label: 'Fields' },
    { label: 'SQL' }
  ]);

  const selectedTab = ref<string>('0');
  const baseViewSQL = ref<any>(null);

  watch(model,async () => {
    open.value = model.value != undefined && model.value != null;

    if (model.value) {
      selectedTab.value = '0';
      baseViewSQL.value = markdownWrapSqlCode(model.value?.SQL);
    }
    else {
      selectedTab.value = '0';
      baseViewSQL.value = null;
    }
  });

  watch(open, () => {
    if (!open.value) {
      model.value = null;
    }
  });

  // Fields
  const fieldColumns: TableColumn<any>[] = [
    {
      accessorKey: 'DisplayName',
      header: 'Field',
    },
    {
      accessorKey: 'SQLDataType',
      header: 'Data Type',
      cell: ({ row }) => h(SqlDataType, { type: row.original.SQLDataType }),
    },
    {
      accessorKey: 'EntityField',
      header: 'Entity Field/Calculation',
    }
  ];

  const fieldData = computed(() => {
    const keyword = fieldsKeyword.value.toLowerCase().trim();
    return model.value.Fields?.filter((field: any) => {
      if (keyword.length > 2) {
        return field.Name.toLowerCase().includes(keyword) || field.DisplayName.toLowerCase().includes(keyword);
      }

      return true;
    });
  });
</script>
<template>
  <EntityDefinitionDrawer v-if="model" v-model="open" :all-props="model">
    <template #header>
      <div class="flex items-center gap-2">
      <span>{{ model.DisplayName }}</span> <small class="text-muted">({{ model.Name }})</small>
      </div>
    </template>

    <section v-if="model.Description && model.Description != model.DisplayName && model.Description != model.Name">
      {{ model.Description }}
    </section>

    <section>
      <UTable :data="[ model ]" :columns="fieldTypeColumns">
        <template #ExtendedType-header>
          <PopoverHoverText label="Extended Type">
            <Markdown :markdown="tooltip('ExtendedType')" />
          </PopoverHoverText>
        </template>

        <template #SQLDataType-cell="{ row }">
          <SqlDataType :type="row.original.SQLDataType" />
        </template>

        <template #SQLAllowNull-cell="{ row }">
          <IconCheckbox :checked="row.original.SQLAllowNull" />
        </template>
      </UTable>
    </section>

    <section v-if="model.IsVirtual">
      <UTable :data="[ model ]" :columns="virtualColumns">
        <template #JoinedEntity-cell="{ row }">
          <nuxt-link :to="`/definition/${row.original.JoinedEntity.toLowerCase().replaceAll(' ', '-')}/overview`">
            {{ row.original.JoinedEntity }}
          </nuxt-link>
        </template>
      </UTable>
    </section>

    <section>
      <span class="block text-muted mb-3">Options</span>
      <UTable :data="fieldOptions" :columns="fieldOptionColumns" class="[&>table>thead]:hidden" />
    </section>

    <template #footer>
      <nuxt-link v-if="supportLink" :href="supportLink.Value" target="_blank">
        {{ supportLink.Title }}
      </nuxt-link>
    </template>
  </EntityDefinitionDrawer>
</template>

<script setup lang="ts">
  import { h } from 'vue';
  import type { TableColumn } from '@nuxt/ui';
  import { IconCheckbox, Markdown } from '#components';

  const model = defineModel<any>();
  const open = ref<boolean>(false);

  const tooltips: Array<any> = await useAptifySoa().soaAppContentBlocks('Tooltip_Fields_');
  const sqlDataTypes: Array<any> = await useAptifySoa().soaAppContentBlocks('DataType_');
  const supportLink: any = (await useAptifySoa().soaAppContentBlocks('Part_Fields_DetailsSupportLink'))?.[0] ?? null;

  const tooltip = (tip: string) => {
    return tooltips.find((t: any) => t.Name.toLowerCase().endsWith(tip.replaceAll(' ', '').toLowerCase()))?.Value ?? '';
  };

  const sqlDataType = (type: string) => {
    return sqlDataTypes.find((t: any) => t.Name.toLowerCase().endsWith(type.toLowerCase()))?.Value ?? '';
  };

  const fieldOptions = computed(() => [
    { label: 'Is Required', value: model.value?.IsRequired, description: 'The field must have a value. It can not be NULL or blank.' },
    { label: 'Is Updatable', value: model.value?.Updateable, description: 'The field can be updated through the Aptify Framework.' },
    { label: 'In Table', value: model.value?.InTable, description: 'The field exists on the entities Base Table.' },
    { label: 'Is Unique', value: model.value?.IsUnique, description: 'The field\'s value can only exists on one record only.' },
    { label: 'Is Name Field', value: model.value?.IsNameField, description: 'The field\'s value is used when doing basic entity search operations.' },
    { label: 'Is Encrypted', value: model.value?.EncryptData, description: 'The field\'s value is encrypted.' },
    { label: 'Show in Default View', value: model.value?.DefaultInView, description: 'The field is included in the Fields selection when creating a new view.' },
    { label: 'Show in Find', value: model.value?.ShowInFind, description: 'The field is included in the default results of a Search using the Find operation.' }
  ]);

  const fieldOptionColumns: TableColumn<any>[] = [
    {
      accessorKey: 'label',
      cell: ({ row }) => h(IconCheckbox, { checked: row.original.value, label: row.original.label }),
    },

    {
      accessorKey: 'description',
      header: 'Description',
      cell: ({ row }) => h(Markdown, { markdown: tooltip(row.original.label)}),
      meta: {
        class: {
          td: '!text-muted !whitespace-normal'
        }
      }
    },
  ];

  watch(model, () => {
    open.value = model != undefined && model != null;
  });

  watch(open, () => {
    if (!open.value) {
      model.value = null;
    }
  });

  const fieldTypeColumns: TableColumn<any>[] = [
    {
      accessorKey: 'SQLDataType',
      header: 'Data Type',
    },
    {
      accessorKey: 'SQLFieldSize',
      header: 'Size',
      meta: {
        class: {
          th: 'text-center',
          td: 'text-center'
        }
      }
    },
    {
      accessorKey: 'SQLAllowNull',
      header: 'Allow Null',
      meta: {
        class: {
          th: 'text-center',
          td: 'text-center'
        }
      }
    },
    {
      accessorKey: 'ExtendedType',
      header: 'Extended Type',
    }
  ];

  const dataType = (row: any) => {
    if (row.original.SQLTypeLabel.trim().toLowerCase() != row.original.SQLDataType.trim().toLowerCase()) {
      return `${row.original.SQLTypeLabel.trim()} <span class="text-muted">(${row.original.SQLDataType.trim().toUpperCase()})</span>`;
    }

    return row.original.SQLTypeLabel.trim();
  }

  const virtualColumns: TableColumn<any>[] = [
    {
      accessorKey: 'VirtualFieldType',
      header: 'Virtual Type',
    },
    {
      accessorKey: 'JoinedEntity',
      header: 'Entity',
    },
    {
      accessorKey: 'JoinedField',
      header: 'Field',
    },
    {
      accessorKey: 'JoinForeignKey',
      header: 'Using',
    }
  ];
</script>
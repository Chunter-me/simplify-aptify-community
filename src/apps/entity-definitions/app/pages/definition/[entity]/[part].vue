<template>
  <article v-if="entityDefEntity" class="size-full flex flex-col gap-3 overflow-hidden">
    <header class="shrink-0 flex items-center justify-between gap-4 px-2 pb-3 border-b border-b-zinc-600 dark:border-b-zinc-600">
      <div>
        <small v-if="entityDefEntity.ParentID && entityDefEntity.ParentID > 0" class="pl-1 text-muted text-xs">
          Sub-Type of: <nuxt-link :to="`/definition/${entityDefEntity.ParentEntity.toLowerCase().replaceAll(' ', '-')}/overview`">{{ entityDefEntity.ParentEntity }}</nuxt-link>
        </small>
        <h1 class="w-full flex items-center gap-2.5">
          <span v-html="headerTitle" class="flex items-center gap-2" />

          <UModal :title="partDescription?.Title" :ui="{ content: '!w-full !max-w-[900px]', body: 'text-content !py-0' }">
            <UButton v-if="partDescription"
              icon="fa6-regular:circle-question"
              variant="link"
              class="!text-sky-700 dark:!text-sky-400"
            />

            <template #body>
              <div class="part-description">
                <Markdown :markdown="partDescription?.Value" />
              </div>
            </template>
          </UModal>
        </h1>
      </div>

      <ul class="list-none flex items-center justify-end text-right gap-6 whitespace-nowrap">
        <li v-if="entityDefEntity.Name != entityDefEntity.DisplayName" class="flex flex-col">
          <small class="text-muted">Name:</small>
          <span>{{ entityDefEntity.Name }}</span>
        </li>

        <li class="flex flex-col">
          <small class="text-muted">Base View:</small>
          <span>{{ entityDefEntity.BaseView }}</span>
        </li>

        <li class="flex flex-col">
          <small class="text-muted">ID:</small>
          <span>{{ entityDefEntity.ID }}</span>
        </li>
      </ul>
    </header>

    <div class="grow flex gap-8 overflow-hidden">
      <div class="grow relative overflow-y-auto">
        <component v-if="selectedPart" :is="selectedPart.component" />
      </div>

      <nav class="shrink-0 w-[60px] md:w-[180px] overflow-y-auto">
        <ul class="list-none flex flex-col gap-2 pt-4">
          <li v-for="tab in definition" :key="tab.partId">
            <UButton :to="`./${tab.partId}`"
              :icon="tab.icon"
              :label="tab.label"
              :variant="tab.partId == selectedPart.partId ? 'solid' : 'link'"
              :class="[ 'w-full !no-underline justify-center md:justify-start',
                { '!text-gray-200': tab.partId == selectedPart.partId },
                { '!text-gray-800 dark:!text-gray-200 dark:hover:!text-gray-800': tab.partId != selectedPart.partId },
                '[&>span:nth-child(even)]:hidden md:[&>span:nth-child(even)]:inline-block',
              ]"
              @click="navClick(tab)"
            />
          </li>
        </ul>
      </nav>
    </div>
  </article>

  <div v-else-if="!show && !definition">
    No entity data found for {{ routeParamEntity }}.
  </div>
</template>


<script setup lang="ts">
  const { show, showHidePageLoader } = usePageLoader();
  const { soaAppContentBlocks } = useAptifySoa();

  if (!show.value) {
    showHidePageLoader(true, 'Loading Definition...');
  }

  const route = useRoute();
  const routeParamEntity = String(route.params.entity);
  const routeParamPart = String(route.params?.part ?? '')?.toLowerCase().trim() ?? 'overview';

  const selectedPart = computed(() => definition.find((def: any) => def.partId == routeParamPart));

  const { entityDefEntity, entityDefinitionGet } = useEntityDefinition();
  let definition: Array<any> = (await entityDefinitionGet(routeParamEntity)) ?? [];
  logDev('definition: ', definition);

  onMounted(async () => {
    setTimeout(() => {
      showHidePageLoader(false);
    }, 250);
  });

  const headerTitle = computed(() => {
    const title = [ `<span>${entityDefEntity?.value.DisplayName}</span>` ];

    if (selectedPart.value) {
      let subTitle = `${selectedPart.value.data?.length ?? 0} ${selectedPart.value.label}`;

      if (selectedPart.value.partId == 'overview') {
        subTitle = 'Overview';
      }

      else if (selectedPart.value.partId == 'process-pipeline') {
        subTitle = 'Process Pipelines';
      }

      title.push('<span>-</span>')
      title.push(`<span>${subTitle}</span>`)
    }

    return title.join('');
  });

  const appBlocks = await soaAppContentBlocks('PartDesc_');
  const partDescription = computed(() => {
    return appBlocks?.find((b: any) => b.Name.toLowerCase().endsWith(selectedPart.value.partId.replace('-', '').trim().toLowerCase()));
  });

  const navClick = (tab: any) => {
    if (tab.partId != selectedPart.value.partId) {
      showHidePageLoader(true, `Loading ${tab.label}...`);
    }
  }
</script>
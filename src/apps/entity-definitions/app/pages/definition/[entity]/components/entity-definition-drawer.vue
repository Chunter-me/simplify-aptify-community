<template>
  <UDrawer v-model:open="open"
    as="aside"
    direction="right"
    :handle="false"
    :handleOnly="true"
    title="Entity Definition Drawer"
    description="Additional Details of the Component"
  >
    <template #content>
      <div class="min-w-[300px] md:!min-w-[700px] w-full max-w-[1200px] h-full flex flex-col gap-4 p-6 overflow-hidden">
        <header class="shrink-0 flex items-center justify-between gap-4">
          <h3 class="w-full text-2xl font-semibold">
            <slot name="header" />
          </h3>

          <slot name="header-right">
            <UButton v-if="hasAllProps && allPropsOpen" class="h-[26px] flex items-center justify-center !text-[14px]"
              icon="fa6-solid:arrow-left"
              label="Back"
              color="neutral"
              variant="outline"
              @click="allPropsOpen = false"
            />

            <UButton v-else class="size-[26px] flex items-center justify-center !text-[14px]"
              icon="fa6-solid:xmark"
              color="neutral"
              variant="outline"
              @click="open = false"
              title="Close"
            />
          </slot>
        </header>

        <div class="grow h-full flex flex-col gap-4 overflow-y-auto">
          <template v-if="!allPropsOpen">
            <slot />
          </template>
          <template v-else-if="hasAllProps && allPropsOpen">
            <slot name="all-props">
              <EntityDefinitionDrawerAllProps v-if="allProps" :data="allProps" />
            </slot>
          </template>
        </div>

        <footer class="shrink-0 flex items-center justify-between gap-4 text-sm">
          <div>
            <UButton v-if="hasAllProps && !allPropsOpen" color="neutral" variant="outline" @click="allPropsOpen = true">
              All Properties
            </UButton>
          </div>

          <slot name="footer" />
        </footer>
      </div>
    </template>
  </UDrawer>
</template>


<script setup lang="ts">
  const open = defineModel<boolean>();
  const { allProps } = defineProps<{ allProps?: any }>();

  logDev('All Props: ', allProps);

  const allPropsOpen = ref<boolean>(false);

  const hasAllProps = computed(() => {
    return allProps != undefined && allProps != null;
  })
</script>
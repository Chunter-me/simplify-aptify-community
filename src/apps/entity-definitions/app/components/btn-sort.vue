<template>
  <UButton class="shrink-0 size-[26px] flex items-center justify-center !text-[14px]"
    :icon="direction == 'asc' ? 'fa6-solid:sort-up' : direction == 'desc' ? 'fa6-solid:sort-down' : 'fa6-solid:sort'"
    color="neutral"
    variant="link"
    @click="click"
  />
</template>

<script setup lang="ts">
  const { direction } = defineProps<{ direction?: string | boolean | undefined }>();
  const emits = defineEmits([ 'click' ]);

  const click = () => {
    emits('click', (!direction || direction == undefined) ? true : direction === 'asc' ? undefined : false);
  }
</script>
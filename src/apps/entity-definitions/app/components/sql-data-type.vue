<template>
  <PopoverHoverText v-if="markdown" :label>
    <Markdown :markdown />
  </PopoverHoverText>
</template>

<script setup lang="ts">
  const { type } = defineProps<{ type: string }>();

  const markdown = ref<string>('');
  const label = ref<string>('');

  watch(() => type, async (newType) => {
    const sqlDataType = await useAptifySoa().soaAppContentBlocks(`DataType_${newType}`);
    markdown.value = sqlDataType?.[0]?.Value ?? '';
    label.value = sqlDataType?.[0]?.Title ?? '';
  }, { immediate: true });
</script>
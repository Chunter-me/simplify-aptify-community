<template>
  <article v-if="overview" class="text-content pt-2 px-2 pb-6">
    <Markdown :mdc="overview" />
  </article>
</template>

<script setup lang="ts">
  const { entityDefEntity } = useEntityDefinition();
  const overview = entityDefEntity.value && entityDefEntity.value.Definition && (entityDefEntity.value.DefinitionParsed == undefined || entityDefEntity.value.DefinitionParsed == null)
    ? await useMarkdownParser().parseMarkdown(entityDefEntity.value.Definition)
    : entityDefEntity.value.DefinitionParsed;
</script>
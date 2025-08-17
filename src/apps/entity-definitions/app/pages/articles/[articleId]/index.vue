<template>
  <article v-if="article?.Article?.ArticleParsed" class="text-content pt-2 px-2 pb-6">
    <Markdown :mdc="article.Article.ArticleParsed" />
  </article>
</template>

<script setup lang="ts">
  const { showHidePageLoader } = usePageLoader();

  showHidePageLoader(true, 'Loading Article...');

  const route = useRoute();
  const lookupKey = String(route.params.articleId ?? '');

  const article = ref<any>();

  if (lookupKey) {
    article.value = await useAptifySoa().soaArticle(lookupKey);
  }

  setTimeout(() => {
    showHidePageLoader(false);
  }, 50);
</script>
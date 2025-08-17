<template>
  <h1>Edit/Create Article(s)</h1>
</template>

<script setup lang="ts">
  const { showHidePageLoader } = usePageLoader();

  showHidePageLoader(true, 'Loading Article...');

  const route = useRoute();
  const lookupKey = String(route.params.articleId ?? '');

  const article = ref<any>();

  if (lookupKey && lookupKey != '-1') {
    article.value = await useAptifySoa().getArticle(lookupKey);
  }

  setTimeout(() => {
    showHidePageLoader(false);
  }, 50);
</script>
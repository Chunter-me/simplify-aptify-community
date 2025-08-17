<template>
  <div class="size-full overflow-y-scroll">
    <div class="flex flex-col gap-8 text-content">
      <h1 class="shrink-0 !text-4xl text-center !m-0 pt-3">About Entity Definitions</h1>

      <section>
        <p>
          Entity Definitions was created by <nuxt-link href="https://chunter.me" target="_blank">Christopher Hunter</nuxt-link> after being inspired by <a href="https://dbdocs.io" target="_blank">dbdocs.io</a>.
          The community needed a way to expose Entity details to users so that they can learn the entities and how they connect.
        </p>

        <p>
          Entity Definitions is a <em>Static Site Generated Nuxt.js</em> (compiles to just HTML/CSS/JavaScript) that was built to live side-by-side with Aptify Web.
          It uses the existing Aptify Web IIS setup where it's an Application of the site.
          By doing the same with Entity Definitions we can take advantage of the Domain Cookie for accessing the Aptify Web SOA.
          This means we have access to Service Data Objects and thus we get Entity Definitions.
        </p>
      </section>

      <h2 class="shrink-0 !m-0">Version Logs</h2>

      <div class="shrink-0">
        <div v-if="isLoading" class="text-muted">
          Loading version logs...
        </div>

        <ul v-else-if="!isLoading && versionLogs.length > 0" class="!list-none !p-0 !m-0 flex flex-col divide-y-2 divide-zinc-600">
          <li class="!px-0 !py-6 !m-0 first:!pt-0 last:!pb-0 flex flex-col gap-2" v-for="(log, idx) in versionLogs" :key="idx">
            <h3 class="!m-0">{{ log.title }}</h3>

            <p class="!m-0 font-semibold text-muted">
              v{{ log.version }} - {{ log.date }}
            </p>

            <markdown :markdown="log.content" />
          </li>
        </ul>

        <div v-else-if="!isLoading && versionLogs.length <= 0" class="text-gray-500">
          No version logs available.
        </div>

        <div v-else-if="versionErrors !== undefined && versionErrors !== null && versionErrors != ''" class="text-red-500">
          {{ versionErrors }}
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  const isLoading = ref(true);
  const versionLogs = ref<Array<any>>([]);
  const versionErrors = ref<string | null>(null);

  onMounted(() => {
    getVersions();
  });

  const getVersions = async () => {
    versionLogs.value = (await fetch('https://raw.githubusercontent.com/Chunter-me/simplify-aptify-community/refs/heads/main/public/entity-definition-about-versions.json')
      .then(response => response.json())
      .catch(error => {
        console.error('Error fetching versions:', error);
        versionErrors.value = `Failed to load version logs: ${error}`;
        return [];
      })).sort((a: any, b: any) => {
        return parseDateString(b.date).getTime() - parseDateString(a.date).getTime();
      });

    isLoading.value = false;
  };
</script>
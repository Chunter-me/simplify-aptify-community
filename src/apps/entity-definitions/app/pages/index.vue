<template>
  <div class="size-full flex flex-col gap-16 overflow-y-auto">
    <section class="shrink-0 flex flex-col items-center justify-center gap-8 text-center pt-8">
      <h1 class="!text-5xl">
        <strong class="block text-xl font-semibold pb-1">Welcome to</strong>
        Aptify Entity Definitions
      </h1>

      <p>
        Learn all about your entities structure, how they link between each other, and so much more.
      </p>

      <p>
        <strong>Get started by:</strong><br>
        Pressing Ctrl + K anytime on your keyboard.<br>
        Clicking the Search button above.
      </p>
    </section>

    <section v-if="featured && featured.length > 0" class="px-8">
      <h2 class="!text-4xl text-center mb-6">Featured Entities</h2>

      <ul class="w-full list-none grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-8">
        <li v-for="entity in featured" :key="entity.id">
          <nuxt-link :to="`/definition/${entity.LookupName}`" :class="[
            'h-[340px] block border rounded-lg cursor-pointer !overflow-hidden',
            'border-zinc-600 !text-gray-800 hover:border-zinc-100 hover:!no-underline',
            'dark:!text-gray-200 dark:border-zinc-400 dark:hover:border-zinc-950 dark:hover:bg-zinc-950'
          ]">
            <div class="size-full border-b-[12px] border-zinc-50 dark:border-zinc-900 overflow-hidden">
              <div class="shrink-0 w-full h-[140px] bg-zinc-300 dark:bg-zinc-800 flex items-center justify-center">
                <UIcon :name="entity.Icon" size="60" />
              </div>

              <div class="grow p-3 !pb-0 text-sm">
                <h3 class="font-semibold text-lg">
                  {{ entity.DisplayName }}
                </h3>

                <section class="pt-2">
                  <Markdown :markdown="entity.IsFeaturedSummary" />
                </section>
              </div>
            </div>
          </nuxt-link>
        </li>
      </ul>
    </section>
  </div>
</template>


<script setup lang="ts">
  const { soaEntityList } = useAptifySoa();
  const featured = (await soaEntityList()).filter((entity: any) => entity.IsFeatured);
</script>
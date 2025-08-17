<template>
  <section v-if="entityDefArticles && entityDefArticles.length > 0">
    <ul class="list-none grid grid-cols-12 gap-8">
      <li v-for="article in entityDefArticles" :key="article.id" class="col-span-12 md:col-span-6 lg:col-span-4">
        <nuxt-link :to="`/articles/${article.LookupKey}`" :class="[
          'h-[300px] flex flex-col border rounded-lg overflow-hidden cursor-pointer',
          'border-zinc-600 !text-gray-800 hover:border-zinc-300 hover:bg-zinc-500 hover:text-white hover:!no-underline',
          'dark:!text-gray-200 dark:border-zinc-400 dark:hover:border-zinc-950 dark:hover:bg-zinc-950'
        ]">
          <figure class="shrink-0">
            <img v-if="article.ImageUrl && article.ImageUrl != ''"
              class="w-full h-[140px] rounded-t-lg size-full object-cover"
              :src="article.ImageUrl"
              :alt="article.Title"
            />

            <div v-else class="w-full h-[140px] bg-zinc-300 dark:bg-zinc-800 flex items-center justify-center">
              <UIcon :name="typeIcon(article.Type)" size="60" />
            </div>
          </figure>

          <div class="grow mt-3 mx-4 overflow-hidden text-sm">
            <h3 class="font-semibold text-lg">
              {{ article.Title }}
            </h3>

            <section class="pt-2">
              {{ article.Summary }}
            </section>
          </div>
        </nuxt-link>
      </li>
    </ul>
  </section>

  <section v-else>
    There are currently no Articles for the {{ entityDefEntity?.DisplayName }} entity.
  </section>
</template>


<script setup lang="ts">
  const { entityDefEntity, entityDefArticles } = useEntityDefinition();

  const typeIcon = (type: string) => {
    if (type == 'SOP') {
      return 'fa6-solid:stairs';
    }
    else if (type == 'Guide') {
      return 'fa6-solid:book';
    }

    return 'fa6-regular:newspaper';
  }
</script>
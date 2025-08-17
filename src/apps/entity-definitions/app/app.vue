<template>
  <UApp>
    <PageLoader />

    <header class="shrink-0 w-full py-4 px-6 border-b border-b-zinc-300 dark:border-b-zinc-600 overflow-hidden">
      <div class="app-header w-full max-w-[1600px] mx-auto flex items-center justify-between text-gray-600 dark:text-gray-400">
        <nav class="shrink-0" aria-label="Application switcher.">
          <UDropdownMenu :items="appMenuItems" :content="{ align: 'start' }" :ui="{ itemLabel: 'text-xs' }">
            <UButton v-bind="{ ...selectedApp }"
              block
              trailingIcon="i-lucide-chevrons-up-down"
              color="neutral"
              variant="ghost"
              class="data-[state=open]:bg-elevated w-full max-w-[220px] [&>span:nth-child(even)]:hidden md:[&>span:nth-child(even)]:inline-block"
              :ui="{
                label: 'text-xs',
                trailingIcon: 'text-dimmed'
              }"
            />
          </UDropdownMenu>
        </nav>

        <section class="grow flex items-center justify-end gap-6" aria-label="Settings and back to Aptify Web.">
          <Search />

          <UButton class="shrink-0 size-[30px] flex items-center justify-center p-0 !text-[12px]"
            :icon="colorMode == 'dark' ? 'fa6-regular:sun' : 'fa6-regular:moon'"
            color="neutral"
            variant="outline"
            @click="toggleMode"
          />
        </section>
      </div>
    </header>

    <main class="grow w-full max-w-[1600px] px-8 mx-auto overflow-hidden">
      <nuxt-page />
    </main>

    <footer class="shrink-0 h-[46px] w-full flex items-center justify-center px-6 border-t border-b-zinc-300 text-gray-400 dark:border-t-zinc-600">
      <div class="w-full max-w-[1600px] flex items-center justify-between text-xs">
        <span>
          By: <nuxt-link href="https://chunter.me" target="_blank" class="!text-gray-400 hover:underline">Simplify Aptify - Christopher Hunter</nuxt-link> - v{{ useRuntimeConfig().public.AppVersion }}
        </span>

        <nav aria-label="Sub page navigation">
          <ul class="flex items-center gap-5">
            <li>
              <nuxt-link to="/about" class="!text-gray-400 hover:underline">
                About
              </nuxt-link>
            </li>
          </ul>
        </nav>
      </div>
    </footer>
  </UApp>
</template>


<script setup lang="ts">
  import { useColorMode } from '@vueuse/core';
  import type { DropdownMenuItem } from '@nuxt/ui';

  const { system, store } = useColorMode();

  const colorMode = computed(() => store.value === 'auto' ? system.value : store.value);

  const toggleMode = () => {
    store.value = colorMode.value === 'dark' ? 'light' : 'dark'
  }

  const { aptifyWebUrl } = useAptifySoa();

  const apps = ref([
    {
    label: 'Entity Definitions',
    avatar: {
      icon: 'fa6-solid:hexagon-nodes',
      alt: 'Entity Definitions'
    }
  },
    {
      label: 'Aptify Web',
      avatar: {
        src: `${aptifyWebUrl}/styles/images/favicon.png`,
        alt: 'Aptify Web'
      }
    }
  ]);

  const selectedApp = ref<any>(apps.value[0]);

  const appMenuItems = computed<DropdownMenuItem[]>(() => {
    return apps.value.map(app => ({
      ...app,
      onSelect() {
        if (app.label === 'Aptify Web') {
          window.location.href = aptifyWebUrl;
        }
        else {
          useRouter().push('/');
        }
      }
    }));
  });
</script>
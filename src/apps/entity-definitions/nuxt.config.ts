import type { NuxtPage } from 'nuxt/schema';
import pkg from './package.json';

export default defineNuxtConfig({
  future: {
    compatibilityVersion: 4
  },
  compatibilityDate: '2024-11-01',
  devtools: { enabled: false },
  ssr: false,

  hooks: {
    'pages:extend'(pages) {
      const pagesToRemove: NuxtPage[] = []
      pages.forEach((page) => {
        if (page.path.includes('component')) {
          pagesToRemove.push(page)
        }
      });

      pagesToRemove.forEach(page => {
        pages.splice(pages.indexOf(page), 1)
      });
    }
  },

  components: [
    { path: '~/components', pathPrefix: false },
    { path: '~/pages/definition/[entity]/components', pathPrefix: false }
  ],

  runtimeConfig: {
    public: {
      AppVersion: pkg.version,
      AptifySoaUrl: process.env.BASE_APTIFY_SOA_URL,
      AptifyToken: process.env.APTIFY_SOA_TOKEN
    },
  },

  app: {
    baseURL: process.env.APP_BASE_URL,
    head: {
      title: 'Aptify Entity Definitions',

      htmlAttrs: {
        lang: 'en',
        class: 'dark'
      },

      meta: [
        { charset: 'utf-8' },
        { name: 'viewport', content: 'width=device-width, initial-scale=1' },
        { name: 'description', content: 'Aptify Entity Definitions' }
      ],

      link: [
        { rel: 'icon', type: 'image/x-icon', href: 'entity-definitions-logo.svg' }
      ]
    }
  },

  css: [
    'assets/app.css'
  ],

  modules: [
    '@nuxt/fonts',
    '@nuxt/icon',
    '@nuxt/image',
    '@nuxt/ui',
    '@nuxtjs/mdc',
    '@vueuse/nuxt'
  ]
});
import { fileURLToPath } from 'node:url'

// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2025-07-15',
  devtools: { enabled: true },
  modules: [
    '@nuxtjs/tailwindcss'
  ],
  alias: {
    '~': fileURLToPath(new URL('./app', import.meta.url)),
    '@': fileURLToPath(new URL('./app', import.meta.url))
  },
  srcDir: 'app/',
  app: {
    pageTransition: { name: 'page', mode: 'out-in' }
  }
})

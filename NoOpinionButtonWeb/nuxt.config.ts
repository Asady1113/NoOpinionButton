// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2025-05-15',
  devtools: { enabled: true },

  runtimeConfig: {
    public: {
      apiBaseUrl: process.env.NUXT_PUBLIC_API_BASE_URL
    }
  },

  modules: ['@nuxtjs/tailwindcss'],
  tailwindcss: {
     cssPath: ["~/assets/css/tailwind.css", { injectPosition: "first" }], // Default
     config: {
       content: [
         "~/components/**/*.{js,vue,ts}",
         "~/layouts/**/*.vue",
         "~/pages/*.vue",
         "~/plugins/**/*.{js,ts}",
         "~/app.vue",
         "~/error.vue",
       ],
     },
     viewer: true,
   },
})
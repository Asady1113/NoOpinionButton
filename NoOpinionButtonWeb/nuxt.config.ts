// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2025-05-15',
  devtools: { enabled: true },

  // Static site generation configuration
  // 指定したページをビルド時にあらかじめ HTML にしておく
  nitro: {
    prerender: {
      routes: ['/signin', '/facilitator', '/participant']
    }
  },

  // Ensure static generation mode
  // サーバーサイドレンダリングを有効にする
  // 上で指定したページ以外は、ユーザーがページを開いたとき、サーバー側で HTML を作って送る
  // 今回はそのユースケースはないのと、S3に置くことになるので、この機能は使えない
  ssr: true,

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
  }
})
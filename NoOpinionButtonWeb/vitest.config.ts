// Vitest に設定を渡すための関数
import { defineConfig } from 'vitest/config'
// パス（ディレクトリの場所）を絶対パスに変換する関数
import { resolve } from 'path'
// Vue ファイル(.vue)を解析するためのプラグイン
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  resolve: {
    // エイリアス：プロジェクトの中で、ファイルの場所を簡単に呼べるニックネームみたいなもの
    alias: {
      // '~' と '@' は、プロジェクトのルートを指してる。
      // import '~/foo' と書けば プロジェクト直下の foo を読める
      '~': resolve(__dirname, '.'),
      '@': resolve(__dirname, '.'),
      // #appはてtests/mock/app.tsを指す
      '#app': resolve(__dirname, 'tests/mocks/app.ts'),
    },
  },
  test: {
    // ブラウザみたいな環境を Node.js 上で再現してくれるのが happy-dom
    environment: 'happy-dom',
    // どのテストでも勝手にグローバルで使える
    globals: true,
    // テストが始まる前に必ず読み込む初期設定ファイル。ここでモックを仕込んだり、グローバル設定をしたりする
    setupFiles: ['./tests/setup.ts']
  }
})
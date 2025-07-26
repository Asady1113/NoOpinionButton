/*
setup.tsは、テストが始まる前に必ず読み込まれて、以下のような役割を持つ
モック関数（偽物の関数）を用意する
それをグローバルで使えるようにする
毎回テストの前にモックをリセットする
*/

// vi は Vitest のモック機能のこと
import { vi } from 'vitest'

// グローバル変数を使うときはdeclare globalを使う
// fetchはimportするのではなく、グローバルのものを直接使うためここで指定する
// ReturnType<typeof vi.fn> は「vi.fn() っていう偽物関数が返す型だよ」という意味
declare global {
  var fetch: ReturnType<typeof vi.fn>
}

// Mock fetch API only (Nuxt composables are mocked via vi.mock())
global.fetch = vi.fn()

// すべてのテストケースの前に必ず実行されるお約束
// モック関数が「何回呼ばれたか」とかを全部リセット
// Setup global test environment
beforeEach(() => {
  vi.clearAllMocks()
})
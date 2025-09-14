<template>
  <!-- 
   @click="$emit('click')"
   → 親コンポーネントに click イベントを通知する
   :disabled="disabled || loading"
   → disabled または loading が true のときボタンを押せなくする。
   :aria-label="text"
   → スクリーンリーダー用のラベル。ボタンのテキストを読み上げさせる。
  -->
  <button
    @click="$emit('click')"
    :disabled="disabled || loading"
    :aria-label="text"
    class="px-6 py-2 bg-indigo-500 text-white rounded-full disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center min-w-[120px] transition-opacity"
  >
    <!--
    v-if="loading"
    → ローディング中は スピナーアイコン + テキスト を表示。

    <svg class="animate-spin ...">
    → Tailwindの animate-spin でくるくる回転するアイコン。
    → よくある「読み込み中インジケータ」。

    <span>{{ text }}</span>
    → ボタンラベル（例: "保存中..."）。
    -->
    <div v-if="loading" class="flex items-center">
      <svg
        class="animate-spin -ml-1 mr-2 h-4 w-4 text-white"
        xmlns="http://www.w3.org/2000/svg"
        fill="none"
        viewBox="0 0 24 24"
        aria-hidden="true"
      >
        <circle
          class="opacity-25"
          cx="12"
          cy="12"
          r="10"
          stroke="currentColor"
          stroke-width="4"
        ></circle>
        <path
          class="opacity-75"
          fill="currentColor"
          d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
        ></path>
      </svg>
      <span>{{ text }}</span>
    </div>
    <!--
    ローディングでないときは普通にテキストだけを表示。
    -->
    <span v-else>{{ text }}</span>
  </button>
</template>

<script setup lang="ts">
interface Props {
  disabled?: boolean
  loading?: boolean
  text: string
}

/*
props が「親 → 子」
emit は「子 → 親」

e: イベント名を表す引数
→イベント名はclickだよということを宣言してる

クリックされたら emit('click') で親に通知する
今回は引数がないので、呼ばれたことだけ親に通知
親は子からの emit('click') を @click="..." で受け取る → 親の関数が実行される
*/
interface Emits {
  (e: 'click'): void
}

defineProps<Props>()
defineEmits<Emits>()
</script>
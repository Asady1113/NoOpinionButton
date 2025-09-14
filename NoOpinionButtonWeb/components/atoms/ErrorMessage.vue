<template>
  <!-- role="alert" 
   　　 →スクリーンリーダーにアラートであることを伝える 
       :aria-live="visible ? 'polite' : 'off'"
       →メッセージが更新されたときにスクリーンリーダーが読み上げるかどうかを制御
       'polite': ユーザーの操作を邪魔しないタイミングで読み上げる
  -->
  <div
    v-if="visible && message"
    class="text-red-500 text-sm mt-2 transition-opacity"
    role="alert"
    :aria-live="visible ? 'polite' : 'off'"
  >
    {{ message }}
  </div>
  <div
    v-else-if="!visible"
    class="text-red-500 text-sm mt-2 opacity-0"
    aria-hidden="true"
  >
    &nbsp;
  </div>
</template>

<script setup lang="ts">
interface Props {
  message?: string
  visible?: boolean
}

defineProps<Props>()
</script>
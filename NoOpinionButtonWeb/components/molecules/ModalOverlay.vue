<template>
  <Teleport to="body">
  <!--
    Teleport なし → コンポーネントの親要素の中に描画される → レイアウト制約を受ける
    Teleport あり → body 直下に強制的に描画される → 画面全体を覆うモーダルが作れる
  -->
    <Transition
      name="modal"
      enter-active-class="transition-opacity duration-300"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition-opacity duration-300"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div
        v-if="visible"
        class="fixed inset-0 z-50 flex items-center justify-center p-4"
        @click="handleBackdropClick"
      >
        <!-- Backdrop -->
        <!--
        後ろを暗くして、モーダル本体に注目させる役割
        -->
        <div
          class="absolute inset-0 bg-black bg-opacity-50 backdrop-blur-sm"
          aria-hidden="true"
        ></div>
        
        <!-- Modal Content Container -->
        <!--
        背景（Backdrop）の上に配置される白いカード部分
        -->
        <div
          class="relative bg-white rounded-lg shadow-xl max-w-md w-full max-h-[90vh] overflow-y-auto"
          role="dialog"
          :aria-modal="visible"
          aria-labelledby="modal-title"
          @click.stop
        >
          <div class="p-6">
            <slot />
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { watch, onMounted, onUnmounted, nextTick } from 'vue'

interface Props {
  visible: boolean
  closeOnBackdropClick?: boolean
}

interface Emits {
  (e: 'close'): void
}

const props = withDefaults(defineProps<Props>(), {
  closeOnBackdropClick: false
})

const emit = defineEmits<Emits>()

function handleBackdropClick() {
  if (props.closeOnBackdropClick) {
    emit('close')
  }
}

// Focus management for accessibility
/*
フォーカス：Tabキーで操作すること
モーダルを開いたら、フォーカスをモーダルに移動させたい
→モーダルが表示されたらrole="dialog" の要素を探して、そこに .focus() でフォーカスを移す
*/
onMounted(() => {
  if (props.visible) {
    // Focus the modal when it becomes visible
    // nextTick；DOMが更新されたら呼ばれる
    nextTick(() => {
      const modal = document.querySelector('[role="dialog"]') as HTMLElement
      if (modal) {
        modal.focus()
      }
    })
  }
})

// Prevent body scroll when modal is open
/*
スクロールロック:モーダルが開いているときに、裏のページをスクロールできないようにする処理のこと
*/
watch(() => props.visible, (isVisible) => {
  if (typeof document !== 'undefined') {
    if (isVisible) {
      // 背景はスクロールできなくなる
      document.body.style.overflow = 'hidden'
    } else {
      // 背景スクロールが復活
      document.body.style.overflow = ''
    }
  }
})

// Cleanup on unmount
onUnmounted(() => {
  if (typeof document !== 'undefined') {
    document.body.style.overflow = ''
  }
})
</script>

<style scoped>
/* Additional responsive styles */
@media (max-width: 640px) {
  .max-w-md {
    max-width: 95vw;
  }
}
</style>
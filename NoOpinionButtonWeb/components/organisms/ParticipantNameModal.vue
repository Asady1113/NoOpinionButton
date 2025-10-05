<template>
  <ModalOverlay
    :visible="isVisible"
    :close-on-backdrop-click="false"
    @close="handleClose"
  >
    <div
      ref="modalContent"
      tabindex="-1"
      role="dialog"
      aria-labelledby="modal-title"
      :aria-modal="isVisible"
      aria-describedby="modal-description"
      @keydown="handleKeydown"
    >
      <ModalHeader
        id="modal-title"
        title="ユーザー名を入力してください"
        level="h2"
      />
      
      <div id="modal-description" class="sr-only">
        ユーザー名を入力して決定ボタンを押してください
      </div>
      
      <NameInputForm
        :participant-id="participantId"
        placeholder="ユーザー名を入力してください"
        :max-length="50"
        button-text="決定"
        @success="handleSuccess"
        @error="handleError"
      />
    </div>
  </ModalOverlay>
</template>

<script setup lang="ts">
import { ref, watch, nextTick, onMounted, onUnmounted } from 'vue'
import ModalOverlay from '~/components/molecules/ModalOverlay.vue'
import ModalHeader from '~/components/molecules/ModalHeader.vue'
import NameInputForm from '~/components/molecules/NameInputForm.vue'

interface Props {
  isVisible: boolean
  participantId: string
}

interface Emits {
  (e: 'close'): void
  (e: 'success'): void
  (e: 'error', message: string): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

// Template refs
const modalContent = ref<HTMLElement>()

// Store the previously focused element for restoration
// モーダルを開く前に、どの要素にフォーカスがあったか保存。モーダル閉じたら元に戻す
let previouslyFocusedElement: HTMLElement | null = null

// Handle success event from NameInputForm
function handleSuccess() {
  emit('success')
  handleClose()
}

// Handle error event from NameInputForm
function handleError(message: string) {
  emit('error', message)
}

// Handle modal close
function handleClose() {
  emit('close')
}

// Handle keyboard navigation
/*
Escape は閉じない
Tab / Shift+Tab のときフォーカスをモーダル内に閉じ込める
*/
function handleKeydown(event: KeyboardEvent) {
  if (event.key === 'Escape') {
    // Don't allow closing with Escape for this modal
    // as it's required for name registration
    event.preventDefault()
    return
  }
  
  if (event.key === 'Tab') {
    trapFocus(event)
  }
}

// Focus trap implementation
function trapFocus(event: KeyboardEvent) {
  if (!modalContent.value) return
  
  const focusableElements = modalContent.value.querySelectorAll(
    'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
  ) as NodeListOf<HTMLElement>
  
  const firstElement = focusableElements[0]
  const lastElement = focusableElements[focusableElements.length - 1]
  
  if (event.shiftKey) {
    // Shift + Tab
    if (document.activeElement === firstElement) {
      event.preventDefault()
      lastElement?.focus()
    }
  } else {
    // Tab
    if (document.activeElement === lastElement) {
      event.preventDefault()
      firstElement?.focus()
    }
  }
}

// Focus management when modal visibility changes
watch(() => props.isVisible, async (isVisible) => {
  if (isVisible) {
    // Store the currently focused element
    previouslyFocusedElement = document.activeElement as HTMLElement
    
    // Wait for the modal to be rendered
    await nextTick()
    
    // Focus the first focusable element in the modal
    if (modalContent.value) {
      const firstFocusable = modalContent.value.querySelector(
        'input, button, [href], select, textarea, [tabindex]:not([tabindex="-1"])'
      ) as HTMLElement
      
      if (firstFocusable) {
        firstFocusable.focus()
      } else {
        // Fallback to the modal container itself
        modalContent.value.focus()
      }
    }
  } else {
    // Restore focus to the previously focused element
    if (previouslyFocusedElement) {
      previouslyFocusedElement.focus()
      previouslyFocusedElement = null
    }
  }
})

// Cleanup on unmount
onUnmounted(() => {
  if (previouslyFocusedElement) {
    previouslyFocusedElement.focus()
  }
})
</script>

<style scoped>
/* Screen reader only class for accessibility */
.sr-only {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border: 0;
}
</style>
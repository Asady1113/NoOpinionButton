<template>
  <div class="border-t bg-white">
    <!-- エラーメッセージ表示 - 既存のApiErrorMessageコンポーネントを使用 -->
    <div v-if="messageSending.state.value.error" class="px-4 pt-4">
      <ApiErrorMessage 
        :error="messageSending.state.value.error" 
        :dismissible="true"
        @dismiss="handleErrorDismiss" 
      />
    </div>

    <!-- メッセージ送信フォーム -->
    <form @submit.prevent="handleSubmit" class="flex items-center space-x-2 p-4">
      <!-- 入力フィールド -->
      <input
        :value="messageSending.state.value.content"
        @input="handleInput"
        @keydown="handleKeydown"
        :disabled="disabled || messageSending.state.value.isSubmitting"
        :placeholder="placeholder"
        class="flex-1 px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent disabled:opacity-50 disabled:cursor-not-allowed"
      />
      
      <!-- 送信ボタン -->
      <button
        type="submit"
        :disabled="!messageSending.state.value.canSubmit || disabled"
        class="w-12 h-12 bg-blue-500 hover:bg-blue-600 disabled:bg-gray-300 disabled:cursor-not-allowed rounded-lg flex items-center justify-center transition-colors"
        :aria-label="messageSending.state.value.isSubmitting ? '送信中...' : 'メッセージを送信'"
      >
        <!-- ローディング中のスピナー -->
        <svg
          v-if="messageSending.state.value.isSubmitting"
          class="animate-spin w-6 h-6 text-white"
          xmlns="http://www.w3.org/2000/svg"
          fill="none"
          viewBox="0 0 24 24"
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
        
        <!-- 送信アイコン（青色三角形） -->
        <svg
          v-else
          class="w-6 h-6 text-white"
          fill="currentColor"
          viewBox="0 0 24 24"
        >
          <path d="M2.01 21L23 12 2.01 3 2 10l15 2-15 2z"/>
        </svg>
      </button>
    </form>
  </div>
</template>

<script setup lang="ts">
import { useMessageSending } from '~/composables/message/useMessageSending'
import ApiErrorMessage from '~/components/atoms/ApiErrorMessage.vue'

interface Props {
  meetingId: string
  participantId: string
  disabled?: boolean
  placeholder?: string
}

const props = withDefaults(defineProps<Props>(), {
  disabled: false,
  placeholder: '匿名意見を入力してください'
})

// Composable for message sending functionality
const messageSending = useMessageSending()

// Handle input changes
function handleInput(event: Event) {
  const target = event.target as HTMLInputElement
  messageSending.updateContent(target.value)
}

// Handle keydown events
function handleKeydown(event: KeyboardEvent) {
  if (event.key === 'Enter') {
    if (event.shiftKey) {
      // Shift+Enter: 送信
      event.preventDefault()
      handleSubmit()
    } else {
      // Enter only: フォーム送信を防ぐ
      event.preventDefault()
    }
  }
}

// Handle form submission
async function handleSubmit() {
  if (!messageSending.state.value.canSubmit || props.disabled) {
    return
  }

  try {
    await messageSending.submitMessage(props.meetingId, props.participantId)
  } catch (error) {
    // Error is already handled by the composable and will be displayed via ApiErrorMessage
    console.error('Message sending failed:', error)
  }
}

// Handle error dismissal
function handleErrorDismiss() {
  // API errors will be cleared automatically on next request
}
</script>
<template>
  <div
    v-if="visible && error"
    class="flex items-start space-x-2 p-3 rounded-lg border"
    :class="errorClasses"
    role="alert"
    :aria-live="visible ? 'polite' : 'off'"
  >
    <!-- エラーアイコン -->
    <div class="flex-shrink-0">
      <svg 
        class="w-5 h-5"
        :class="iconClasses"
        fill="currentColor" 
        viewBox="0 0 20 20"
      >
        <path 
          fill-rule="evenodd" 
          d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" 
          clip-rule="evenodd" 
        />
      </svg>
    </div>
    
    <!-- エラーメッセージ -->
    <div class="flex-1 min-w-0">
      <p class="text-sm font-medium" :class="titleClasses">
        {{ errorTitle }}
      </p>
      <p v-if="errorMessage" class="text-sm mt-1" :class="messageClasses">
        {{ errorMessage }}
      </p>
      
      <!-- 再試行ボタン（オプション） -->
      <div v-if="showRetryButton && onRetry" class="mt-2">
        <button
          @click="handleRetry"
          class="text-sm font-medium underline hover:no-underline focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500"
          :class="retryButtonClasses"
        >
          再試行
        </button>
      </div>
    </div>
    
    <!-- 閉じるボタン（オプション） -->
    <div v-if="dismissible" class="flex-shrink-0">
      <button
        @click="handleDismiss"
        class="rounded-md p-1 hover:bg-red-100 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500"
        :class="closeButtonClasses"
      >
        <span class="sr-only">閉じる</span>
        <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
          <path 
            fill-rule="evenodd" 
            d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" 
            clip-rule="evenodd" 
          />
        </svg>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { ApiError } from '~/types/error'

interface Props {
  error?: ApiError | null
  visible?: boolean
  dismissible?: boolean
  showRetryButton?: boolean
  variant?: 'error' | 'warning' | 'info'
}

interface Emits {
  (e: 'dismiss'): void
  (e: 'retry'): void
}

const props = withDefaults(defineProps<Props>(), {
  error: null,
  visible: true,
  dismissible: false,
  showRetryButton: false,
  variant: 'error'
})

const emit = defineEmits<Emits>()

// エラータイトルの計算
const errorTitle = computed(() => {
  if (!props.error) return ''
  
  switch (props.error.type) {
    case 'WebSocketConnection':
      return '接続エラー'
    case 'WebSocketMessage':
      return 'メッセージエラー'
    case 'BadRequest':
      return '入力エラー'
    case 'Unauthorized':
      return '認証エラー'
    case 'NotFound':
      return '見つかりません'
    case 'Server':
      return 'サーバーエラー'
    default:
      return 'エラー'
  }
})

// エラーメッセージの計算
const errorMessage = computed(() => {
  if (!props.error) return ''
  
  // より具体的なエラーメッセージを提供
  switch (props.error.type) {
    case 'WebSocketConnection':
      if (props.error.message.includes('timeout')) {
        return '接続がタイムアウトしました。ネットワーク接続を確認してください。'
      } else if (props.error.message.includes('failed')) {
        return 'サーバーに接続できませんでした。しばらく待ってから再試行してください。'
      } else if (props.error.message.includes('再試行回数')) {
        return '接続の再試行回数が上限に達しました。ページを再読み込みしてください。'
      } else {
        return 'WebSocket接続でエラーが発生しました。'
      }
    case 'WebSocketMessage':
      if (props.error.message.includes('形式が正しくありません')) {
        return '受信したメッセージの形式が正しくありません。'
      } else if (props.error.message.includes('処理中にエラー')) {
        return 'メッセージの処理中にエラーが発生しました。'
      } else {
        return 'メッセージの受信でエラーが発生しました。'
      }
    default:
      return props.error.message || '不明なエラーが発生しました。'
  }
})

// スタイルクラスの計算
const errorClasses = computed(() => {
  switch (props.variant) {
    case 'warning':
      return 'bg-yellow-50 border-yellow-200'
    case 'info':
      return 'bg-blue-50 border-blue-200'
    default:
      return 'bg-red-50 border-red-200'
  }
})

const iconClasses = computed(() => {
  switch (props.variant) {
    case 'warning':
      return 'text-yellow-400'
    case 'info':
      return 'text-blue-400'
    default:
      return 'text-red-400'
  }
})

const titleClasses = computed(() => {
  switch (props.variant) {
    case 'warning':
      return 'text-yellow-800'
    case 'info':
      return 'text-blue-800'
    default:
      return 'text-red-800'
  }
})

const messageClasses = computed(() => {
  switch (props.variant) {
    case 'warning':
      return 'text-yellow-700'
    case 'info':
      return 'text-blue-700'
    default:
      return 'text-red-700'
  }
})

const retryButtonClasses = computed(() => {
  switch (props.variant) {
    case 'warning':
      return 'text-yellow-800 hover:text-yellow-900'
    case 'info':
      return 'text-blue-800 hover:text-blue-900'
    default:
      return 'text-red-800 hover:text-red-900'
  }
})

const closeButtonClasses = computed(() => {
  switch (props.variant) {
    case 'warning':
      return 'text-yellow-400 hover:text-yellow-500'
    case 'info':
      return 'text-blue-400 hover:text-blue-500'
    default:
      return 'text-red-400 hover:text-red-500'
  }
})

// 再試行ボタンの表示判定
const onRetry = computed(() => {
  return props.error?.type === 'WebSocketConnection'
})

// イベントハンドラー
const handleDismiss = () => {
  emit('dismiss')
}

const handleRetry = () => {
  emit('retry')
}
</script>
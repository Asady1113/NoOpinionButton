<template>
  <div class="flex flex-col h-full">
    <!-- 接続状態インジケーター -->
    <div class="flex items-center space-x-2 p-3 bg-gray-50 border-b border-gray-200 flex-shrink-0">
      <div class="flex items-center space-x-2">
        <!-- 接続状態アイコン -->
        <div 
          class="w-3 h-3 rounded-full"
          :class="{
            'bg-green-500': isConnected,
            'bg-yellow-500': isConnecting,
            'bg-red-500': !isConnected && !isConnecting
          }"
        ></div>
        
        <!-- 接続状態テキスト -->
        <span class="text-sm font-medium text-gray-700">
          {{ connectionStatusText }}
        </span>
      </div>
      
      <!-- エラーメッセージ -->
      <div v-if="hasError" class="ml-2 max-w-md">
        <ApiErrorMessage
          :error="primaryError"
          :visible="true"
          :dismissible="false"
        />
      </div>
    </div>

    <!-- メッセージリスト -->
    <div 
      ref="messageListContainer"
      class="flex-1 overflow-y-auto p-4 space-y-2"
      :class="{
        'opacity-50': !isConnected && !isConnecting
      }"
    >
      <!-- メッセージが無い場合の表示 -->
      <div 
        v-if="messages.length === 0" 
        class="flex items-center justify-center h-full text-gray-500 text-sm"
      >
        <div class="text-center">
          <div class="mb-2">📝</div>
          <div>メッセージはまだありません</div>
          <div v-if="isConnected" class="text-xs mt-1">
            新しいメッセージを待っています...
          </div>
        </div>
      </div>

      <!-- メッセージアイテム -->
      <MessageItem
        v-for="message in messages"
        :key="message.id"
        :participant-id="message.participantId"
        :participant-name="message.participantName"
        :content="message.content"
        :created-at="message.createdAt"
        :show-timestamp="showTimestamps"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, nextTick, ref, watch } from 'vue'
import MessageItem from '~/components/molecules/MessageItem.vue'
import ApiErrorMessage from '~/components/atoms/ApiErrorMessage.vue'
import type { ApiError } from '~/types/error'

interface MessageData {
  id: string
  meetingId: string
  participantId: string
  participantName: string
  content: string
  createdAt: string
  likeCount: number
  reportedCount: number
  isActive: boolean
}

interface Props {
  messages: MessageData[]
  isConnected: boolean
  isConnecting?: boolean
  connectionError?: ApiError | null
  messageError?: ApiError | null
  showTimestamps?: boolean
  autoScroll?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  isConnecting: false,
  connectionError: null,
  messageError: null,
  showTimestamps: false,
  autoScroll: true
})

const messageListContainer = ref<HTMLElement>()

// 接続状態テキストの計算
const connectionStatusText = computed(() => {
  if (props.isConnecting) {
    return '接続中...'
  } else if (props.isConnected) {
    return '接続済み'
  } else {
    return '切断中'
  }
})

// エラー表示の計算
const hasError = computed(() => {
  return !!(props.connectionError || props.messageError)
})

// 優先表示するエラーの計算（接続エラーを優先）
const primaryError = computed(() => {
  return props.connectionError || props.messageError || null
})



// 自動スクロール機能
const scrollToBottom = async () => {
  if (!props.autoScroll || !messageListContainer.value) {
    return
  }

  await nextTick()
  
  const container = messageListContainer.value
  container.scrollTop = container.scrollHeight
}

// メッセージが追加されたときに自動スクロール
watch(
  () => props.messages.length,
  async (newLength, oldLength) => {
    // 新しいメッセージが追加された場合のみスクロール
    if (newLength > oldLength) {
      await scrollToBottom()
    }
  }
)

// 接続状態が変わったときの処理
watch(
  () => props.isConnected,
  async (isConnected) => {
    if (isConnected) {
      // 接続が確立されたときに最下部にスクロール
      await scrollToBottom()
    }
  }
)
</script>
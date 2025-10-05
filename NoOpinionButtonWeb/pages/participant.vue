<template>
  <div class="h-screen flex flex-col">
    <!-- Participant Name Registration Modal -->
    <ParticipantNameModal
      :is-visible="showNameModal"
      :participant-id="signInStore.id"
      @close="handleModalClose"
      @success="handleNameRegistrationSuccess"
      @error="handleNameRegistrationError"
    />
    
    <!-- Main participant page content -->
    <div 
      :class="{ 'pointer-events-none': showNameModal }"
      class="flex-1 flex flex-col"
    >
      <!-- Page Header -->
      <div class="bg-white border-b border-gray-200 px-4 py-3 flex-shrink-0">
        <h1 class="text-lg font-semibold text-gray-900">参加者ページ</h1>
        <p v-if="signInStore.meetingName" class="text-sm text-gray-600 mt-1">
          {{ signInStore.meetingName }}
        </p>
      </div>

      <!-- Message List -->
      <div class="flex-1 min-h-0">
        <MessageList
          v-if="nameRegistrationCompleted"
          :messages="messageReception.state.value.messages"
          :is-connected="webSocketConnection.state.value.isConnected"
          :is-connecting="webSocketConnection.state.value.isConnecting"
          :connection-error="webSocketConnection.state.value.error"
          :message-error="messageReception.state.value.error"
          :auto-scroll="true"
          :show-timestamps="false"
        />
        
        <!-- Loading state while connecting -->
        <div 
          v-else-if="nameRegistrationCompleted && webSocketConnection.state.value.isConnecting"
          class="flex items-center justify-center h-full text-gray-500"
        >
          <div class="text-center">
            <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500 mx-auto mb-2"></div>
            <div>接続中...</div>
          </div>
        </div>
        
        <!-- Placeholder when name registration not completed -->
        <div 
          v-else
          class="flex items-center justify-center h-full text-gray-500"
        >
          <div class="text-center">
            <div class="mb-2">👋</div>
            <div>参加者名を登録してください</div>
          </div>
        </div>
      </div>

      <!-- Message Sending Form -->
      <MessageSendingForm
        v-if="nameRegistrationCompleted && signInStore.meetingId && signInStore.id"
        :meeting-id="signInStore.meetingId"
        :participant-id="signInStore.id"
        :disabled="!webSocketConnection.state.value.isConnected"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch, onUnmounted } from 'vue'
import { useSignInStore } from '~/composables/signIn/useSignIn'
import { useWebSocketConnection } from '~/composables/webSocket/useWebSocketConnection'
import { useMessageReception } from '~/composables/message/useMessageReception'
import ParticipantNameModal from '~/components/organisms/ParticipantNameModal.vue'
import MessageList from '~/components/organisms/MessageList.vue'
import MessageSendingForm from '~/components/molecules/MessageSendingForm.vue'

const signInStore = useSignInStore()

// WebSocket connection and message reception
const webSocketConnection = useWebSocketConnection()
const messageReception = useMessageReception()

// Modal visibility state
// モーダル表示の状態
// 名前登録が完了したかどうか
const showNameModal = ref(false)
const nameRegistrationCompleted = ref(false)

// Computed property to check if participant ID exists
// サインインしているかどうか
// ID が空でない場合 true
const hasParticipantId = computed(() => {
  return signInStore.value.id && signInStore.value.id.trim() !== ''
})

// Show modal logic based on sign-in state
// ID があり、かつ名前登録がまだならモーダルを表示
const shouldShowModal = computed(() => {
  return hasParticipantId.value && !nameRegistrationCompleted.value
})

// WebSocket connection setup
const setupWebSocketConnection = async () => {
  try {
    const meetingId = signInStore.value.meetingId
    const participantId = signInStore.value.id

    if (!meetingId || !participantId) {
      console.error('Missing meetingId or participantId for WebSocket connection')
      return
    }

    console.log('Starting WebSocket connection...', { meetingId, participantId })

    // Set up message handler before connecting
    webSocketConnection.onMessage((data) => {
      console.log('Received WebSocket message:', data)
      // messageReceptionのhandleWebSockectMessageメソッドを、メッセージが受信されたタイミングで実行するように指定
      messageReception.handleWebSocketMessage(data)
    })

    // Set up error handler
    webSocketConnection.onError((error) => {
      console.error('WebSocket error:', error)
    })

    // Connect to WebSocket
    await webSocketConnection.connect(meetingId, participantId)
    console.log('WebSocket connection established successfully')

  } catch (error) {
    console.error('Failed to establish WebSocket connection:', error)
  }
}

// Handle modal close event
function handleModalClose() {
  // For this required modal, we don't allow closing without completing registration
  // The modal will only close on successful registration
  // このモーダルは 必須登録モーダル のため、閉じることは許可しない
  // ユーザーが閉じようとしてもログだけ出す
  console.log('Modal close attempted - registration required')
}

// Handle successful name registration
async function handleNameRegistrationSuccess() {
  console.log('Name registration successful')
  nameRegistrationCompleted.value = true
  showNameModal.value = false
  
  // Start WebSocket connection after successful name registration
  await setupWebSocketConnection()
}

// Handle name registration error
// 登録失敗 → モーダルは開いたまま、再入力可能
function handleNameRegistrationError(message: string) {
  console.error('Name registration error:', message)
  // Modal stays open on error to allow retry
}

// Initialize modal visibility on component mount
// ページ読み込み時に条件をチェックしてモーダルを開く
onMounted(() => {
  if (shouldShowModal.value) {
    showNameModal.value = true
  }
})

// Watch for changes in sign-in state
// ページ読み込み時に条件をチェックしてモーダルを開く
watch(() => shouldShowModal.value, (shouldShow) => {
  if (shouldShow) {
    showNameModal.value = true
  }
})

// Cleanup WebSocket connection on component unmount
onUnmounted(() => {
  webSocketConnection.disconnect()
})

console.log('受け取った情報:', signInStore.value)
</script>
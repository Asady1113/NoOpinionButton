<template>
  <div>
    <!-- Participant Name Registration Modal -->
    <ParticipantNameModal
      :is-visible="showNameModal"
      :participant-id="signInStore.id"
      @close="handleModalClose"
      @success="handleNameRegistrationSuccess"
      @error="handleNameRegistrationError"
    />
    
    <!-- Main participant page content -->
     <!-- 
     モーダルが表示されている間は 背面のページをクリック不可にする
     -->
    <div :class="{ 'pointer-events-none': showNameModal }">
      参加者ページ
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useSignInStore } from '~/composables/signIn/useSignIn'
import ParticipantNameModal from '~/components/organisms/ParticipantNameModal.vue'

const signInStore = useSignInStore()

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

// Handle modal close event
function handleModalClose() {
  // For this required modal, we don't allow closing without completing registration
  // The modal will only close on successful registration
  // このモーダルは 必須登録モーダル のため、閉じることは許可しない
  // ユーザーが閉じようとしてもログだけ出す
  console.log('Modal close attempted - registration required')
}

// Handle successful name registration
function handleNameRegistrationSuccess() {
  console.log('Name registration successful')
  nameRegistrationCompleted.value = true
  showNameModal.value = false
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

console.log('受け取った情報:', signInStore.value)
</script>
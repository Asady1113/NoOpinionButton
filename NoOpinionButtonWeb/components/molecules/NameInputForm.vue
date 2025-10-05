<template>
  <!--
  HTMLでは、デフォルトではボタンを押すと ページ全体がリロードされてフォームのデータが送信されるが、
  通常のページリロードやサーバー送信をキャンセルして、自分の関数 handleSubmit を呼ぶ
  -->
  <form @submit.prevent="handleSubmit" class="space-y-4">
    <!--
    nameInputが更新されると、子コンポーネントからそれが通知され、
    validateInputが呼ばれる
    -->
    <TextInput
      v-model="nameInput"
      :placeholder="placeholder"
      :max-length="maxLength"
      :disabled="isLoading"
      @input="validateInput"
    />
    
    <ErrorMessage
      :message="errorMessage"
      :visible="!!errorMessage"
    />
    
    <!--
    @click="handleSubmit"
    は子コンポーネントで発火されると呼ばれる
    -->
    <div class="flex justify-center">
      <SubmitButton
        :text="buttonText"
        :disabled="!isValidInput || isLoading"
        :loading="isLoading"
        @click="handleSubmit"
      />
    </div>
  </form>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useParticipantNameApi } from '~/composables/participantName/useParticipantNameApi'
import type { ApiError } from '~/types/error'
import { ApiErrorType } from '~/types/error'
import TextInput from '~/components/atoms/TextInput.vue'
import ErrorMessage from '~/components/atoms/ErrorMessage.vue'
import SubmitButton from '~/components/atoms/SubmitButton.vue'

interface Props {
  participantId: string
  placeholder?: string
  maxLength?: number
  buttonText?: string
}

interface Emits {
  (e: 'success'): void
  (e: 'error', message: string): void
}

const props = withDefaults(defineProps<Props>(), {
  placeholder: 'ユーザー名を入力してください',
  maxLength: 50,
  buttonText: '決定'
})

const emit = defineEmits<Emits>()

// Composables
const { updateParticipantName } = useParticipantNameApi()

// Reactive state
const nameInput = ref('')
const errorMessage = ref('')
const isLoading = ref(false)

// Computed properties with enhanced validation
const isValidInput = computed(() => {
  const inputValue = nameInput.value
  const trimmedName = inputValue.trim()
  
  // Must have content after trimming
  if (trimmedName.length === 0) {
    return false
  }
  
  // Must be within length limits (using character count)
  if (trimmedName.length > props.maxLength) {
    return false
  }
  
  // Must have at least 1 character
  if (trimmedName.length < 1) {
    return false
  }
  
  return true
})

// Validation function with enhanced real-time feedback
function validateInput() {
  const inputValue = nameInput.value
  const trimmedName = inputValue.trim()
  
  // Clear error message for empty input (no validation error shown until user starts typing)
  if (inputValue.length === 0) {
    errorMessage.value = ''
    return
  }
  
  // Check for whitespace-only input
  if (trimmedName.length === 0) {
    errorMessage.value = '有効な名前を入力してください'
    return
  }
  
  // Check for length validation (using character count, not byte count)
  if (trimmedName.length > props.maxLength) {
    errorMessage.value = `名前は${props.maxLength}文字以内で入力してください`
    return
  }
  
  // Check minimum length (at least 1 character after trimming)
  if (trimmedName.length < 1) {
    errorMessage.value = '名前を入力してください'
    return
  }
  
  // All validation passed
  errorMessage.value = ''
}

// Enhanced validation for form submission
function validateForSubmission(): { isValid: boolean; errorMessage: string } {
  const inputValue = nameInput.value
  const trimmedName = inputValue.trim()
  
  // Check for empty input
  if (inputValue.length === 0) {
    return {
      isValid: false,
      errorMessage: '名前を入力してください'
    }
  }
  
  // Check for whitespace-only input
  if (trimmedName.length === 0) {
    return {
      isValid: false,
      errorMessage: '有効な名前を入力してください'
    }
  }
  
  // Check minimum length
  if (trimmedName.length < 1) {
    return {
      isValid: false,
      errorMessage: '名前を入力してください'
    }
  }
  
  // Check maximum length
  if (trimmedName.length > props.maxLength) {
    return {
      isValid: false,
      errorMessage: `名前は${props.maxLength}文字以内で入力してください`
    }
  }
  
  return {
    isValid: true,
    errorMessage: ''
  }
}

// Form submission handler with enhanced validation
async function handleSubmit() {
  if (isLoading.value) {
    return
  }

  // Perform comprehensive validation before submission
  const validation = validateForSubmission()
  
  if (!validation.isValid) {
    errorMessage.value = validation.errorMessage
    return
  }

  const trimmedName = nameInput.value.trim()
  isLoading.value = true
  errorMessage.value = ''

  try {
    const response = await updateParticipantName(props.participantId, trimmedName)
    
    if (response.Error) {
      errorMessage.value = response.Error
      emit('error', response.Error)
    } else {
      emit('success')
    }
  } catch (error: unknown) {
    const apiError = error as ApiError
    let userMessage = 'エラーが発生しました。しばらく時間をおいて再度お試しください。'
    
    switch (apiError.type) {
      case ApiErrorType.BadRequest:
        userMessage = '入力内容に問題があります。名前を確認してください。'
        break
      case ApiErrorType.Unauthorized:
        userMessage = '認証に失敗しました。ページを再読み込みしてください。'
        break
      case ApiErrorType.NotFound:
        userMessage = '参加者が見つかりません。ページを再読み込みしてください。'
        break
      case ApiErrorType.Server:
        // Use the specific message from the API error if available
        if (apiError.message) {
          userMessage = apiError.message
        } else if (apiError.statusCode >= 500) {
          userMessage = 'サーバーエラーが発生しました。しばらく時間をおいて再度お試しください。'
        } else {
          userMessage = 'エラーが発生しました。しばらく時間をおいて再度お試しください。'
        }
        break
      default:
        userMessage = 'ネットワークエラーが発生しました。インターネット接続を確認してください。'
        break
    }
    
    errorMessage.value = userMessage
    emit('error', userMessage)
  } finally {
    isLoading.value = false
  }
}
</script>
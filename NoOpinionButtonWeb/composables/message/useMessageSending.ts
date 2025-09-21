import { ref, computed } from 'vue'
import { useMessageSendingApi } from './useMessageSendingApi'
import type { ApiError } from '~/types/error'

// TypeScript interfaces for form state management
export interface MessageSendingState {
    content: string
    isSubmitting: boolean
    error: ApiError | null
    canSubmit: boolean
}

export function useMessageSending() {
    const { sendMessage, isLoading: apiLoading, error: apiError } = useMessageSendingApi()

    // Form state
    const content = ref('')

    // Computed properties for state management
    const isSubmitting = computed(() => apiLoading?.value ?? false)
    const canSubmit = computed(() => {
        const trimmedContent = content.value.trim()
        return trimmedContent.length > 0 && trimmedContent.length <= 500 && !isSubmitting.value
    })

    // Combined error state - use API errors only
    const currentError = computed(() => {
        return apiError?.value
    })

    // Combined state object
    const state = computed<MessageSendingState>(() => ({
        content: content.value,
        isSubmitting: isSubmitting.value,
        error: currentError.value,
        canSubmit: canSubmit.value
    }))

    // Update content function
    function updateContent(newContent: string): void {
        content.value = newContent
    }

    // Submit message function
    async function submitMessage(meetingId: string, participantId: string): Promise<void> {
        try {
            await sendMessage({
                meetingId,
                participantId,
                content: content.value.trim()
            })

            // Clear form and reset state on successful submission
            clearForm()
        } catch (error) {
            // API errors are already handled by useMessageSendingApi
            // The error will be available through apiError.value
            throw error
        }
    }

    // Clear form function - resets all form state
    function clearForm(): void {
        content.value = ''
    }

    return {
        state,
        updateContent,
        submitMessage,
        clearForm
    }
}
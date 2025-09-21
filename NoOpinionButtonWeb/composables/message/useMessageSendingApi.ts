import { ref, readonly } from 'vue'
import { useRuntimeConfig } from '#app'
import { ApiErrorType } from '~/types/error'
import type { ApiError } from '~/types/error'

// TypeScript interfaces for request/response types
export interface MessageSendingRequest {
    meetingId: string
    participantId: string
    content: string
}

export interface MessageSendingResponse {
    messageId?: string
    error?: string
}

export function useMessageSendingApi() {
    const config = useRuntimeConfig()
    const isLoading = ref(false)
    const error = ref<ApiError | null>(null)

    async function sendMessage(request: MessageSendingRequest): Promise<MessageSendingResponse> {
        isLoading.value = true
        error.value = null

        try {
            const response = await fetch(`${config.public.apiBaseUrl}/message`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(request)
            })

            if (!response.ok) {
                let type: ApiErrorType

                switch (response.status) {
                    case 400:
                        type = ApiErrorType.BadRequest
                        break
                    case 401:
                        type = ApiErrorType.Unauthorized
                        break
                    case 404:
                        type = ApiErrorType.NotFound
                        break
                    default:
                        type = ApiErrorType.Server
                        break
                }

                const responseBody = await response.json()

                const apiError: ApiError = {
                    type,
                    message: responseBody?.Error ?? `Request failed with status ${response.status}`,
                    statusCode: response.status,
                }

                error.value = apiError
                throw apiError
            }

            const result = await response.json()
            return result
        } catch (err: unknown) {
            // Handle network errors and other non-HTTP errors
            if (err instanceof TypeError) {
                const networkError: ApiError = {
                    type: ApiErrorType.Server,
                    message: 'ネットワークエラーが発生しました。インターネット接続を確認してください。',
                    statusCode: 0,
                }
                error.value = networkError
                throw networkError
            }
            throw err
        } finally {
            isLoading.value = false
        }
    }

    return {
        sendMessage,
        isLoading: readonly(isLoading),
        error: readonly(error)
    }
}
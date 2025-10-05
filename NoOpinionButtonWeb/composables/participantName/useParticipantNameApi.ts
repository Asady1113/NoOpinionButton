import { useRuntimeConfig } from '#app'
import { ApiErrorType } from '~/types/error'
import type { ApiError } from '~/types/error'

// TypeScript interfaces for request/response types
export interface UpdateParticipantNameRequest {
    name: string
}

export interface UpdateParticipantNameResponse {
    Data: {
        updatedName: string
    } | null
    Error: string | null
}

export function useParticipantNameApi() {
    const config = useRuntimeConfig()

    async function updateParticipantName(participantId: string, name: string): Promise<UpdateParticipantNameResponse> {
        // Create AbortController for timeout handling
        const controller = new AbortController()
        const timeoutId = setTimeout(() => controller.abort(), 30000) // 30 second timeout

        try {
            const requestBody: UpdateParticipantNameRequest = { name }

            const response = await fetch(`${config.public.apiBaseUrl}/participants/${participantId}/name`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(requestBody),
                signal: controller.signal
            })

            clearTimeout(timeoutId)

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

                const error: ApiError = {
                    type,
                    message: responseBody?.Error ?? `Request failed with status ${response.status}`,
                    statusCode: response.status,
                }

                throw error
            }

            return await response.json()
        } catch (error: unknown) {
            clearTimeout(timeoutId)
            
            // Handle AbortError (timeout)
            if (error instanceof DOMException && error.name === 'AbortError') {
                const timeoutError: ApiError = {
                    type: ApiErrorType.Server,
                    message: 'リクエストがタイムアウトしました。しばらく時間をおいて再度お試しください。',
                    statusCode: 0,
                }
                throw timeoutError
            }

            // Handle network errors and other non-HTTP errors
            if (error instanceof TypeError) {
                const networkError: ApiError = {
                    type: ApiErrorType.Server,
                    message: 'ネットワークエラーが発生しました。インターネット接続を確認してください。',
                    statusCode: 0,
                }
                throw networkError
            }

            // Re-throw API errors (already typed as ApiError from the response handling above)
            throw error
        }
    }

    return { updateParticipantName }
}
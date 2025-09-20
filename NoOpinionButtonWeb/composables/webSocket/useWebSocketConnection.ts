import { ref, onUnmounted, type Ref } from 'vue'
import { ApiErrorType, type ApiError } from '~/types/error'

interface WebSocketConnectionState {
    isConnected: boolean
    isConnecting: boolean
    error: ApiError | null
    reconnectAttempts: number
}

interface WebSocketConnection {
    state: Ref<WebSocketConnectionState>
    connect: (meetingId: string, participantId: string) => Promise<void>
    disconnect: () => void
    send: (message: any) => void
    onMessage: (callback: (data: any) => void) => void
    onError: (callback: (error: ApiError) => void) => void
    getErrorMessage: (error: ApiError) => string
}

export const useWebSocketConnection = (): WebSocketConnection => {
    const state = ref<WebSocketConnectionState>({
        isConnected: false,
        isConnecting: false,
        error: null,
        reconnectAttempts: 0
    })

    let websocket: WebSocket | null = null
    let messageCallback: ((data: any) => void) | null = null
    let errorCallback: ((error: ApiError) => void) | null = null
    let reconnectTimeoutId: NodeJS.Timeout | null = null
    let currentMeetingId: string | null = null
    let currentParticipantId: string | null = null

    const MAX_RECONNECT_ATTEMPTS = 5
    const BASE_RECONNECT_DELAY = 1000 // 1 second
    const WEBSOCKET_URL = 'wss://0xjupqx66b.execute-api.ap-northeast-1.amazonaws.com/prod/'

    const calculateReconnectDelay = (attempt: number): number => {
        // Exponential backoff: 1s, 2s, 4s, 8s, 16s
        return BASE_RECONNECT_DELAY * Math.pow(2, attempt)
    }

    const createApiError = (type: ApiErrorType, message: string, statusCode: number = 0): ApiError => {
        return {
            type,
            message,
            statusCode
        }
    }

    const getErrorMessage = (error: ApiError): string => {
        switch (error.type) {
            case ApiErrorType.WebSocketConnection:
                if (error.message.includes('timeout')) {
                    return '接続がタイムアウトしました。ネットワーク接続を確認してください。'
                } else if (error.message.includes('failed')) {
                    return 'サーバーに接続できませんでした。しばらく待ってから再試行してください。'
                } else {
                    return 'WebSocket接続でエラーが発生しました。'
                }
            case ApiErrorType.WebSocketMessage:
                return 'メッセージの送受信でエラーが発生しました。'
            default:
                return error.message || '不明なエラーが発生しました。'
        }
    }

    const clearReconnectTimeout = () => {
        if (reconnectTimeoutId) {
            clearTimeout(reconnectTimeoutId)
            reconnectTimeoutId = null
        }
    }

    const handleWebSocketOpen = () => {
        state.value.isConnected = true
        state.value.isConnecting = false
        state.value.error = null
        state.value.reconnectAttempts = 0
        console.log('WebSocket connected successfully')
    }

    const handleWebSocketMessage = (event: MessageEvent) => {
        try {
            const data = JSON.parse(event.data)
            if (messageCallback) {
                messageCallback(data)
            }
        } catch (error) {
            console.error('Failed to parse WebSocket message:', error)
            const apiError = createApiError(
                ApiErrorType.WebSocketMessage,
                'メッセージの解析に失敗しました'
            )
            state.value.error = apiError
            
            if (errorCallback) {
                errorCallback(apiError)
            }
        }
    }

    const handleWebSocketError = (event: Event) => {
        console.error('WebSocket error:', event)
        const apiError = createApiError(
            ApiErrorType.WebSocketConnection,
            'WebSocket接続でエラーが発生しました'
        )
        state.value.error = apiError

        if (errorCallback) {
            errorCallback(apiError)
        }
    }

    const handleWebSocketClose = (event: CloseEvent) => {
        state.value.isConnected = false
        state.value.isConnecting = false

        console.log('WebSocket closed:', event.code, event.reason)

        // Only attempt reconnection if it wasn't a clean close and we haven't exceeded max attempts
        if (!event.wasClean && state.value.reconnectAttempts < MAX_RECONNECT_ATTEMPTS) {
            const delay = calculateReconnectDelay(state.value.reconnectAttempts)
            console.log(`Attempting reconnection in ${delay}ms (attempt ${state.value.reconnectAttempts + 1}/${MAX_RECONNECT_ATTEMPTS})`)

            reconnectTimeoutId = setTimeout(() => {
                if (currentMeetingId && currentParticipantId) {
                    state.value.reconnectAttempts++
                    connect(currentMeetingId, currentParticipantId)
                }
            }, delay)
        } else if (state.value.reconnectAttempts >= MAX_RECONNECT_ATTEMPTS) {
            const apiError = createApiError(
                ApiErrorType.WebSocketConnection,
                '接続の再試行回数が上限に達しました。ページを再読み込みしてください。'
            )
            state.value.error = apiError
            console.error('Max reconnection attempts reached')
            
            if (errorCallback) {
                errorCallback(apiError)
            }
        }
    }

    const connect = async (meetingId: string, participantId: string): Promise<void> => {
        if (state.value.isConnecting || state.value.isConnected) {
            return
        }

        try {
            state.value.isConnecting = true
            state.value.error = null

            // Store connection parameters for reconnection
            currentMeetingId = meetingId
            currentParticipantId = participantId

            const url = `${WEBSOCKET_URL}?meetingId=${encodeURIComponent(meetingId)}&participantId=${encodeURIComponent(participantId)}`

            websocket = new WebSocket(url)

            websocket.onopen = handleWebSocketOpen
            websocket.onmessage = handleWebSocketMessage
            websocket.onerror = handleWebSocketError
            websocket.onclose = handleWebSocketClose

            // Wait for connection to be established or fail
            await new Promise<void>((resolve, reject) => {
                const timeout = window.setTimeout(() => {
                    reject(new Error('WebSocket connection timeout'))
                }, 10000) // 10 second timeout

                const originalOnOpen = websocket?.onopen
                const originalOnError = websocket?.onerror

                if (websocket) {
                    websocket.onopen = (event) => {
                        window.clearTimeout(timeout)
                        if (originalOnOpen && websocket) originalOnOpen.call(websocket, event)
                        resolve()
                    }

                    websocket.onerror = (event) => {
                        window.clearTimeout(timeout)
                        if (originalOnError && websocket) originalOnError.call(websocket, event)
                        reject(new Error('WebSocket connection failed'))
                    }
                }
            })

        } catch (error) {
            state.value.isConnecting = false
            const apiError = createApiError(
                ApiErrorType.WebSocketConnection,
                error instanceof Error ? error.message : 'WebSocket接続に失敗しました'
            )
            state.value.error = apiError
            console.error('WebSocket connection error:', error)
            
            if (errorCallback) {
                errorCallback(apiError)
            }
            throw apiError
        }
    }

    const disconnect = () => {
        clearReconnectTimeout()

        if (websocket) {
            // Set a flag to prevent reconnection on clean disconnect
            const ws = websocket
            websocket = null

            // Clean close
            ws.close(1000, 'Client disconnect')
        }

        state.value.isConnected = false
        state.value.isConnecting = false
        state.value.error = null
        state.value.reconnectAttempts = 0
        currentMeetingId = null
        currentParticipantId = null
    }

    const send = (message: any) => {
        if (!websocket || websocket.readyState !== WebSocket.OPEN) {
            throw new Error('WebSocket is not connected')
        }

        try {
            const messageString = typeof message === 'string' ? message : JSON.stringify(message)
            websocket.send(messageString)
        } catch (error) {
            console.error('Failed to send WebSocket message:', error)
            throw error
        }
    }

    const onMessage = (callback: (data: any) => void) => {
        messageCallback = callback
    }

    const onError = (callback: (error: ApiError) => void) => {
        errorCallback = callback
    }

    // Cleanup on component unmount
    onUnmounted(() => {
        disconnect()
    })

    return {
        state,
        connect,
        disconnect,
        send,
        onMessage,
        onError,
        getErrorMessage
    }
}
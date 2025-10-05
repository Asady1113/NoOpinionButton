import { ref, onUnmounted, type Ref } from 'vue'
import { ApiErrorType, type ApiError } from '~/types/error'

// 接続状況・エラー・再接続回数をまとめてリアクティブに管理。
interface WebSocketConnectionState {
    isConnected: boolean
    isConnecting: boolean
    error: ApiError | null
}

interface WebSocketConnection {
    state: Ref<WebSocketConnectionState>
    connect: (meetingId: string, participantId: string) => Promise<void>
    disconnect: () => void
    onMessage: (callback: (data: any) => void) => void
    onError: (callback: (error: ApiError) => void) => void
    getErrorMessage: (error: ApiError) => string
}

export const useWebSocketConnection = (): WebSocketConnection => {
    // リアクティブ変数の初期値を設定
    const state = ref<WebSocketConnectionState>({
        isConnected: false,
        isConnecting: false,
        error: null
    })

    let websocket: WebSocket | null = null
    let messageCallback: ((data: any) => void) | null = null
    let errorCallback: ((error: ApiError) => void) | null = null

    const WEBSOCKET_URL = 'wss://0xjupqx66b.execute-api.ap-northeast-1.amazonaws.com/prod/'

    // エラーオブジェクトを作るヘルパー関数
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

    // WebSocket の onopen イベントで呼ばれる 接続成功時の処理
    const handleWebSocketOpen = () => {
        state.value.isConnected = true
        state.value.isConnecting = false
        state.value.error = null
        console.log('WebSocket connected successfully')
    }

    const handleWebSocketMessage = (event: MessageEvent) => {
        try {
            const data = JSON.parse(event.data)
            if (messageCallback) {
                // onMessageで登録したメソッドを呼ぶ（これはつまりmessageReceptionのhandleWebSockectMessageメソッド）
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
    }

    const connect = async (meetingId: string, participantId: string): Promise<void> => {
        if (state.value.isConnecting || state.value.isConnected) {
            return
        }

        try {
            state.value.isConnecting = true
            state.value.error = null

            const url = `${WEBSOCKET_URL}?meetingId=${encodeURIComponent(meetingId)}&participantId=${encodeURIComponent(participantId)}`

            websocket = new WebSocket(url)

            // WebSocket API に元から備わっているイベントハンドラのプロパティ
            // 接続成功時
            websocket.onopen = handleWebSocketOpen
            // メッセージ受信時
            websocket.onmessage = handleWebSocketMessage
            websocket.onerror = handleWebSocketError
            websocket.onclose = handleWebSocketClose

            // Wait for connection to be established or fail
            await new Promise<void>((resolve, reject) => {
                const timeout = window.setTimeout(() => {
                    reject(new Error('WebSocket connection timeout'))
                }, 10000) // 10 second timeout

                // もともと WebSocket インスタンスにセットされていた onopen と onerror を 別の変数に保存
                const originalOnOpen = websocket?.onopen
                const originalOnError = websocket?.onerror

                if (websocket) {
                    websocket.onopen = (event) => {
                        // 接続成功したらまず タイマーをクリア
                        window.clearTimeout(timeout)
                        // もし元々の onopen があれば呼びだす
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
        if (websocket) {
            const ws = websocket
            websocket = null
            // onclose（＝handleWebSocketClose）が呼ばれる
            ws.close(1000, 'Client disconnect')
        }
    }

    // messageCallback関数を登録するメソッド
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
        onMessage,
        onError,
        getErrorMessage
    }
}
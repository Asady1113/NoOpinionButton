import { ref, type Ref } from 'vue'
import { ApiErrorType, type ApiError } from '~/types/error'

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

interface MessageReceptionState {
  messages: MessageData[]
  isLoading: boolean
  error: ApiError | null
}

interface MessageReception {
  state: Ref<MessageReceptionState>
  addMessage: (messageData: MessageData) => void
  clearMessages: () => void
  handleWebSocketMessage: (rawData: any) => void
  getErrorMessage: (error: ApiError) => string
}

export const useMessageReception = (): MessageReception => {
  const state = ref<MessageReceptionState>({
    messages: [],
    isLoading: false,
    error: null
  })

  const MAX_MESSAGES = 100 // Limit messages to prevent memory issues

  const createApiError = (type: ApiErrorType, message: string, statusCode: number = 0): ApiError => {
    return {
      type,
      message,
      statusCode
    }
  }

  const getErrorMessage = (error: ApiError): string => {
    switch (error.type) {
      case ApiErrorType.WebSocketMessage:
        if (error.message.includes('Invalid message data format')) {
          return '受信したメッセージの形式が正しくありません。'
        } else if (error.message.includes('処理中にエラー')) {
          return 'メッセージの処理中にエラーが発生しました。'
        } else {
          return 'メッセージの受信でエラーが発生しました。'
        }
      default:
        return error.message || '不明なエラーが発生しました。'
    }
  }

  /**
   * Validates if the received data matches the expected server message format
   */
  const validateMessageData = (data: any): boolean => {
    if (!data || typeof data !== 'object') {
      console.error('Message validation failed: data is not an object', data)
      return false
    }

    // Check required string fields
    const stringFields = ['messageId', 'meetingId', 'participantId', 'participantName', 'content', 'createdAt']
    for (const field of stringFields) {
      if (!(field in data)) {
        console.error(`Message validation failed: missing field '${field}'`, data)
        return false
      }
      if (typeof data[field] !== 'string') {
        console.error(`Message validation failed: field '${field}' should be string, got ${typeof data[field]}`, data)
        return false
      }
    }

    // Check required number fields
    const numberFields = ['likeCount', 'reportedCount']
    for (const field of numberFields) {
      if (!(field in data)) {
        console.error(`Message validation failed: missing field '${field}'`, data)
        return false
      }
      if (typeof data[field] !== 'number') {
        console.error(`Message validation failed: field '${field}' should be number, got ${typeof data[field]}`, data)
        return false
      }
    }

    // Check isActive field (can be boolean or number 0/1)
    if (!('isActive' in data)) {
      console.error('Message validation failed: missing field \'isActive\'', data)
      return false
    }

    const isActiveValue = data.isActive
    const isValidBoolean = typeof isActiveValue === 'boolean'
    const isValidNumber = typeof isActiveValue === 'number' && (isActiveValue === 0 || isActiveValue === 1)

    if (!isValidBoolean && !isValidNumber) {
      console.error(`Message validation failed: field 'isActive' should be boolean or number (0/1), got ${typeof isActiveValue} with value ${isActiveValue}`, data)
      return false
    }

    return true
  }

  /**
   * Converts server message format to frontend MessageData format
   */
  const convertToMessageData = (rawData: any): MessageData => {
    if (!validateMessageData(rawData)) {
      console.error('Invalid message data received:', rawData)
      throw new Error('Invalid message data format')
    }

    // Convert isActive to boolean if it's a number
    const isActive = typeof rawData.isActive === 'number'
      ? rawData.isActive === 1
      : rawData.isActive

    return {
      id: rawData.messageId, // Map messageId to id
      meetingId: rawData.meetingId,
      participantId: rawData.participantId,
      participantName: rawData.participantName,
      content: rawData.content,
      createdAt: rawData.createdAt,
      likeCount: rawData.likeCount,
      reportedCount: rawData.reportedCount,
      isActive: isActive
    }
  }

  /**
   * Adds a new message to the message list
   * Maintains maximum message limit by removing oldest messages
   */
  const addMessage = (messageData: MessageData): void => {
    try {
      // Check if message already exists to prevent duplicates
      const existingMessageIndex = state.value.messages.findIndex(
        msg => msg.id === messageData.id
      )

      if (existingMessageIndex !== -1) {
        // Update existing message
        state.value.messages[existingMessageIndex] = messageData
      } else {
        // Add new message
        state.value.messages.push(messageData)

        // Maintain message limit by removing oldest messages
        if (state.value.messages.length > MAX_MESSAGES) {
          state.value.messages = state.value.messages.slice(-MAX_MESSAGES)
        }
      }

      // Clear any previous errors when successfully adding a message
      state.value.error = null
    } catch (error) {
      console.error('Failed to add message:', error)
      const apiError = createApiError(
        ApiErrorType.WebSocketMessage,
        'メッセージの受信に失敗しました'
      )
      state.value.error = apiError
    }
  }

  /**
   * Clears all messages from the list
   */
  const clearMessages = (): void => {
    state.value.messages = []
    state.value.error = null
  }

  /**
   * Handles raw WebSocket message data
   * Parses, validates, and adds the message to the list
   */
  const handleWebSocketMessage = (rawData: any): void => {
    try {
      state.value.error = null

      // Log received message for debugging
      console.log('Received WebSocket message:', rawData)

      // Handle different message types if needed
      // For now, we assume all messages are MessageEntity format
      const messageData = convertToMessageData(rawData)

      // Only add active messages
      if (messageData.isActive) {
        addMessage(messageData)
        console.log('Message added successfully:', messageData)
      } else {
        console.log('Message skipped (not active):', messageData)
      }
    } catch (error) {
      console.error('Failed to handle WebSocket message:', error)
      console.error('Raw data that caused error:', rawData)

      // Create user-friendly error message
      let errorMessage: string
      if (error instanceof Error) {
        if (error.message.includes('Invalid message data format')) {
          errorMessage = '受信したメッセージの形式が正しくありません'
        } else {
          errorMessage = 'メッセージの処理中にエラーが発生しました'
        }
      } else {
        errorMessage = '不明なエラーが発生しました'
      }

      // Create ApiError for consistent error handling
      const apiError = createApiError(
        ApiErrorType.WebSocketMessage,
        errorMessage
      )
      state.value.error = apiError

      // Log the structured error for debugging
      console.error('WebSocket message error:', apiError)
    }
  }

  return {
    state,
    addMessage,
    clearMessages,
    handleWebSocketMessage,
    getErrorMessage
  }
}
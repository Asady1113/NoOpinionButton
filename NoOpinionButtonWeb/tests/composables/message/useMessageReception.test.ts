import { describe, it, expect, beforeEach } from 'vitest'
import { useMessageReception } from '~/composables/message/useMessageReception'
import { ApiErrorType } from '~/types/error'

describe('useMessageReception', () => {
  let messageReception: ReturnType<typeof useMessageReception>

  beforeEach(() => {
    messageReception = useMessageReception()
  })

  describe('初期状態', () => {
    it('should initialize with empty state', () => {
      expect(messageReception.state.value.messages).toEqual([])
      expect(messageReception.state.value.isLoading).toBe(false)
      expect(messageReception.state.value.error).toBeNull()
    })
  })

  describe('addMessage', () => {
    const validMessageData = {
      id: 'test-message-1',
      meetingId: 'test-meeting',
      participantId: 'test-participant',
      participantName: 'テスト参加者',
      content: 'テストメッセージ',
      createdAt: '2024-01-01T00:00:00Z',
      likeCount: 0,
      reportedCount: 0,
      isActive: true
    }

    it('should add a new message to the list', () => {
      messageReception.addMessage(validMessageData)

      expect(messageReception.state.value.messages).toHaveLength(1)
      expect(messageReception.state.value.messages[0]).toEqual(validMessageData)
      expect(messageReception.state.value.error).toBeNull()
    })

    it('should update existing message if same ID', () => {
      messageReception.addMessage(validMessageData)
      
      const updatedMessage = {
        ...validMessageData,
        content: '更新されたメッセージ',
        likeCount: 5
      }
      
      messageReception.addMessage(updatedMessage)

      expect(messageReception.state.value.messages).toHaveLength(1)
      expect(messageReception.state.value.messages[0].content).toBe('更新されたメッセージ')
      expect(messageReception.state.value.messages[0].likeCount).toBe(5)
    })

    it('should maintain maximum message limit', () => {
      // Add 101 messages to test the limit
      for (let i = 0; i < 101; i++) {
        messageReception.addMessage({
          ...validMessageData,
          id: `test-message-${i}`,
          content: `メッセージ ${i}`
        })
      }

      expect(messageReception.state.value.messages).toHaveLength(100)
      // Should keep the latest 100 messages
      expect(messageReception.state.value.messages[0].id).toBe('test-message-1')
      expect(messageReception.state.value.messages[99].id).toBe('test-message-100')
    })
  })

  describe('clearMessages', () => {
    it('should clear all messages and errors', () => {
      const validMessageData = {
        id: 'test-message-1',
        meetingId: 'test-meeting',
        participantId: 'test-participant',
        participantName: 'テスト参加者',
        content: 'テストメッセージ',
        createdAt: '2024-01-01T00:00:00Z',
        likeCount: 0,
        reportedCount: 0,
        isActive: true
      }

      messageReception.addMessage(validMessageData)
      messageReception.state.value.error = 'テストエラー'

      messageReception.clearMessages()

      expect(messageReception.state.value.messages).toEqual([])
      expect(messageReception.state.value.error).toBeNull()
    })
  })

  describe('handleWebSocketMessage', () => {
    const validWebSocketData = {
      id: 'ws-message-1',
      meetingId: 'test-meeting',
      participantId: 'test-participant',
      participantName: 'WebSocket参加者',
      content: 'WebSocketメッセージ',
      createdAt: '2024-01-01T00:00:00Z',
      likeCount: 0,
      reportedCount: 0,
      isActive: true
    }

    it('should parse and add valid WebSocket message', () => {
      messageReception.handleWebSocketMessage(validWebSocketData)

      expect(messageReception.state.value.messages).toHaveLength(1)
      expect(messageReception.state.value.messages[0]).toEqual(validWebSocketData)
      expect(messageReception.state.value.error).toBeNull()
    })

    it('should ignore inactive messages', () => {
      const inactiveMessage = {
        ...validWebSocketData,
        isActive: false
      }

      messageReception.handleWebSocketMessage(inactiveMessage)

      expect(messageReception.state.value.messages).toHaveLength(0)
      expect(messageReception.state.value.error).toBeNull()
    })

    it('should handle invalid message format', () => {
      const invalidMessage = {
        id: 'invalid-message',
        // Missing required fields
        content: 'Invalid message'
      }

      messageReception.handleWebSocketMessage(invalidMessage)

      expect(messageReception.state.value.messages).toHaveLength(0)
      expect(messageReception.state.value.error).toBe('受信したメッセージの形式が正しくありません')
    })

    it('should handle null or undefined data', () => {
      messageReception.handleWebSocketMessage(null)

      expect(messageReception.state.value.messages).toHaveLength(0)
      expect(messageReception.state.value.error).toBe('受信したメッセージの形式が正しくありません')
    })

    it('should handle missing required string fields', () => {
      const invalidMessage = {
        id: 123, // Should be string
        meetingId: 'test-meeting',
        participantId: 'test-participant',
        participantName: 'テスト参加者',
        content: 'テストメッセージ',
        createdAt: '2024-01-01T00:00:00Z',
        likeCount: 0,
        reportedCount: 0,
        isActive: true
      }

      messageReception.handleWebSocketMessage(invalidMessage)

      expect(messageReception.state.value.messages).toHaveLength(0)
      expect(messageReception.state.value.error).toBe('受信したメッセージの形式が正しくありません')
    })

    it('should handle missing required number fields', () => {
      const invalidMessage = {
        id: 'test-message',
        meetingId: 'test-meeting',
        participantId: 'test-participant',
        participantName: 'テスト参加者',
        content: 'テストメッセージ',
        createdAt: '2024-01-01T00:00:00Z',
        likeCount: 'invalid', // Should be number
        reportedCount: 0,
        isActive: true
      }

      messageReception.handleWebSocketMessage(invalidMessage)

      expect(messageReception.state.value.messages).toHaveLength(0)
      expect(messageReception.state.value.error).toBe('受信したメッセージの形式が正しくありません')
    })

    it('should clear previous errors when processing valid message', () => {
      // First, create an error
      messageReception.handleWebSocketMessage(null)
      expect(messageReception.state.value.error).toBeTruthy()

      // Then process a valid message
      messageReception.handleWebSocketMessage(validWebSocketData)

      expect(messageReception.state.value.messages).toHaveLength(1)
      expect(messageReception.state.value.error).toBeNull()
    })
  })

  describe('メッセージデータ変換', () => {
    it('should convert MessageEntity to MessageData format correctly', () => {
      const messageEntity = {
        id: 'entity-1',
        meetingId: 'meeting-1',
        participantId: 'participant-1',
        participantName: '変換テスト参加者',
        content: '変換テストメッセージ',
        createdAt: '2024-01-01T12:00:00Z',
        likeCount: 3,
        reportedCount: 1,
        isActive: true
      }

      messageReception.handleWebSocketMessage(messageEntity)

      const convertedMessage = messageReception.state.value.messages[0]
      expect(convertedMessage.id).toBe(messageEntity.id)
      expect(convertedMessage.meetingId).toBe(messageEntity.meetingId)
      expect(convertedMessage.participantId).toBe(messageEntity.participantId)
      expect(convertedMessage.participantName).toBe(messageEntity.participantName)
      expect(convertedMessage.content).toBe(messageEntity.content)
      expect(convertedMessage.createdAt).toBe(messageEntity.createdAt)
      expect(convertedMessage.likeCount).toBe(messageEntity.likeCount)
      expect(convertedMessage.reportedCount).toBe(messageEntity.reportedCount)
      expect(convertedMessage.isActive).toBe(messageEntity.isActive)
    })
  })
})
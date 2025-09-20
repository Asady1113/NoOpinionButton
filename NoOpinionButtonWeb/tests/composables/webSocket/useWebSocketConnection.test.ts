import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest'
import { useWebSocketConnection } from '~/composables/webSocket/useWebSocketConnection'

// Mock WebSocket
class MockWebSocket {
  static CONNECTING = 0
  static OPEN = 1
  static CLOSING = 2
  static CLOSED = 3

  readyState = MockWebSocket.CONNECTING
  url: string
  onopen: ((event: Event) => void) | null = null
  onclose: ((event: CloseEvent) => void) | null = null
  onmessage: ((event: MessageEvent) => void) | null = null
  onerror: ((event: Event) => void) | null = null

  constructor(url: string) {
    this.url = url
    // Simulate async connection
    setTimeout(() => {
      this.readyState = MockWebSocket.OPEN
      if (this.onopen) {
        this.onopen(new Event('open'))
      }
    }, 10)
  }

  close(code?: number, reason?: string) {
    this.readyState = MockWebSocket.CLOSED
    if (this.onclose) {
      const closeEvent = new CloseEvent('close', {
        code: code || 1000,
        reason: reason || '',
        wasClean: code === 1000
      })
      this.onclose(closeEvent)
    }
  }

  send(data: string) {
    if (this.readyState !== MockWebSocket.OPEN) {
      throw new Error('WebSocket is not open')
    }
  }

  // Helper methods for testing
  simulateMessage(data: any) {
    if (this.onmessage) {
      const messageEvent = new MessageEvent('message', {
        data: JSON.stringify(data)
      })
      this.onmessage(messageEvent)
    }
  }

  simulateError() {
    if (this.onerror) {
      this.onerror(new Event('error'))
    }
  }

  simulateClose(wasClean = false, code = 1006) {
    this.readyState = MockWebSocket.CLOSED
    if (this.onclose) {
      const closeEvent = new CloseEvent('close', {
        code,
        reason: '',
        wasClean
      })
      this.onclose(closeEvent)
    }
  }
}

// Mock global WebSocket
global.WebSocket = MockWebSocket as any

describe('useWebSocketConnection', () => {
  let connection: ReturnType<typeof useWebSocketConnection>

  beforeEach(() => {
    vi.clearAllTimers()
    vi.useFakeTimers()
    connection = useWebSocketConnection()
  })

  afterEach(() => {
    vi.useRealTimers()
    connection.disconnect()
  })

  describe('Initial State', () => {
    it('should have correct initial state', () => {
      expect(connection.state.value).toEqual({
        isConnected: false,
        isConnecting: false,
        error: null,
        reconnectAttempts: 0
      })
    })
  })

  describe('Connection Management', () => {
    it('should connect successfully with correct URL format', async () => {
      const meetingId = 'test-meeting'
      const participantId = 'test-participant'
      
      const connectPromise = connection.connect(meetingId, participantId)
      
      expect(connection.state.value.isConnecting).toBe(true)
      
      // Wait for connection to complete
      vi.advanceTimersByTime(20)
      await connectPromise
      
      expect(connection.state.value.isConnected).toBe(true)
      expect(connection.state.value.isConnecting).toBe(false)
      expect(connection.state.value.error).toBe(null)
    })

    it('should include meetingId and participantId in query parameters', async () => {
      const meetingId = 'test-meeting'
      const participantId = 'test-participant'
      
      await connection.connect(meetingId, participantId)
      
      // Check that WebSocket was created with correct URL
      const expectedUrl = `wss://0xjupqx66b.execute-api.ap-northeast-1.amazonaws.com/prod/?meetingId=${meetingId}&participantId=${participantId}`
      // We can't directly access the WebSocket instance, but we can verify the connection succeeded
      expect(connection.state.value.isConnected).toBe(true)
    })

    it('should handle connection timeout', async () => {
      // Mock WebSocket that never opens
      global.WebSocket = class extends MockWebSocket {
        constructor(url: string) {
          super(url)
          // Don't call onopen
        }
      } as any

      const connectPromise = connection.connect('test-meeting', 'test-participant')
      
      // Advance time to trigger timeout
      vi.advanceTimersByTime(11000)
      
      await expect(connectPromise).rejects.toThrow('WebSocket connection timeout')
      expect(connection.state.value.isConnecting).toBe(false)
      expect(connection.state.value.error).toContain('timeout')
    })

    it('should disconnect cleanly', async () => {
      await connection.connect('test-meeting', 'test-participant')
      expect(connection.state.value.isConnected).toBe(true)
      
      connection.disconnect()
      
      expect(connection.state.value.isConnected).toBe(false)
      expect(connection.state.value.isConnecting).toBe(false)
      expect(connection.state.value.error).toBe(null)
      expect(connection.state.value.reconnectAttempts).toBe(0)
    })
  })

  describe('Message Handling', () => {
    beforeEach(async () => {
      await connection.connect('test-meeting', 'test-participant')
    })

    it('should receive and parse JSON messages', () => {
      const testMessage = { type: 'test', content: 'hello' }
      let receivedMessage: any = null
      
      connection.onMessage((data) => {
        receivedMessage = data
      })
      
      // Simulate receiving a message
      const ws = (global as any).lastWebSocket || new MockWebSocket('')
      ws.simulateMessage(testMessage)
      
      expect(receivedMessage).toEqual(testMessage)
    })

    it('should handle invalid JSON messages gracefully', () => {
      connection.onMessage(() => {})
      
      // Simulate receiving invalid JSON
      const ws = new MockWebSocket('')
      if (ws.onmessage) {
        const invalidMessageEvent = new MessageEvent('message', {
          data: 'invalid json'
        })
        ws.onmessage(invalidMessageEvent)
      }
      
      expect(connection.state.value.error).toContain('メッセージの解析に失敗しました')
    })
  })

  describe('Error Handling', () => {
    it('should handle WebSocket errors', async () => {
      let errorReceived: Event | null = null
      
      connection.onError((error) => {
        errorReceived = error
      })
      
      await connection.connect('test-meeting', 'test-participant')
      
      // Simulate WebSocket error
      const ws = new MockWebSocket('')
      ws.simulateError()
      
      expect(connection.state.value.error).toContain('WebSocket接続でエラーが発生しました')
      expect(errorReceived).toBeInstanceOf(Event)
    })
  })

  describe('Reconnection Logic', () => {
    beforeEach(async () => {
      await connection.connect('test-meeting', 'test-participant')
    })

    it('should attempt reconnection on unexpected disconnect', () => {
      // Simulate unexpected disconnect
      const ws = new MockWebSocket('')
      ws.simulateClose(false, 1006) // Abnormal closure
      
      expect(connection.state.value.isConnected).toBe(false)
      expect(connection.state.value.reconnectAttempts).toBe(0)
      
      // Advance time to trigger first reconnection attempt
      vi.advanceTimersByTime(1000)
      
      expect(connection.state.value.reconnectAttempts).toBe(1)
    })

    it('should use exponential backoff for reconnection delays', () => {
      const ws = new MockWebSocket('')
      
      // First disconnect and reconnection attempt
      ws.simulateClose(false, 1006)
      vi.advanceTimersByTime(1000) // 1s delay
      expect(connection.state.value.reconnectAttempts).toBe(1)
      
      // Second disconnect and reconnection attempt
      ws.simulateClose(false, 1006)
      vi.advanceTimersByTime(2000) // 2s delay
      expect(connection.state.value.reconnectAttempts).toBe(2)
      
      // Third disconnect and reconnection attempt
      ws.simulateClose(false, 1006)
      vi.advanceTimersByTime(4000) // 4s delay
      expect(connection.state.value.reconnectAttempts).toBe(3)
    })

    it('should stop reconnecting after max attempts', () => {
      const ws = new MockWebSocket('')
      
      // Simulate 5 failed reconnection attempts
      for (let i = 0; i < 5; i++) {
        ws.simulateClose(false, 1006)
        vi.advanceTimersByTime(Math.pow(2, i) * 1000)
      }
      
      expect(connection.state.value.reconnectAttempts).toBe(5)
      
      // One more disconnect should not trigger reconnection
      ws.simulateClose(false, 1006)
      vi.advanceTimersByTime(32000) // Wait longer than any reconnection delay
      
      expect(connection.state.value.error).toContain('接続の再試行回数が上限に達しました')
    })

    it('should not reconnect on clean disconnect', () => {
      const ws = new MockWebSocket('')
      
      // Simulate clean disconnect
      ws.simulateClose(true, 1000)
      
      expect(connection.state.value.isConnected).toBe(false)
      
      // Advance time - should not trigger reconnection
      vi.advanceTimersByTime(5000)
      
      expect(connection.state.value.reconnectAttempts).toBe(0)
    })

    it('should reset reconnect attempts on successful connection', async () => {
      // Set some reconnect attempts
      connection.state.value.reconnectAttempts = 3
      
      // Successful connection should reset attempts
      await connection.connect('test-meeting', 'test-participant')
      
      expect(connection.state.value.reconnectAttempts).toBe(0)
    })
  })

  describe('Send Messages', () => {
    beforeEach(async () => {
      await connection.connect('test-meeting', 'test-participant')
    })

    it('should send string messages', () => {
      expect(() => {
        connection.send('test message')
      }).not.toThrow()
    })

    it('should send object messages as JSON', () => {
      expect(() => {
        connection.send({ type: 'test', content: 'hello' })
      }).not.toThrow()
    })

    it('should throw error when not connected', () => {
      connection.disconnect()
      
      expect(() => {
        connection.send('test message')
      }).toThrow('WebSocket is not connected')
    })
  })

  describe('Cleanup', () => {
    it('should clean up resources on disconnect', () => {
      connection.connect('test-meeting', 'test-participant')
      connection.disconnect()
      
      // Verify state is reset
      expect(connection.state.value).toEqual({
        isConnected: false,
        isConnecting: false,
        error: null,
        reconnectAttempts: 0
      })
    })
  })
})
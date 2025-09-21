import { describe, it, expect, vi, beforeEach } from 'vitest'
import { ref, readonly } from 'vue'
import { useMessageSending } from '~/composables/message/useMessageSending'

// Mock the useMessageSendingApi composable
vi.mock('~/composables/message/useMessageSendingApi', () => ({
  useMessageSendingApi: vi.fn()
}))

describe('useMessageSending', () => {
  let mockSendMessage: ReturnType<typeof vi.fn>
  let mockIsLoading: { value: boolean }
  let mockError: { value: any }

  beforeEach(async () => {
    mockSendMessage = vi.fn()
    mockIsLoading = { value: false }
    mockError = { value: null }

    const { useMessageSendingApi } = await import('~/composables/message/useMessageSendingApi')
    vi.mocked(useMessageSendingApi).mockReturnValue({
      sendMessage: mockSendMessage,
      isLoading: mockIsLoading as any,
      error: mockError as any
    })
  })

  describe('初期状態', () => {
    it('初期状態が正しく設定される', () => {
      const { state } = useMessageSending()

      expect(state.value.content).toBe('')
      expect(state.value.isSubmitting).toBe(false)
      expect(state.value.error).toBe(null)
      expect(state.value.canSubmit).toBe(false)
    })
  })

  describe('updateContent', () => {
    it('コンテンツが正しく更新される', () => {
      const { state, updateContent } = useMessageSending()

      updateContent('テストメッセージ')

      expect(state.value.content).toBe('テストメッセージ')
      expect(state.value.canSubmit).toBe(true)
    })

    it('コンテンツ更新時にエラーがクリアされる', () => {
      const { state, updateContent, clearError } = useMessageSending()

      // エラーを設定
      clearError()
      updateContent('') // エラー状態にする

      // コンテンツを更新
      updateContent('新しいメッセージ')

      expect(state.value.error).toBe(null)
    })
  })

  describe('canSubmit computed property', () => {
    it('空文字の場合は送信不可', () => {
      const { state, updateContent } = useMessageSending()

      updateContent('')
      expect(state.value.canSubmit).toBe(false)

      updateContent('   ') // 空白のみ
      expect(state.value.canSubmit).toBe(false)
    })

    it('テキストがある場合は送信可能', () => {
      const { state, updateContent } = useMessageSending()

      updateContent('テストメッセージ')
      expect(state.value.canSubmit).toBe(true)
    })

    it('送信中は送信不可', () => {
      const { state, updateContent } = useMessageSending()

      updateContent('テストメッセージ')
      mockIsLoading.value = true

      expect(state.value.canSubmit).toBe(false)
    })
  })

  describe('submitMessage', () => {
    it('正常にメッセージが送信される', async () => {
      const { state, updateContent, submitMessage } = useMessageSending()

      updateContent('テストメッセージ')
      mockSendMessage.mockResolvedValue({ messageId: '123' })

      await submitMessage('meeting123', 'participant456')

      expect(mockSendMessage).toHaveBeenCalledWith({
        meetingId: 'meeting123',
        participantId: 'participant456',
        content: 'テストメッセージ'
      })

      // フォームがクリアされる
      expect(state.value.content).toBe('')
      expect(state.value.error).toBe(null)
    })

    it('空文字の場合はバリデーションエラー', async () => {
      const { state, updateContent, submitMessage } = useMessageSending()

      updateContent('')

      await submitMessage('meeting123', 'participant456')

      expect(mockSendMessage).not.toHaveBeenCalled()
      expect(state.value.error).toEqual({
        type: 'BadRequest',
        message: 'メッセージを入力してください',
        statusCode: 400
      })
    })

    it('空白のみの場合はバリデーションエラー', async () => {
      const { state, updateContent, submitMessage } = useMessageSending()

      updateContent('   ')

      await submitMessage('meeting123', 'participant456')

      expect(mockSendMessage).not.toHaveBeenCalled()
      expect(state.value.error).toEqual({
        type: 'BadRequest',
        message: 'メッセージを入力してください',
        statusCode: 400
      })
    })

    it('API エラーが発生した場合', async () => {
      const { state, updateContent, submitMessage } = useMessageSending()

      updateContent('テストメッセージ')
      const apiError = {
        type: 'Server' as const,
        message: 'サーバーエラーが発生しました',
        statusCode: 500
      }
      mockError.value = apiError // Set the API error in the mock
      mockSendMessage.mockRejectedValue(apiError)

      await expect(submitMessage('meeting123', 'participant456')).rejects.toThrow()

      expect(state.value.error).toEqual(apiError)
      expect(state.value.content).toBe('テストメッセージ') // エラー時はフォームクリアしない
    })

    it('不明なエラーが発生した場合', async () => {
      const { state, updateContent, submitMessage } = useMessageSending()

      updateContent('テストメッセージ')
      const unknownError = new Error('Unknown error')
      mockSendMessage.mockRejectedValue(unknownError)

      await expect(submitMessage('meeting123', 'participant456')).rejects.toThrow()

      // For unknown errors, the API error should be null since it's not an ApiError
      expect(state.value.error).toBe(null)
    })
  })

  describe('clearForm', () => {
    it('フォームが正しくクリアされる', () => {
      const { state, updateContent, clearForm } = useMessageSending()

      updateContent('テストメッセージ')
      clearForm()

      expect(state.value.content).toBe('')
      expect(state.value.error).toBe(null)
    })
  })

  describe('clearError', () => {
    it('エラーが正しくクリアされる', async () => {
      const { state, updateContent, submitMessage, clearError } = useMessageSending()

      // エラー状態にする
      updateContent('')
      await submitMessage('meeting123', 'participant456')
      expect(state.value.error).toEqual({
        type: 'BadRequest',
        message: 'メッセージを入力してください',
        statusCode: 400
      })

      // エラーをクリア
      clearError()
      expect(state.value.error).toBe(null)
    })
  })
})
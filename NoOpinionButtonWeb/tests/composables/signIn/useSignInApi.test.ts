import { describe, test, expect, beforeEach, vi } from 'vitest'
import { ApiErrorType } from '~/types/error'
import type { ApiError } from '~/types/error'

// Mock useRuntimeConfig
const mockConfig = {
  public: {
    apiBaseUrl: 'https://api.example.com'
  }
}

// Mock #app module before importing
vi.mock('#app', () => ({
  useRuntimeConfig: vi.fn(() => mockConfig)
}))

// Import after mocking
const { useSignInApi } = await import('~/composables/signIn/useSignInApi')

beforeEach(() => {
  vi.clearAllMocks()
  global.fetch = vi.fn()
})

describe('useSignInApi', () => {
  test('正常系_signIn_有効なパラメータで正常レスポンスとfetch呼び出し', async () => {
    // Arrange
    const mockResponseData = {
      Data: {
        id: 'participant123',
        meetingId: 'meeting456',
        meetingName: 'テスト会議',
        isFacilitator: false
      }
    }

    // fetchをモック化
    global.fetch = vi.fn().mockResolvedValue({
      ok: true,
      json: vi.fn().mockResolvedValue(mockResponseData)
    })

    const { signIn } = useSignInApi()

    // Act
    const result = await signIn('meeting456', 'password123')

    // Assert
    // レスポンス値の検証
    expect(result).toEqual(mockResponseData)
    
    // fetch呼び出しパラメータの検証
    expect(global.fetch).toHaveBeenCalledTimes(1)
    expect(global.fetch).toHaveBeenCalledWith(
      'https://api.example.com/signin',
      {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ meetingId: 'meeting456', password: 'password123' })
      }
    )
  })

  test('異常系_signIn_400BadRequest_適切なエラー生成', async () => {
    // Arrange
    const errorResponse = { Error: 'Invalid request parameters' }
    global.fetch = vi.fn().mockResolvedValue({
      ok: false,
      status: 400,
      json: vi.fn().mockResolvedValue(errorResponse)
    })

    const { signIn } = useSignInApi()

    // Act & Assert
    // エラーが投げられることを確認
    await expect(signIn('meeting456', 'invalid')).rejects.toThrow()
    
    try {
      await signIn('meeting456', 'invalid')
    } catch (error) {
      // エラーの中身を確認
      const apiError = error as ApiError
      expect(apiError.type).toBe(ApiErrorType.BadRequest)
      expect(apiError.message).toBe('Invalid request parameters')
      expect(apiError.statusCode).toBe(400)
    }
  })

  test('異常系_signIn_401Unauthorized_適切なエラー生成', async () => {
    // Arrange
    const errorResponse = { Error: 'Invalid credentials' }
    global.fetch = vi.fn().mockResolvedValue({
      ok: false,
      status: 401,
      json: vi.fn().mockResolvedValue(errorResponse)
    })

    const { signIn } = useSignInApi()

    // Act & Assert
    try {
      await signIn('meeting456', 'wrongpassword')
    } catch (error) {
      const apiError = error as ApiError
      expect(apiError.type).toBe(ApiErrorType.Unauthorized)
      expect(apiError.message).toBe('Invalid credentials')
      expect(apiError.statusCode).toBe(401)
    }
  })

  test('異常系_signIn_404NotFound_適切なエラー生成', async () => {
    // Arrange
    const errorResponse = { Error: 'Meeting not found' }
    global.fetch = vi.fn().mockResolvedValue({
      ok: false,
      status: 404,
      json: vi.fn().mockResolvedValue(errorResponse)
    })

    const { signIn } = useSignInApi()

    // Act & Assert
    try {
      await signIn('nonexistent', 'password123')
    } catch (error) {
      const apiError = error as ApiError
      expect(apiError.type).toBe(ApiErrorType.NotFound)
      expect(apiError.message).toBe('Meeting not found')
      expect(apiError.statusCode).toBe(404)
    }
  })

  test('異常系_signIn_500ServerError_適切なエラー生成', async () => {
    // Arrange
    const errorResponse = { Error: 'Internal server error' }
    global.fetch = vi.fn().mockResolvedValue({
      ok: false,
      status: 500,
      json: vi.fn().mockResolvedValue(errorResponse)
    })

    const { signIn } = useSignInApi()

    // Act & Assert
    try {
      await signIn('meeting456', 'password123')
    } catch (error) {
      const apiError = error as ApiError
      expect(apiError.type).toBe(ApiErrorType.Server)
      expect(apiError.message).toBe('Internal server error')
      expect(apiError.statusCode).toBe(500)
    }
  })

  test('異常系_signIn_レスポンスボディ解析失敗時のフォールバック', async () => {
    // Arrange - Error フィールドがない、または空のレスポンス
    global.fetch = vi.fn().mockResolvedValue({
      ok: false,
      status: 403,
      json: vi.fn().mockResolvedValue({}) // Error フィールドなし
    })

    const { signIn } = useSignInApi()

    // Act & Assert
    try {
      await signIn('meeting456', 'password123')
    } catch (error) {
      const apiError = error as ApiError
      expect(apiError.type).toBe(ApiErrorType.Server) // 403 is mapped to Server
      expect(apiError.message).toBe('Request failed with status 403')
      expect(apiError.statusCode).toBe(403)
    }
  })
})
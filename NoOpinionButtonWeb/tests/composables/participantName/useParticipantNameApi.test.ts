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
const { useParticipantNameApi } = await import('~/composables/participantName/useParticipantNameApi')

beforeEach(() => {
  vi.clearAllMocks()
  global.fetch = vi.fn()
})

describe('useParticipantNameApi', () => {
  test('正常系_updateParticipantName_有効なパラメータで正常レスポンスとfetch呼び出し', async () => {
    // Arrange
    const mockResponseData = {
      Data: {
        updatedName: 'テスト太郎'
      },
      Error: null
    }

    // fetchをモック化
    global.fetch = vi.fn().mockResolvedValue({
      ok: true,
      json: vi.fn().mockResolvedValue(mockResponseData)
    })

    const { updateParticipantName } = useParticipantNameApi()

    // Act
    const result = await updateParticipantName('participant123', 'テスト太郎')

    // Assert
    // レスポンス値の検証
    expect(result).toEqual(mockResponseData)
    
    // fetch呼び出しパラメータの検証
    expect(global.fetch).toHaveBeenCalledTimes(1)
    
    // fetch の呼び出し引数を取得
    const fetchCall = (global.fetch as any).mock.calls[0]
    expect(fetchCall[0]).toBe('https://api.example.com/participants/participant123/name')
    
    const fetchOptions = fetchCall[1]
    expect(fetchOptions.method).toBe('PUT')
    expect(fetchOptions.headers).toEqual({
      'Content-Type': 'application/json'
    })
    expect(fetchOptions.body).toBe(JSON.stringify({ name: 'テスト太郎' }))
    expect(fetchOptions.signal).toBeInstanceOf(AbortSignal)
  })

  test('正常系_updateParticipantName_絵文字を含む名前で正常処理', async () => {
    // Arrange
    const mockResponseData = {
      Data: {
        updatedName: 'テスト太郎😊'
      },
      Error: null
    }

    global.fetch = vi.fn().mockResolvedValue({
      ok: true,
      json: vi.fn().mockResolvedValue(mockResponseData)
    })

    const { updateParticipantName } = useParticipantNameApi()

    // Act
    const result = await updateParticipantName('participant123', 'テスト太郎😊')

    // Assert
    expect(result).toEqual(mockResponseData)
    
    // fetch の呼び出し引数を検証
    const fetchCall = (global.fetch as any).mock.calls[0]
    expect(fetchCall[0]).toBe('https://api.example.com/participants/participant123/name')
    
    const fetchOptions = fetchCall[1]
    expect(fetchOptions.method).toBe('PUT')
    expect(fetchOptions.headers).toEqual({
      'Content-Type': 'application/json'
    })
    expect(fetchOptions.body).toBe(JSON.stringify({ name: 'テスト太郎😊' }))
    expect(fetchOptions.signal).toBeInstanceOf(AbortSignal)
  })

  test('異常系_updateParticipantName_400BadRequest_適切なエラー生成', async () => {
    // Arrange
    const errorResponse = { Error: '入力内容に問題があります' }
    global.fetch = vi.fn().mockResolvedValue({
      ok: false,
      status: 400,
      json: vi.fn().mockResolvedValue(errorResponse)
    })

    const { updateParticipantName } = useParticipantNameApi()

    // Act & Assert
    // エラーが投げられることを確認
    await expect(updateParticipantName('participant123', '')).rejects.toThrow()
    
    try {
      await updateParticipantName('participant123', '')
    } catch (error) {
      // エラーの中身を確認
      const apiError = error as ApiError
      expect(apiError.type).toBe(ApiErrorType.BadRequest)
      expect(apiError.message).toBe('入力内容に問題があります')
      expect(apiError.statusCode).toBe(400)
    }
  })

  test('異常系_updateParticipantName_401Unauthorized_適切なエラー生成', async () => {
    // Arrange
    const errorResponse = { Error: '認証に失敗しました' }
    global.fetch = vi.fn().mockResolvedValue({
      ok: false,
      status: 401,
      json: vi.fn().mockResolvedValue(errorResponse)
    })

    const { updateParticipantName } = useParticipantNameApi()

    // Act & Assert
    try {
      await updateParticipantName('participant123', 'テスト太郎')
    } catch (error) {
      const apiError = error as ApiError
      expect(apiError.type).toBe(ApiErrorType.Unauthorized)
      expect(apiError.message).toBe('認証に失敗しました')
      expect(apiError.statusCode).toBe(401)
    }
  })

  test('異常系_updateParticipantName_404NotFound_適切なエラー生成', async () => {
    // Arrange
    const errorResponse = { Error: '参加者が見つかりません' }
    global.fetch = vi.fn().mockResolvedValue({
      ok: false,
      status: 404,
      json: vi.fn().mockResolvedValue(errorResponse)
    })

    const { updateParticipantName } = useParticipantNameApi()

    // Act & Assert
    try {
      await updateParticipantName('nonexistent', 'テスト太郎')
    } catch (error) {
      const apiError = error as ApiError
      expect(apiError.type).toBe(ApiErrorType.NotFound)
      expect(apiError.message).toBe('参加者が見つかりません')
      expect(apiError.statusCode).toBe(404)
    }
  })

  test('異常系_updateParticipantName_500ServerError_適切なエラー生成', async () => {
    // Arrange
    const errorResponse = { Error: 'サーバーエラーが発生しました' }
    global.fetch = vi.fn().mockResolvedValue({
      ok: false,
      status: 500,
      json: vi.fn().mockResolvedValue(errorResponse)
    })

    const { updateParticipantName } = useParticipantNameApi()

    // Act & Assert
    try {
      await updateParticipantName('participant123', 'テスト太郎')
    } catch (error) {
      const apiError = error as ApiError
      expect(apiError.type).toBe(ApiErrorType.Server)
      expect(apiError.message).toBe('サーバーエラーが発生しました')
      expect(apiError.statusCode).toBe(500)
    }
  })

  test('異常系_updateParticipantName_レスポンスボディ解析失敗時のフォールバック', async () => {
    // Arrange - Error フィールドがない、または空のレスポンス
    global.fetch = vi.fn().mockResolvedValue({
      ok: false,
      status: 403,
      json: vi.fn().mockResolvedValue({}) // Error フィールドなし
    })

    const { updateParticipantName } = useParticipantNameApi()

    // Act & Assert
    try {
      await updateParticipantName('participant123', 'テスト太郎')
    } catch (error) {
      const apiError = error as ApiError
      expect(apiError.type).toBe(ApiErrorType.Server) // 403 is mapped to Server
      expect(apiError.message).toBe('Request failed with status 403')
      expect(apiError.statusCode).toBe(403)
    }
  })



  test('異常系_updateParticipantName_タイムアウトエラー_適切なエラー生成', async () => {
    // Arrange - AbortError をスローしてタイムアウトをシミュレート
    const abortError = new DOMException('The operation was aborted', 'AbortError')
    global.fetch = vi.fn().mockRejectedValue(abortError)

    const { updateParticipantName } = useParticipantNameApi()

    // Act & Assert
    try {
      await updateParticipantName('participant123', 'テスト太郎')
    } catch (error) {
      const apiError = error as ApiError
      expect(apiError.type).toBe(ApiErrorType.Server)
      expect(apiError.message).toBe('リクエストがタイムアウトしました。しばらく時間をおいて再度お試しください。')
      expect(apiError.statusCode).toBe(0)
    }
  })

  test('異常系_updateParticipantName_ネットワークエラー_適切なエラー生成', async () => {
    // Arrange - TypeError をスローしてネットワークエラーをシミュレート
    global.fetch = vi.fn().mockRejectedValue(new TypeError('Failed to fetch'))

    const { updateParticipantName } = useParticipantNameApi()

    // Act & Assert
    try {
      await updateParticipantName('participant123', 'テスト太郎')
    } catch (error) {
      const apiError = error as ApiError
      expect(apiError.type).toBe(ApiErrorType.Server)
      expect(apiError.message).toBe('ネットワークエラーが発生しました。インターネット接続を確認してください。')
      expect(apiError.statusCode).toBe(0)
    }
  })

  test('正常系_updateParticipantName_長い名前で正常処理', async () => {
    // Arrange - 50文字の名前
    const longName = 'あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやゆよらりるれろわをん'
    const mockResponseData = {
      Data: {
        updatedName: longName
      },
      Error: null
    }

    global.fetch = vi.fn().mockResolvedValue({
      ok: true,
      json: vi.fn().mockResolvedValue(mockResponseData)
    })

    const { updateParticipantName } = useParticipantNameApi()

    // Act
    const result = await updateParticipantName('participant123', longName)

    // Assert
    expect(result).toEqual(mockResponseData)
    
    // fetch の呼び出し引数を検証
    const fetchCall = (global.fetch as any).mock.calls[0]
    expect(fetchCall[0]).toBe('https://api.example.com/participants/participant123/name')
    
    const fetchOptions = fetchCall[1]
    expect(fetchOptions.method).toBe('PUT')
    expect(fetchOptions.headers).toEqual({
      'Content-Type': 'application/json'
    })
    expect(fetchOptions.body).toBe(JSON.stringify({ name: longName }))
    expect(fetchOptions.signal).toBeInstanceOf(AbortSignal)
  })
})
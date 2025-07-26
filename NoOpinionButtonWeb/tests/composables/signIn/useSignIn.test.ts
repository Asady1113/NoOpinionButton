import { describe, test, expect, beforeEach, vi } from 'vitest'

// Mock useState
const mockState = {
  id: '',
  meetingId: '',
  meetingName: ''
}

const mockUseState = vi.fn(() => mockState)

// Mock #app module
vi.mock('#app', () => ({
  useState: mockUseState
}))

// Import after mocking
const { useSignInStore } = await import('~/composables/signIn/useSignIn')

beforeEach(() => {
  vi.clearAllMocks()
  // Reset mock state
  mockState.id = ''
  mockState.meetingId = ''
  mockState.meetingName = ''
})

describe('useSignIn', () => {
  // 初期化関数が正しいかどうかを検証
  test('正常系_useSignInStore_useState初期化関数が正しい値を返す', () => {
    // Act
    useSignInStore()

    // Assert
    // mockUseState が最初に呼ばれた時の引数を分解。
    // stateName が 'signin' になっているか確認。
    const [stateName, initFunction] = mockUseState.mock.calls[0]
    expect(stateName).toBe('signin')
    
    // その初期化関数を呼んで戻り値を取得し、期待通りか検証
    const initialState = initFunction()
    expect(initialState).toEqual({
      id: '',
      meetingId: '',
      meetingName: ''
    })
  })

  // 初期の戻り値が正しいかを検証（初期化関数から受け取った値に対して、不要な加工が行われていないか）
  test('正常系_useSignInStore_戻り値が正しい初期状態', () => {
    // Act
    const result = useSignInStore()

    // Assert  
    // モックした useState が、'signin' という名前と何らかの初期化関数（expect.any(Function)）と共に呼ばれているかチェック。
    expect(mockUseState).toHaveBeenCalledWith('signin', expect.any(Function))
    // useSignInStore() の戻り値が、期待通りの初期値オブジェクトか確かめてる
    expect(result).toEqual({
      id: '',
      meetingId: '',
      meetingName: ''
    })
  })

  test('正常系_useSignInStore_状態の更新が正しく動作する', () => {
    // Arrange
    const store = useSignInStore()

    // Act
    // ストアのプロパティを好きな値に上書き
    store.id = 'participant123'
    store.meetingId = 'meeting456'
    store.meetingName = 'テスト会議'

    // Assert
    expect(store.id).toBe('participant123')
    expect(store.meetingId).toBe('meeting456')
    expect(store.meetingName).toBe('テスト会議')
  })

  test('正常系_useSignInStore_複数回呼び出しで同じインスタンスを返す', () => {
    // Act
    const store1 = useSignInStore()
    const store2 = useSignInStore()

    // Assert
    expect(store1).toBe(store2)
    // useState モックが2回呼ばれているかを検証
    expect(mockUseState).toHaveBeenCalledTimes(2)
    // 引数が 'signin' と何らかの初期化関数だったかを順番まで含めて検証
    expect(mockUseState).toHaveBeenNthCalledWith(1, 'signin', expect.any(Function))
    expect(mockUseState).toHaveBeenNthCalledWith(2, 'signin', expect.any(Function))
  })
})
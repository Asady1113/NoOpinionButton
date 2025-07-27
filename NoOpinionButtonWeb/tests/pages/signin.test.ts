import { describe, test, expect, beforeEach, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import { nextTick } from 'vue'

// Mock dependencies
const mockSignIn = vi.fn()
const mockPush = vi.fn()
const mockSignInStore = { value: { id: '', meetingId: '', meetingName: '' } }

// Individual mocks (recommended approach)
vi.mock('~/composables/signIn/useSignInApi', () => ({
  useSignInApi: vi.fn(() => ({ signIn: mockSignIn }))
}))

vi.mock('~/composables/signIn/useSignIn', () => ({
  useSignInStore: vi.fn(() => mockSignInStore)
}))

vi.mock('vue-router', () => ({
  useRouter: vi.fn(() => ({ push: mockPush }))
}))

// Import component after mocking
const SigninPage = await import('~/pages/signin.vue')

describe('signin.vue', () => {
  let wrapper: any

  beforeEach(() => {
    vi.clearAllMocks()
    
    // Reset store state
    mockSignInStore.value = { id: '', meetingId: '', meetingName: '' }
    
    // Mount component
    // SigninPage コンポーネントを仮想DOM上にマウントして、テスト対象として使えるようにします
    wrapper = mount(SigninPage.default)
  })

  // コンポーネントを仮想DOMからアンマウントして、テスト間の副作用を避けます。
  afterEach(() => {
    if (wrapper) {
      wrapper.unmount()
    }
  })

  // UI表示・初期状態テスト（3件）
  test('正常系_signin_初期表示が正しく行われる', () => {
    // Assert
    // タイトル表示確認
    expect(wrapper.find('h1').text()).toBe('意見ありませんボタン')
    
    // 入力フィールド表示確認
    const meetingIdInput = wrapper.find('input[placeholder="会議ID"]')
    const passwordInput = wrapper.find('input[placeholder="パスワード"]')
    expect(meetingIdInput.exists()).toBe(true)
    expect(passwordInput.exists()).toBe(true)
    expect(passwordInput.attributes('type')).toBe('password')
    
    // ボタンが無効状態で表示
    const submitButton = wrapper.find('button')
    expect(submitButton.exists()).toBe(true)
    expect(submitButton.text()).toBe('会議に参加')
    expect(submitButton.attributes('disabled')).toBeDefined()
  })

  test('正常系_signin_入力フィールドが正しく動作する', async () => {
    // Arrange
    const meetingIdInput = wrapper.find('input[placeholder="会議ID"]')
    const passwordInput = wrapper.find('input[placeholder="パスワード"]')

    // Act
    await meetingIdInput.setValue('test-meeting')
    await passwordInput.setValue('test-password')

    // Assert
    expect(meetingIdInput.element.value).toBe('test-meeting')
    expect(passwordInput.element.value).toBe('test-password')
  })

  test('正常系_signin_canSubmit算出プロパティが正しく動作する', async () => {
    // Arrange
    const meetingIdInput = wrapper.find('input[placeholder="会議ID"]')
    const passwordInput = wrapper.find('input[placeholder="パスワード"]')
    const submitButton = wrapper.find('button')

    // 初期状態：両フィールド空 → ボタン無効
    expect(submitButton.attributes('disabled')).toBeDefined()

    // Act & Assert: 片方のみ入力 → ボタン無効
    await meetingIdInput.setValue('test-meeting')
    // DOM の更新が完了するまで待つための関数
    await nextTick()
    expect(submitButton.attributes('disabled')).toBeDefined()

    // Act & Assert: 両方入力 → ボタン有効
    await passwordInput.setValue('test-password')
    await nextTick()
    expect(submitButton.attributes('disabled')).toBeUndefined()

    // Act & Assert: 片方を空に → ボタン無効
    await passwordInput.setValue('')
    await nextTick()
    expect(submitButton.attributes('disabled')).toBeDefined()
  })

  // フォーム送信テスト（4件）
  test('正常系_signin_司会者ログイン成功時の画面遷移', async () => {
    // Arrange
    const mockResponse = {
      Data: {
        id: 'facilitator123',
        meetingId: 'meeting456',
        meetingName: 'テスト会議',
        isFacilitator: true
      }
    }
    mockSignIn.mockResolvedValue(mockResponse)

    await wrapper.find('input[placeholder="会議ID"]').setValue('meeting456')
    await wrapper.find('input[placeholder="パスワード"]').setValue('facilitator-pass')

    // Act
    await wrapper.find('button').trigger('click')
    await nextTick()

    // Assert
    // API呼び出し確認
    expect(mockSignIn).toHaveBeenCalledWith('meeting456', 'facilitator-pass')
    
    // ストア更新確認
    expect(mockSignInStore.value).toEqual({
      id: 'facilitator123',
      meetingId: 'meeting456',
      meetingName: 'テスト会議'
    })
    
    // 画面遷移確認
    expect(mockPush).toHaveBeenCalledWith('/facilitator')
  })

  test('正常系_signin_参加者ログイン成功時の画面遷移', async () => {
    // Arrange
    const mockResponse = {
      Data: {
        id: 'participant123',
        meetingId: 'meeting456',
        meetingName: 'テスト会議',
        isFacilitator: false
      }
    }
    mockSignIn.mockResolvedValue(mockResponse)

    await wrapper.find('input[placeholder="会議ID"]').setValue('meeting456')
    await wrapper.find('input[placeholder="パスワード"]').setValue('participant-pass')

    // Act
    await wrapper.find('button').trigger('click')
    await nextTick()

    // Assert
    // API呼び出し確認
    expect(mockSignIn).toHaveBeenCalledWith('meeting456', 'participant-pass')
    
    // ストア更新確認
    expect(mockSignInStore.value).toEqual({
      id: 'participant123',
      meetingId: 'meeting456',
      meetingName: 'テスト会議'
    })
    
    // 画面遷移確認
    expect(mockPush).toHaveBeenCalledWith('/participant')
  })

  test('異常系_signin_APIエラー時のエラーメッセージ表示', async () => {
    // Arrange
    const errorMessage = 'パスワードが間違っています'
    mockSignIn.mockRejectedValue(new Error(errorMessage))

    await wrapper.find('input[placeholder="会議ID"]').setValue('meeting456')
    await wrapper.find('input[placeholder="パスワード"]').setValue('wrong-password')

    // Act
    await wrapper.find('button').trigger('click')
    await nextTick()

    // Assert
    // API呼び出し確認
    expect(mockSignIn).toHaveBeenCalledWith('meeting456', 'wrong-password')
    
    // エラーメッセージ表示確認
    const errorElement = wrapper.find('p.text-red-500')
    expect(errorElement.exists()).toBe(true)
    expect(errorElement.text()).toBe(errorMessage)
    expect(errorElement.classes()).not.toContain('opacity-0')
    
    // 画面遷移しないことを確認
    expect(mockPush).not.toHaveBeenCalled()
  })

  test('正常系_signin_エラーメッセージの表示制御', async () => {
    // Arrange
    const errorElement = wrapper.find('p.text-red-500')

    // 初期状態：エラーなし → 透明
    expect(errorElement.classes()).toContain('opacity-0')

    // エラー発生
    const errorMessage = '会議が見つかりません'
    mockSignIn.mockRejectedValue(new Error(errorMessage))

    await wrapper.find('input[placeholder="会議ID"]').setValue('invalid-meeting')
    await wrapper.find('input[placeholder="パスワード"]').setValue('password')

    // Act
    await wrapper.find('button').trigger('click')
    await nextTick()

    // Assert
    // エラーあり時：表示、赤色
    expect(errorElement.text()).toBe(errorMessage)
    expect(errorElement.classes()).not.toContain('opacity-0')
    expect(errorElement.classes()).toContain('text-red-500')
  })

  // イベントハンドリングテスト（2件）
  test('正常系_signin_ボタンクリックイベント', async () => {
    // Arrange - 有効状態にする
    await wrapper.find('input[placeholder="会議ID"]').setValue('meeting123')
    await wrapper.find('input[placeholder="パスワード"]').setValue('password123')
    mockSignIn.mockResolvedValue({ Data: { isFacilitator: false } })

    // Act
    await wrapper.find('button').trigger('click')

    // Assert
    expect(mockSignIn).toHaveBeenCalledTimes(1)
  })

  test('正常系_signin_無効状態でのボタンクリック', async () => {
    // Arrange - 無効状態（入力なし）
    const submitButton = wrapper.find('button')
    expect(submitButton.attributes('disabled')).toBeDefined()

    // Act
    await submitButton.trigger('click')

    // Assert - submit関数が呼ばれないことを確認
    expect(mockSignIn).not.toHaveBeenCalled()
  })

})
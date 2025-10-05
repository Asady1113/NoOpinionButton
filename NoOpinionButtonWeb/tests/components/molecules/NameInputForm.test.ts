import { describe, test, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { nextTick } from 'vue'
import NameInputForm from '~/components/molecules/NameInputForm.vue'
import { ApiErrorType } from '~/types/error'
import type { ApiError } from '~/types/error'

// Mock the composable
const mockUpdateParticipantName = vi.fn()
vi.mock('~/composables/participantName/useParticipantNameApi', () => ({
  useParticipantNameApi: () => ({
    updateParticipantName: mockUpdateParticipantName
  })
}))

// Mock child components
vi.mock('~/components/atoms/TextInput.vue', () => ({
  default: {
    name: 'TextInput',
    props: ['modelValue', 'placeholder', 'maxLength', 'disabled'],
    emits: ['update:modelValue', 'input'],
    template: `
      <input 
        :value="modelValue" 
        @input="$emit('update:modelValue', $event.target.value); $emit('input')"
        :placeholder="placeholder"
        :maxlength="maxLength"
        :disabled="disabled"
        data-testid="text-input"
      />
    `
  }
}))

vi.mock('~/components/atoms/ErrorMessage.vue', () => ({
  default: {
    name: 'ErrorMessage',
    props: ['message', 'visible'],
    template: `<div v-if="visible && message" data-testid="error-message">{{ message }}</div>`
  }
}))

vi.mock('~/components/atoms/SubmitButton.vue', () => ({
  default: {
    name: 'SubmitButton',
    props: ['text', 'disabled', 'loading'],
    emits: ['click'],
    template: `
      <button 
        @click="$emit('click')" 
        :disabled="disabled"
        data-testid="submit-button"
      >
        {{ text }}
        <span v-if="loading" data-testid="loading-indicator">Loading...</span>
      </button>
    `
  }
}))

beforeEach(() => {
  vi.clearAllMocks()
})

describe('NameInputForm', () => {
  test('正常系_レンダリング_デフォルトプロパティで正常表示', () => {
    // Arrange & Act
    const wrapper = mount(NameInputForm, {
      props: {
        participantId: 'participant123'
      }
    })

    // Assert
    expect(wrapper.find('[data-testid="text-input"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="submit-button"]').exists()).toBe(true)
    expect(wrapper.find('form').exists()).toBe(true)
    
    const textInput = wrapper.find('[data-testid="text-input"]')
    expect(textInput.attributes('placeholder')).toBe('ユーザー名を入力してください')
    expect(textInput.attributes('maxlength')).toBe('50')
    
    const submitButton = wrapper.find('[data-testid="submit-button"]')
    expect(submitButton.text()).toContain('決定')
  })

  test('正常系_カスタムプロパティ_カスタムプロパティが正しく適用される', () => {
    // Arrange & Act
    const wrapper = mount(NameInputForm, {
      props: {
        participantId: 'participant123',
        placeholder: 'カスタムプレースホルダー',
        maxLength: 30,
        buttonText: 'カスタムボタン'
      }
    })

    // Assert
    const textInput = wrapper.find('[data-testid="text-input"]')
    expect(textInput.attributes('placeholder')).toBe('カスタムプレースホルダー')
    expect(textInput.attributes('maxlength')).toBe('30')
    
    const submitButton = wrapper.find('[data-testid="submit-button"]')
    expect(submitButton.text()).toContain('カスタムボタン')
  })

  test('正常系_入力検証_有効な入力でボタンが有効化される', async () => {
    // Arrange
    const wrapper = mount(NameInputForm, {
      props: {
        participantId: 'participant123'
      }
    })

    // Act
    const textInput = wrapper.find('[data-testid="text-input"]')
    await textInput.setValue('有効な名前')
    await textInput.trigger('input')
    await nextTick()

    // Assert
    const submitButton = wrapper.find('[data-testid="submit-button"]')
    expect(submitButton.attributes('disabled')).toBeUndefined()
  })

  test('正常系_入力検証_空入力でボタンが無効化される', async () => {
    // Arrange
    const wrapper = mount(NameInputForm, {
      props: {
        participantId: 'participant123'
      }
    })

    // Act
    const textInput = wrapper.find('[data-testid="text-input"]')
    await textInput.setValue('')
    await textInput.trigger('input')
    await nextTick()

    // Assert
    const submitButton = wrapper.find('[data-testid="submit-button"]')
    expect(submitButton.attributes('disabled')).toBeDefined()
  })

  test('正常系_入力検証_空白のみの入力でエラーメッセージ表示', async () => {
    // Arrange
    const wrapper = mount(NameInputForm, {
      props: {
        participantId: 'participant123'
      }
    })

    // Act
    const textInput = wrapper.find('[data-testid="text-input"]')
    await textInput.setValue('   ')
    await textInput.trigger('input')
    await nextTick()

    // Assert
    const errorMessage = wrapper.find('[data-testid="error-message"]')
    expect(errorMessage.exists()).toBe(true)
    expect(errorMessage.text()).toBe('有効な名前を入力してください')
  })

  test('正常系_入力検証_長すぎる入力でエラーメッセージ表示', async () => {
    // Arrange
    const wrapper = mount(NameInputForm, {
      props: {
        participantId: 'participant123',
        maxLength: 10
      }
    })

    // Act
    const textInput = wrapper.find('[data-testid="text-input"]')
    await textInput.setValue('これは10文字を超える長い名前です')
    await textInput.trigger('input')
    await nextTick()

    // Assert
    const errorMessage = wrapper.find('[data-testid="error-message"]')
    expect(errorMessage.exists()).toBe(true)
    expect(errorMessage.text()).toBe('名前は10文字以内で入力してください')
  })

  test('正常系_フォーム送信_有効な入力で成功イベント発火', async () => {
    // Arrange
    mockUpdateParticipantName.mockResolvedValue({
      Data: { updatedName: 'テスト太郎' },
      Error: null
    })

    const wrapper = mount(NameInputForm, {
      props: {
        participantId: 'participant123'
      }
    })

    // Act
    const textInput = wrapper.find('[data-testid="text-input"]')
    await textInput.setValue('テスト太郎')
    await textInput.trigger('input')
    await nextTick()

    const submitButton = wrapper.find('[data-testid="submit-button"]')
    await submitButton.trigger('click')
    await nextTick()

    // Assert
    expect(mockUpdateParticipantName).toHaveBeenCalledWith('participant123', 'テスト太郎')
    
    const emittedEvents = wrapper.emitted('success')
    expect(emittedEvents).toBeTruthy()
    expect(emittedEvents!.length).toBe(1)
  })

  test('正常系_フォーム送信_ローディング状態の表示', async () => {
    // Arrange
    let resolvePromise: (value: any) => void
    const promise = new Promise((resolve) => {
      resolvePromise = resolve
    })
    mockUpdateParticipantName.mockReturnValue(promise)

    const wrapper = mount(NameInputForm, {
      props: {
        participantId: 'participant123'
      }
    })

    // Act
    const textInput = wrapper.find('[data-testid="text-input"]')
    await textInput.setValue('テスト太郎')
    await textInput.trigger('input')
    await nextTick()

    const submitButton = wrapper.find('[data-testid="submit-button"]')
    await submitButton.trigger('click')
    await nextTick()

    // Assert - ローディング状態
    expect(wrapper.find('[data-testid="loading-indicator"]').exists()).toBe(true)
    expect(textInput.attributes('disabled')).toBeDefined()
    expect(submitButton.attributes('disabled')).toBeDefined()

    // Cleanup
    resolvePromise!({
      Data: { updatedName: 'テスト太郎' },
      Error: null
    })
    await nextTick()
  })

  test('異常系_API エラー_BadRequest エラーでエラーイベント発火', async () => {
    // Arrange
    const apiError: ApiError = {
      type: ApiErrorType.BadRequest,
      message: 'Invalid input',
      statusCode: 400
    }
    mockUpdateParticipantName.mockRejectedValue(apiError)

    const wrapper = mount(NameInputForm, {
      props: {
        participantId: 'participant123'
      }
    })

    // Act
    const textInput = wrapper.find('[data-testid="text-input"]')
    await textInput.setValue('テスト太郎')
    await textInput.trigger('input')
    await nextTick()

    const submitButton = wrapper.find('[data-testid="submit-button"]')
    await submitButton.trigger('click')
    await nextTick()

    // Assert
    const errorMessage = wrapper.find('[data-testid="error-message"]')
    expect(errorMessage.exists()).toBe(true)
    expect(errorMessage.text()).toBe('入力内容に問題があります。名前を確認してください。')
    
    const emittedEvents = wrapper.emitted('error')
    expect(emittedEvents).toBeTruthy()
    expect(emittedEvents![0]).toEqual(['入力内容に問題があります。名前を確認してください。'])
  })

  test('異常系_API エラー_NotFound エラーでエラーイベント発火', async () => {
    // Arrange
    const apiError: ApiError = {
      type: ApiErrorType.NotFound,
      message: 'Participant not found',
      statusCode: 404
    }
    mockUpdateParticipantName.mockRejectedValue(apiError)

    const wrapper = mount(NameInputForm, {
      props: {
        participantId: 'participant123'
      }
    })

    // Act
    const textInput = wrapper.find('[data-testid="text-input"]')
    await textInput.setValue('テスト太郎')
    await textInput.trigger('input')
    await nextTick()

    const submitButton = wrapper.find('[data-testid="submit-button"]')
    await submitButton.trigger('click')
    await nextTick()

    // Assert
    const errorMessage = wrapper.find('[data-testid="error-message"]')
    expect(errorMessage.exists()).toBe(true)
    expect(errorMessage.text()).toBe('参加者が見つかりません。ページを再読み込みしてください。')
    
    const emittedEvents = wrapper.emitted('error')
    expect(emittedEvents).toBeTruthy()
    expect(emittedEvents![0]).toEqual(['参加者が見つかりません。ページを再読み込みしてください。'])
  })

  test('異常系_API エラー_Server エラーでエラーイベント発火', async () => {
    // Arrange
    const apiError: ApiError = {
      type: ApiErrorType.Server,
      message: 'ネットワークエラーが発生しました。インターネット接続を確認してください。',
      statusCode: 0
    }
    mockUpdateParticipantName.mockRejectedValue(apiError)

    const wrapper = mount(NameInputForm, {
      props: {
        participantId: 'participant123'
      }
    })

    // Act
    const textInput = wrapper.find('[data-testid="text-input"]')
    await textInput.setValue('テスト太郎')
    await textInput.trigger('input')
    await nextTick()

    const submitButton = wrapper.find('[data-testid="submit-button"]')
    await submitButton.trigger('click')
    await nextTick()

    // Assert
    const errorMessage = wrapper.find('[data-testid="error-message"]')
    expect(errorMessage.exists()).toBe(true)
    expect(errorMessage.text()).toBe('ネットワークエラーが発生しました。インターネット接続を確認してください。')
    
    const emittedEvents = wrapper.emitted('error')
    expect(emittedEvents).toBeTruthy()
    expect(emittedEvents![0]).toEqual(['ネットワークエラーが発生しました。インターネット接続を確認してください。'])
  })

  test('異常系_レスポンスエラー_Error フィールドありでエラーイベント発火', async () => {
    // Arrange
    mockUpdateParticipantName.mockResolvedValue({
      Data: null,
      Error: 'サーバー側でエラーが発生しました'
    })

    const wrapper = mount(NameInputForm, {
      props: {
        participantId: 'participant123'
      }
    })

    // Act
    const textInput = wrapper.find('[data-testid="text-input"]')
    await textInput.setValue('テスト太郎')
    await textInput.trigger('input')
    await nextTick()

    const submitButton = wrapper.find('[data-testid="submit-button"]')
    await submitButton.trigger('click')
    await nextTick()

    // Assert
    const errorMessage = wrapper.find('[data-testid="error-message"]')
    expect(errorMessage.exists()).toBe(true)
    expect(errorMessage.text()).toBe('サーバー側でエラーが発生しました')
    
    const emittedEvents = wrapper.emitted('error')
    expect(emittedEvents).toBeTruthy()
    expect(emittedEvents![0]).toEqual(['サーバー側でエラーが発生しました'])
  })

  test('正常系_フォーム送信防止_無効な入力では送信されない', async () => {
    // Arrange
    const wrapper = mount(NameInputForm, {
      props: {
        participantId: 'participant123'
      }
    })

    // Act - 空の入力で送信を試行
    const submitButton = wrapper.find('[data-testid="submit-button"]')
    await submitButton.trigger('click')
    await nextTick()

    // Assert
    expect(mockUpdateParticipantName).not.toHaveBeenCalled()
    expect(wrapper.emitted('success')).toBeFalsy()
    expect(wrapper.emitted('error')).toBeFalsy()
  })

  test('正常系_日本語入力_日本語名前で正常処理', async () => {
    // Arrange
    mockUpdateParticipantName.mockResolvedValue({
      Data: { updatedName: '田中太郎' },
      Error: null
    })

    const wrapper = mount(NameInputForm, {
      props: {
        participantId: 'participant123'
      }
    })

    // Act
    const textInput = wrapper.find('[data-testid="text-input"]')
    await textInput.setValue('田中太郎')
    await textInput.trigger('input')
    await nextTick()

    const submitButton = wrapper.find('[data-testid="submit-button"]')
    await submitButton.trigger('click')
    await nextTick()

    // Assert
    expect(mockUpdateParticipantName).toHaveBeenCalledWith('participant123', '田中太郎')
    expect(wrapper.emitted('success')).toBeTruthy()
  })

  test('正常系_絵文字入力_絵文字を含む名前で正常処理', async () => {
    // Arrange
    mockUpdateParticipantName.mockResolvedValue({
      Data: { updatedName: 'テスト😊' },
      Error: null
    })

    const wrapper = mount(NameInputForm, {
      props: {
        participantId: 'participant123'
      }
    })

    // Act
    const textInput = wrapper.find('[data-testid="text-input"]')
    await textInput.setValue('テスト😊')
    await textInput.trigger('input')
    await nextTick()

    const submitButton = wrapper.find('[data-testid="submit-button"]')
    await submitButton.trigger('click')
    await nextTick()

    // Assert
    expect(mockUpdateParticipantName).toHaveBeenCalledWith('participant123', 'テスト😊')
    expect(wrapper.emitted('success')).toBeTruthy()
  })

  test('正常系_入力検証_1文字の名前で有効', async () => {
    // Arrange
    const wrapper = mount(NameInputForm, {
      props: {
        participantId: 'participant123'
      }
    })

    // Act
    const textInput = wrapper.find('[data-testid="text-input"]')
    await textInput.setValue('A')
    await textInput.trigger('input')
    await nextTick()

    // Assert
    const submitButton = wrapper.find('[data-testid="submit-button"]')
    expect(submitButton.attributes('disabled')).toBeUndefined()
    expect(wrapper.find('[data-testid="error-message"]').exists()).toBe(false)
  })

  test('正常系_入力検証_50文字の名前で有効', async () => {
    // Arrange
    const wrapper = mount(NameInputForm, {
      props: {
        participantId: 'participant123'
      }
    })

    // Act - 50文字の名前
    const fiftyCharName = 'あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやゆよらりるれろわをん'
    const textInput = wrapper.find('[data-testid="text-input"]')
    await textInput.setValue(fiftyCharName)
    await textInput.trigger('input')
    await nextTick()

    // Assert
    const submitButton = wrapper.find('[data-testid="submit-button"]')
    expect(submitButton.attributes('disabled')).toBeUndefined()
    expect(wrapper.find('[data-testid="error-message"]').exists()).toBe(false)
  })

  test('異常系_入力検証_51文字の名前で無効', async () => {
    // Arrange
    const wrapper = mount(NameInputForm, {
      props: {
        participantId: 'participant123'
      }
    })

    // Act - 51文字の名前を確実に作成
    const fiftyOneCharName = 'a'.repeat(51)
    
    const textInput = wrapper.find('[data-testid="text-input"]')
    await textInput.setValue(fiftyOneCharName)
    await textInput.trigger('input')
    await nextTick()

    // Assert
    const submitButton = wrapper.find('[data-testid="submit-button"]')
    expect(submitButton.attributes('disabled')).toBeDefined()
    
    const errorMessage = wrapper.find('[data-testid="error-message"]')
    expect(errorMessage.exists()).toBe(true)
    expect(errorMessage.text()).toBe('名前は50文字以内で入力してください')
  })

  test('異常系_入力検証_前後に空白がある名前で正常処理', async () => {
    // Arrange
    mockUpdateParticipantName.mockResolvedValue({
      Data: { updatedName: 'テスト太郎' },
      Error: null
    })

    const wrapper = mount(NameInputForm, {
      props: {
        participantId: 'participant123'
      }
    })

    // Act - 前後に空白がある名前
    const textInput = wrapper.find('[data-testid="text-input"]')
    await textInput.setValue('  テスト太郎  ')
    await textInput.trigger('input')
    await nextTick()

    const submitButton = wrapper.find('[data-testid="submit-button"]')
    await submitButton.trigger('click')
    await nextTick()

    // Assert - トリムされた名前で API が呼ばれる
    expect(mockUpdateParticipantName).toHaveBeenCalledWith('participant123', 'テスト太郎')
    expect(wrapper.emitted('success')).toBeTruthy()
  })

  test('異常系_フォーム送信_空入力で送信時にエラーメッセージ表示', async () => {
    // Arrange
    const wrapper = mount(NameInputForm, {
      props: {
        participantId: 'participant123'
      }
    })

    // Act - 空入力で送信を試行（ボタンは無効化されているが、直接handleSubmitを呼ぶ）
    // フォーム送信イベントをトリガー
    const form = wrapper.find('form')
    await form.trigger('submit')
    await nextTick()

    // Assert
    expect(mockUpdateParticipantName).not.toHaveBeenCalled()
    
    const errorMessage = wrapper.find('[data-testid="error-message"]')
    expect(errorMessage.exists()).toBe(true)
    expect(errorMessage.text()).toBe('名前を入力してください')
  })

  test('異常系_フォーム送信_空白のみ入力で送信時にエラーメッセージ表示', async () => {
    // Arrange
    const wrapper = mount(NameInputForm, {
      props: {
        participantId: 'participant123'
      }
    })

    // Act - 空白のみ入力で送信を試行
    const textInput = wrapper.find('[data-testid="text-input"]')
    await textInput.setValue('   ')
    await textInput.trigger('input')
    await nextTick()

    const submitButton = wrapper.find('[data-testid="submit-button"]')
    await submitButton.trigger('click')
    await nextTick()

    // Assert
    expect(mockUpdateParticipantName).not.toHaveBeenCalled()
    
    const errorMessage = wrapper.find('[data-testid="error-message"]')
    expect(errorMessage.exists()).toBe(true)
    expect(errorMessage.text()).toBe('有効な名前を入力してください')
  })

  test('異常系_API エラー_Unauthorized エラーでエラーイベント発火', async () => {
    // Arrange
    const apiError: ApiError = {
      type: ApiErrorType.Unauthorized,
      message: 'Unauthorized',
      statusCode: 401
    }
    mockUpdateParticipantName.mockRejectedValue(apiError)

    const wrapper = mount(NameInputForm, {
      props: {
        participantId: 'participant123'
      }
    })

    // Act
    const textInput = wrapper.find('[data-testid="text-input"]')
    await textInput.setValue('テスト太郎')
    await textInput.trigger('input')
    await nextTick()

    const submitButton = wrapper.find('[data-testid="submit-button"]')
    await submitButton.trigger('click')
    await nextTick()

    // Assert
    const errorMessage = wrapper.find('[data-testid="error-message"]')
    expect(errorMessage.exists()).toBe(true)
    expect(errorMessage.text()).toBe('認証に失敗しました。ページを再読み込みしてください。')
    
    const emittedEvents = wrapper.emitted('error')
    expect(emittedEvents).toBeTruthy()
    expect(emittedEvents![0]).toEqual(['認証に失敗しました。ページを再読み込みしてください。'])
  })

  test('異常系_API エラー_500 Server エラーでエラーイベント発火', async () => {
    // Arrange
    const apiError: ApiError = {
      type: ApiErrorType.Server,
      message: '',
      statusCode: 500
    }
    mockUpdateParticipantName.mockRejectedValue(apiError)

    const wrapper = mount(NameInputForm, {
      props: {
        participantId: 'participant123'
      }
    })

    // Act
    const textInput = wrapper.find('[data-testid="text-input"]')
    await textInput.setValue('テスト太郎')
    await textInput.trigger('input')
    await nextTick()

    const submitButton = wrapper.find('[data-testid="submit-button"]')
    await submitButton.trigger('click')
    await nextTick()

    // Assert
    const errorMessage = wrapper.find('[data-testid="error-message"]')
    expect(errorMessage.exists()).toBe(true)
    expect(errorMessage.text()).toBe('サーバーエラーが発生しました。しばらく時間をおいて再度お試しください。')
    
    const emittedEvents = wrapper.emitted('error')
    expect(emittedEvents).toBeTruthy()
    expect(emittedEvents![0]).toEqual(['サーバーエラーが発生しました。しばらく時間をおいて再度お試しください。'])
  })

  test('異常系_API エラー_タイムアウトエラーでエラーイベント発火', async () => {
    // Arrange
    const apiError: ApiError = {
      type: ApiErrorType.Server,
      message: 'リクエストがタイムアウトしました。しばらく時間をおいて再度お試しください。',
      statusCode: 0
    }
    mockUpdateParticipantName.mockRejectedValue(apiError)

    const wrapper = mount(NameInputForm, {
      props: {
        participantId: 'participant123'
      }
    })

    // Act
    const textInput = wrapper.find('[data-testid="text-input"]')
    await textInput.setValue('テスト太郎')
    await textInput.trigger('input')
    await nextTick()

    const submitButton = wrapper.find('[data-testid="submit-button"]')
    await submitButton.trigger('click')
    await nextTick()

    // Assert
    const errorMessage = wrapper.find('[data-testid="error-message"]')
    expect(errorMessage.exists()).toBe(true)
    expect(errorMessage.text()).toBe('リクエストがタイムアウトしました。しばらく時間をおいて再度お試しください。')
    
    const emittedEvents = wrapper.emitted('error')
    expect(emittedEvents).toBeTruthy()
    expect(emittedEvents![0]).toEqual(['リクエストがタイムアウトしました。しばらく時間をおいて再度お試しください。'])
  })

  test('正常系_エラー回復_エラー後に再試行で成功', async () => {
    // Arrange
    const apiError: ApiError = {
      type: ApiErrorType.Server,
      message: 'ネットワークエラーが発生しました。インターネット接続を確認してください。',
      statusCode: 0
    }
    
    // 最初はエラー、2回目は成功
    mockUpdateParticipantName
      .mockRejectedValueOnce(apiError)
      .mockResolvedValueOnce({
        Data: { updatedName: 'テスト太郎' },
        Error: null
      })

    const wrapper = mount(NameInputForm, {
      props: {
        participantId: 'participant123'
      }
    })

    // Act - 最初の試行（エラー）
    const textInput = wrapper.find('[data-testid="text-input"]')
    await textInput.setValue('テスト太郎')
    await textInput.trigger('input')
    await nextTick()

    const submitButton = wrapper.find('[data-testid="submit-button"]')
    await submitButton.trigger('click')
    await nextTick()

    // Assert - エラーメッセージが表示される
    let errorMessage = wrapper.find('[data-testid="error-message"]')
    expect(errorMessage.exists()).toBe(true)
    expect(errorMessage.text()).toBe('ネットワークエラーが発生しました。インターネット接続を確認してください。')

    // Act - 再試行（成功）
    await submitButton.trigger('click')
    await nextTick()

    // Assert - 成功イベントが発火される
    const emittedSuccessEvents = wrapper.emitted('success')
    expect(emittedSuccessEvents).toBeTruthy()
    expect(emittedSuccessEvents!.length).toBe(1)
  })
})
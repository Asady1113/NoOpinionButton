import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import ApiErrorMessage from '~/components/atoms/ApiErrorMessage.vue'
import { ApiErrorType, type ApiError } from '~/types/error'

describe('ApiErrorMessage', () => {
  const createError = (type: ApiErrorType, message: string): ApiError => ({
    type,
    message,
    statusCode: 0
  })

  it('renders WebSocket connection error correctly', () => {
    const error = createError(ApiErrorType.WebSocketConnection, 'WebSocket接続でエラーが発生しました')
    
    const wrapper = mount(ApiErrorMessage, {
      props: {
        error,
        visible: true
      }
    })

    expect(wrapper.text()).toContain('接続エラー')
    expect(wrapper.text()).toContain('WebSocket接続でエラーが発生しました')
    expect(wrapper.find('[role="alert"]').exists()).toBe(true)
  })

  it('renders WebSocket message error correctly', () => {
    const error = createError(ApiErrorType.WebSocketMessage, '受信したメッセージの形式が正しくありません')
    
    const wrapper = mount(ApiErrorMessage, {
      props: {
        error,
        visible: true
      }
    })

    expect(wrapper.text()).toContain('メッセージエラー')
    expect(wrapper.text()).toContain('受信したメッセージの形式が正しくありません')
  })

  it('shows retry button for connection errors', () => {
    const error = createError(ApiErrorType.WebSocketConnection, 'connection failed')
    
    const wrapper = mount(ApiErrorMessage, {
      props: {
        error,
        visible: true,
        showRetryButton: true
      }
    })

    const retryButton = wrapper.find('button:contains("再試行")')
    expect(retryButton.exists()).toBe(true)
  })

  it('emits retry event when retry button is clicked', async () => {
    const error = createError(ApiErrorType.WebSocketConnection, 'connection failed')
    
    const wrapper = mount(ApiErrorMessage, {
      props: {
        error,
        visible: true,
        showRetryButton: true
      }
    })

    const retryButton = wrapper.find('button')
    await retryButton.trigger('click')

    expect(wrapper.emitted('retry')).toBeTruthy()
  })

  it('shows dismiss button when dismissible', () => {
    const error = createError(ApiErrorType.WebSocketMessage, 'test error')
    
    const wrapper = mount(ApiErrorMessage, {
      props: {
        error,
        visible: true,
        dismissible: true
      }
    })

    const dismissButton = wrapper.find('button[aria-label="閉じる"], button:has(span.sr-only)')
    expect(dismissButton.exists()).toBe(true)
  })

  it('emits dismiss event when dismiss button is clicked', async () => {
    const error = createError(ApiErrorType.WebSocketMessage, 'test error')
    
    const wrapper = mount(ApiErrorMessage, {
      props: {
        error,
        visible: true,
        dismissible: true
      }
    })

    const dismissButton = wrapper.find('button:last-child')
    await dismissButton.trigger('click')

    expect(wrapper.emitted('dismiss')).toBeTruthy()
  })

  it('does not render when visible is false', () => {
    const error = createError(ApiErrorType.WebSocketConnection, 'test error')
    
    const wrapper = mount(ApiErrorMessage, {
      props: {
        error,
        visible: false
      }
    })

    expect(wrapper.find('[role="alert"]').exists()).toBe(false)
  })

  it('does not render when error is null', () => {
    const wrapper = mount(ApiErrorMessage, {
      props: {
        error: null,
        visible: true
      }
    })

    expect(wrapper.find('[role="alert"]').exists()).toBe(false)
  })

  it('applies correct styling for different variants', () => {
    const error = createError(ApiErrorType.WebSocketConnection, 'test error')
    
    const wrapper = mount(ApiErrorMessage, {
      props: {
        error,
        visible: true,
        variant: 'warning'
      }
    })

    expect(wrapper.find('.bg-yellow-50').exists()).toBe(true)
    expect(wrapper.find('.border-yellow-200').exists()).toBe(true)
  })

  it('provides user-friendly messages for timeout errors', () => {
    const error = createError(ApiErrorType.WebSocketConnection, 'WebSocket connection timeout')
    
    const wrapper = mount(ApiErrorMessage, {
      props: {
        error,
        visible: true
      }
    })

    expect(wrapper.text()).toContain('接続がタイムアウトしました')
    expect(wrapper.text()).toContain('ネットワーク接続を確認してください')
  })

  it('provides user-friendly messages for connection failed errors', () => {
    const error = createError(ApiErrorType.WebSocketConnection, 'WebSocket connection failed')
    
    const wrapper = mount(ApiErrorMessage, {
      props: {
        error,
        visible: true
      }
    })

    expect(wrapper.text()).toContain('サーバーに接続できませんでした')
    expect(wrapper.text()).toContain('しばらく待ってから再試行してください')
  })

  it('provides user-friendly messages for max retry errors', () => {
    const error = createError(ApiErrorType.WebSocketConnection, '接続の再試行回数が上限に達しました')
    
    const wrapper = mount(ApiErrorMessage, {
      props: {
        error,
        visible: true
      }
    })

    expect(wrapper.text()).toContain('ページを再読み込みしてください')
  })
})
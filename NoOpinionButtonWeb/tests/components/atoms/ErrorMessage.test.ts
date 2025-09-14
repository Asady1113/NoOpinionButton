import { describe, test, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import ErrorMessage from '~/components/atoms/ErrorMessage.vue'

describe('ErrorMessage', () => {
  test('正常系_表示状態_メッセージとvisibleがtrueの場合に表示される', () => {
    // Arrange & Act
    const wrapper = mount(ErrorMessage, {
      props: {
        message: 'エラーメッセージです',
        visible: true
      }
    })

    // Assert
    const errorDiv = wrapper.find('div[role="alert"]')
    expect(errorDiv.exists()).toBe(true)
    expect(errorDiv.text()).toBe('エラーメッセージです')
    expect(errorDiv.attributes('role')).toBe('alert')
    expect(errorDiv.attributes('aria-live')).toBe('polite')
    expect(errorDiv.classes()).toContain('text-red-500')
    expect(errorDiv.classes()).toContain('text-sm')
    expect(errorDiv.classes()).toContain('mt-2')
    expect(errorDiv.classes()).toContain('transition-opacity')
  })

  test('正常系_非表示状態_visibleがfalseの場合に非表示要素が表示される', () => {
    // Arrange & Act
    const wrapper = mount(ErrorMessage, {
      props: {
        message: 'エラーメッセージです',
        visible: false
      }
    })

    // Assert
    const errorDiv = wrapper.find('div[role="alert"]')
    expect(errorDiv.exists()).toBe(false)
    
    const hiddenDiv = wrapper.find('div[aria-hidden="true"]')
    expect(hiddenDiv.exists()).toBe(true)
    expect(hiddenDiv.classes()).toContain('opacity-0')
    expect(hiddenDiv.attributes('aria-hidden')).toBe('true')
    expect(hiddenDiv.text()).toBe('') // &nbsp; may not render in test environment
  })

  test('正常系_メッセージなし_messageが空の場合に非表示', () => {
    // Arrange & Act
    const wrapper = mount(ErrorMessage, {
      props: {
        message: '',
        visible: true
      }
    })

    // Assert
    const errorDiv = wrapper.find('div[role="alert"]')
    expect(errorDiv.exists()).toBe(false)
    
    // When message is empty and visible is true, no div is shown
    const hiddenDiv = wrapper.find('div[aria-hidden="true"]')
    expect(hiddenDiv.exists()).toBe(false)
  })

  test('正常系_メッセージ未定義_messageがundefinedの場合に非表示', () => {
    // Arrange & Act
    const wrapper = mount(ErrorMessage, {
      props: {
        visible: true
      }
    })

    // Assert
    const errorDiv = wrapper.find('div[role="alert"]')
    expect(errorDiv.exists()).toBe(false)
    
    // When message is undefined and visible is true, no div is shown
    const hiddenDiv = wrapper.find('div[aria-hidden="true"]')
    expect(hiddenDiv.exists()).toBe(false)
  })

  test('正常系_デフォルトプロパティ_プロパティ未指定時の動作', () => {
    // Arrange & Act
    const wrapper = mount(ErrorMessage, {
      props: {}
    })

    // Assert
    const errorDiv = wrapper.find('div[role="alert"]')
    expect(errorDiv.exists()).toBe(false)
    
    const hiddenDiv = wrapper.find('div[aria-hidden="true"]')
    expect(hiddenDiv.exists()).toBe(true)
    expect(hiddenDiv.classes()).toContain('opacity-0')
  })

  test('正常系_長いエラーメッセージ_長いメッセージでも正常表示', () => {
    // Arrange & Act
    const longMessage = 'これは非常に長いエラーメッセージです。複数行にわたる可能性があり、ユーザーに詳細な情報を提供します。'
    const wrapper = mount(ErrorMessage, {
      props: {
        message: longMessage,
        visible: true
      }
    })

    // Assert
    const errorDiv = wrapper.find('div[role="alert"]')
    expect(errorDiv.exists()).toBe(true)
    expect(errorDiv.text()).toBe(longMessage)
  })

  test('正常系_日本語メッセージ_日本語エラーメッセージが正常表示', () => {
    // Arrange & Act
    const japaneseMessage = '名前を入力してください'
    const wrapper = mount(ErrorMessage, {
      props: {
        message: japaneseMessage,
        visible: true
      }
    })

    // Assert
    const errorDiv = wrapper.find('div[role="alert"]')
    expect(errorDiv.exists()).toBe(true)
    expect(errorDiv.text()).toBe(japaneseMessage)
  })

  test('正常系_アクセシビリティ_スクリーンリーダー対応', () => {
    // Arrange & Act
    const wrapper = mount(ErrorMessage, {
      props: {
        message: 'アクセシビリティテスト',
        visible: true
      }
    })

    // Assert
    const errorDiv = wrapper.find('div[role="alert"]')
    expect(errorDiv.attributes('role')).toBe('alert')
    expect(errorDiv.attributes('aria-live')).toBe('polite')
  })

  test('正常系_表示切り替え_visibleの変更で表示状態が切り替わる', async () => {
    // Arrange
    const wrapper = mount(ErrorMessage, {
      props: {
        message: 'テストメッセージ',
        visible: false
      }
    })

    // 初期状態：非表示
    expect(wrapper.find('div[role="alert"]').exists()).toBe(false)
    expect(wrapper.find('div[aria-hidden="true"]').exists()).toBe(true)

    // Act: visible を true に変更
    await wrapper.setProps({ visible: true })

    // Assert: 表示状態に変更
    expect(wrapper.find('div[role="alert"]').exists()).toBe(true)
    expect(wrapper.find('div[aria-hidden="true"]').exists()).toBe(false)
  })

  test('正常系_メッセージ変更_メッセージ内容の動的変更', async () => {
    // Arrange
    const wrapper = mount(ErrorMessage, {
      props: {
        message: '初期メッセージ',
        visible: true
      }
    })

    // 初期状態確認
    expect(wrapper.find('div[role="alert"]').text()).toBe('初期メッセージ')

    // Act: メッセージを変更
    await wrapper.setProps({ message: '変更後メッセージ' })

    // Assert: メッセージが更新される
    expect(wrapper.find('div[role="alert"]').text()).toBe('変更後メッセージ')
  })

  test('正常系_スタイリング_CSSクラスが正しく適用される', () => {
    // Arrange & Act
    const wrapper = mount(ErrorMessage, {
      props: {
        message: 'スタイルテスト',
        visible: true
      }
    })

    // Assert
    const errorDiv = wrapper.find('div[role="alert"]')
    expect(errorDiv.classes()).toContain('text-red-500')
    expect(errorDiv.classes()).toContain('text-sm')
    expect(errorDiv.classes()).toContain('mt-2')
    expect(errorDiv.classes()).toContain('transition-opacity')
  })
})
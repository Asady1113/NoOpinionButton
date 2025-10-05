import { describe, test, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import SubmitButton from '~/components/atoms/SubmitButton.vue'

describe('SubmitButton', () => {
  test('正常系_レンダリング_デフォルトプロパティで正常表示', () => {
    // Arrange & Act
    const wrapper = mount(SubmitButton, {
      props: {
        text: '決定'
      }
    })

    // Assert
    const button = wrapper.find('button')
    expect(button.exists()).toBe(true)
    expect(button.text()).toBe('決定')
    expect(button.element.disabled).toBe(false)
    expect(button.element.getAttribute('aria-label')).toBe('決定')
  })

  test('正常系_クリックイベント_クリック時にイベントが発火される', async () => {
    // Arrange
    const wrapper = mount(SubmitButton, {
      props: {
        text: '決定'
      }
    })

    // Act
    const button = wrapper.find('button')
    await button.trigger('click')

    // Assert
    const emittedEvents = wrapper.emitted('click')
    expect(emittedEvents).toBeTruthy()
    expect(emittedEvents!.length).toBe(1)
  })

  test('正常系_無効状態_disabledプロパティでボタン無効化', () => {
    // Arrange & Act
    const wrapper = mount(SubmitButton, {
      props: {
        text: '決定',
        disabled: true
      }
    })

    // Assert
    const button = wrapper.find('button')
    expect(button.element.disabled).toBe(true)
    expect(button.classes()).toContain('disabled:opacity-50')
    expect(button.classes()).toContain('disabled:cursor-not-allowed')
  })

  test('正常系_ローディング状態_loadingプロパティでローディング表示', () => {
    // Arrange & Act
    const wrapper = mount(SubmitButton, {
      props: {
        text: '決定',
        loading: true
      }
    })

    // Assert
    const button = wrapper.find('button')
    expect(button.element.disabled).toBe(true) // loading時は自動的にdisabled
    
    // ローディングスピナーの存在確認
    const spinner = wrapper.find('svg.animate-spin')
    expect(spinner.exists()).toBe(true)
    expect(spinner.classes()).toContain('animate-spin')
    
    // テキストが表示されることを確認
    expect(button.text()).toBe('決定')
  })

  test('正常系_ローディング状態_スピナーとテキストが同時表示される', () => {
    // Arrange & Act
    const wrapper = mount(SubmitButton, {
      props: {
        text: '送信中',
        loading: true
      }
    })

    // Assert
    const button = wrapper.find('button')
    const spinnerContainer = wrapper.find('div')
    
    expect(spinnerContainer.exists()).toBe(true)
    expect(button.text()).toBe('送信中')
    
    // スピナーのSVG要素が存在することを確認
    const spinner = wrapper.find('svg')
    expect(spinner.exists()).toBe(true)
    expect(spinner.attributes('aria-hidden')).toBe('true')
  })

  test('正常系_非ローディング状態_通常テキストのみ表示', () => {
    // Arrange & Act
    const wrapper = mount(SubmitButton, {
      props: {
        text: '決定',
        loading: false
      }
    })

    // Assert
    const button = wrapper.find('button')
    const spinner = wrapper.find('svg.animate-spin')
    
    expect(spinner.exists()).toBe(false)
    expect(button.text()).toBe('決定')
  })

  test('正常系_複合状態_disabledとloadingが同時に設定された場合', () => {
    // Arrange & Act
    const wrapper = mount(SubmitButton, {
      props: {
        text: '決定',
        disabled: true,
        loading: true
      }
    })

    // Assert
    const button = wrapper.find('button')
    expect(button.element.disabled).toBe(true)
    
    // ローディングスピナーも表示される
    const spinner = wrapper.find('svg.animate-spin')
    expect(spinner.exists()).toBe(true)
  })

  test('正常系_アクセシビリティ_aria-labelが正しく設定される', () => {
    // Arrange & Act
    const wrapper = mount(SubmitButton, {
      props: {
        text: 'カスタムボタン'
      }
    })

    // Assert
    const button = wrapper.find('button')
    expect(button.element.getAttribute('aria-label')).toBe('カスタムボタン')
  })

  test('正常系_スタイリング_基本CSSクラスが適用される', () => {
    // Arrange & Act
    const wrapper = mount(SubmitButton, {
      props: {
        text: '決定'
      }
    })

    // Assert
    const button = wrapper.find('button')
    expect(button.classes()).toContain('px-6')
    expect(button.classes()).toContain('py-2')
    expect(button.classes()).toContain('bg-indigo-500')
    expect(button.classes()).toContain('text-white')
    expect(button.classes()).toContain('rounded-full')
    expect(button.classes()).toContain('flex')
    expect(button.classes()).toContain('items-center')
    expect(button.classes()).toContain('justify-center')
    expect(button.classes()).toContain('min-w-[120px]')
    expect(button.classes()).toContain('transition-opacity')
  })

  test('異常系_無効状態でのクリック_クリックイベントが発火されない', async () => {
    // Arrange
    const wrapper = mount(SubmitButton, {
      props: {
        text: '決定',
        disabled: true
      }
    })

    // Act
    const button = wrapper.find('button')
    await button.trigger('click')

    // Assert
    // disabledなボタンはクリックイベントが発火されない（ブラウザの標準動作）
    const emittedEvents = wrapper.emitted('click')
    expect(emittedEvents).toBeFalsy()
  })

  test('異常系_ローディング状態でのクリック_クリックイベントが発火されない', async () => {
    // Arrange
    const wrapper = mount(SubmitButton, {
      props: {
        text: '決定',
        loading: true
      }
    })

    // Act
    const button = wrapper.find('button')
    await button.trigger('click')

    // Assert
    // loadingでdisabledになったボタンはクリックイベントが発火されない
    const emittedEvents = wrapper.emitted('click')
    expect(emittedEvents).toBeFalsy()
  })

  test('正常系_長いテキスト_長いテキストでも正常表示', () => {
    // Arrange & Act
    const longText = 'とても長いボタンテキストですが正常に表示されるはずです'
    const wrapper = mount(SubmitButton, {
      props: {
        text: longText
      }
    })

    // Assert
    const button = wrapper.find('button')
    expect(button.text()).toBe(longText)
    expect(button.element.getAttribute('aria-label')).toBe(longText)
  })
})
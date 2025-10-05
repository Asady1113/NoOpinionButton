import { describe, test, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import TextInput from '~/components/atoms/TextInput.vue'

describe('TextInput', () => {
  test('正常系_レンダリング_デフォルトプロパティで正常表示', () => {
    // Arrange & Act
    const wrapper = mount(TextInput, {
      props: {
        modelValue: ''
      }
    })

    // Assert
    const input = wrapper.find('input')
    expect(input.exists()).toBe(true)
    expect(input.element.value).toBe('')
    expect(input.element.placeholder).toBe('')
    expect(input.element.disabled).toBe(false)
    expect(input.element.maxLength).toBe(-1) // デフォルトは制限なし
  })

  test('正常系_プロパティ_全プロパティが正しく適用される', () => {
    // Arrange & Act
    const wrapper = mount(TextInput, {
      props: {
        modelValue: 'テスト値',
        placeholder: 'お名前を入力してください',
        maxLength: 50,
        disabled: true
      }
    })

    // Assert
    const input = wrapper.find('input')
    expect(input.element.value).toBe('テスト値')
    expect(input.element.placeholder).toBe('お名前を入力してください')
    expect(input.element.maxLength).toBe(50)
    expect(input.element.disabled).toBe(true)
  })

  test('正常系_v-model_入力値変更時にイベントが発火される', async () => {
    // Arrange
    const wrapper = mount(TextInput, {
      props: {
        modelValue: ''
      }
    })

    // Act
    const input = wrapper.find('input')
    await input.setValue('新しい値')

    // Assert
    const emittedEvents = wrapper.emitted('update:modelValue')
    expect(emittedEvents).toBeTruthy()
    expect(emittedEvents![0]).toEqual(['新しい値'])
  })

  test('正常系_日本語入力_日本語文字が正しく処理される', async () => {
    // Arrange
    const wrapper = mount(TextInput, {
      props: {
        modelValue: ''
      }
    })

    // Act
    const input = wrapper.find('input')
    await input.setValue('田中太郎')

    // Assert
    const emittedEvents = wrapper.emitted('update:modelValue')
    expect(emittedEvents![0]).toEqual(['田中太郎'])
  })

  test('正常系_絵文字入力_絵文字が正しく処理される', async () => {
    // Arrange
    const wrapper = mount(TextInput, {
      props: {
        modelValue: ''
      }
    })

    // Act
    const input = wrapper.find('input')
    await input.setValue('テスト😊👍')

    // Assert
    const emittedEvents = wrapper.emitted('update:modelValue')
    expect(emittedEvents![0]).toEqual(['テスト😊👍'])
  })

  test('正常系_無効状態_disabledプロパティでインタラクション無効化', async () => {
    // Arrange
    const wrapper = mount(TextInput, {
      props: {
        modelValue: '',
        disabled: true
      }
    })

    // Act
    const input = wrapper.find('input')
    await input.trigger('input')

    // Assert
    expect(input.element.disabled).toBe(true)
    expect(input.classes()).toContain('disabled:opacity-50')
    expect(input.classes()).toContain('disabled:cursor-not-allowed')
  })

  test('正常系_フォーカス状態_フォーカス時のスタイリング確認', () => {
    // Arrange & Act
    const wrapper = mount(TextInput, {
      props: {
        modelValue: ''
      }
    })

    // Assert
    const input = wrapper.find('input')
    expect(input.classes()).toContain('focus:outline-none')
    expect(input.classes()).toContain('focus:ring-2')
    expect(input.classes()).toContain('focus:ring-indigo-500')
    expect(input.classes()).toContain('focus:border-transparent')
  })

  test('正常系_スタイリング_基本CSSクラスが適用される', () => {
    // Arrange & Act
    const wrapper = mount(TextInput, {
      props: {
        modelValue: ''
      }
    })

    // Assert
    const input = wrapper.find('input')
    expect(input.classes()).toContain('border')
    expect(input.classes()).toContain('px-4')
    expect(input.classes()).toContain('py-1')
    expect(input.classes()).toContain('rounded-lg')
    expect(input.classes()).toContain('w-full')
  })

  test('正常系_maxLength_文字数制限が適用される', () => {
    // Arrange & Act
    const wrapper = mount(TextInput, {
      props: {
        modelValue: '',
        maxLength: 10
      }
    })

    // Assert
    const input = wrapper.find('input')
    expect(input.element.maxLength).toBe(10)
  })

  test('正常系_プレースホルダー_プレースホルダーテキストが表示される', () => {
    // Arrange & Act
    const wrapper = mount(TextInput, {
      props: {
        modelValue: '',
        placeholder: 'テストプレースホルダー'
      }
    })

    // Assert
    const input = wrapper.find('input')
    expect(input.element.placeholder).toBe('テストプレースホルダー')
  })

  test('正常系_空文字入力_空文字でもイベントが発火される', async () => {
    // Arrange
    const wrapper = mount(TextInput, {
      props: {
        modelValue: 'initial'
      }
    })

    // Act
    const input = wrapper.find('input')
    await input.setValue('')

    // Assert
    const emittedEvents = wrapper.emitted('update:modelValue')
    expect(emittedEvents![0]).toEqual([''])
  })
})
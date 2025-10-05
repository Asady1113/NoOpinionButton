import { describe, test, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import Title from '~/components/atoms/Title.vue'

describe('Title', () => {
  test('正常系_デフォルトレンダリング_h2タグで正常表示', () => {
    // Arrange & Act
    const wrapper = mount(Title, {
      props: {
        text: 'テストタイトル'
      }
    })

    // Assert
    const title = wrapper.find('h2')
    expect(title.exists()).toBe(true)
    expect(title.text()).toBe('テストタイトル')
    expect(title.classes()).toContain('font-bold')
    expect(title.classes()).toContain('text-center')
    expect(title.classes()).toContain('text-xl')
    expect(title.classes()).toContain('md:text-2xl')
    expect(title.classes()).toContain('mb-4')
  })

  test('正常系_h1レベル_h1タグで正常表示', () => {
    // Arrange & Act
    const wrapper = mount(Title, {
      props: {
        text: 'メインタイトル',
        level: 'h1'
      }
    })

    // Assert
    const title = wrapper.find('h1')
    expect(title.exists()).toBe(true)
    expect(title.text()).toBe('メインタイトル')
    expect(title.classes()).toContain('font-bold')
    expect(title.classes()).toContain('text-center')
    expect(title.classes()).toContain('text-2xl')
    expect(title.classes()).toContain('md:text-3xl')
    expect(title.classes()).toContain('mb-6')
  })

  test('正常系_h2レベル_h2タグで正常表示', () => {
    // Arrange & Act
    const wrapper = mount(Title, {
      props: {
        text: 'サブタイトル',
        level: 'h2'
      }
    })

    // Assert
    const title = wrapper.find('h2')
    expect(title.exists()).toBe(true)
    expect(title.text()).toBe('サブタイトル')
    expect(title.classes()).toContain('font-bold')
    expect(title.classes()).toContain('text-center')
    expect(title.classes()).toContain('text-xl')
    expect(title.classes()).toContain('md:text-2xl')
    expect(title.classes()).toContain('mb-4')
  })

  test('正常系_h3レベル_h3タグで正常表示', () => {
    // Arrange & Act
    const wrapper = mount(Title, {
      props: {
        text: '小見出し',
        level: 'h3'
      }
    })

    // Assert
    const title = wrapper.find('h3')
    expect(title.exists()).toBe(true)
    expect(title.text()).toBe('小見出し')
    expect(title.classes()).toContain('font-bold')
    expect(title.classes()).toContain('text-center')
    expect(title.classes()).toContain('text-lg')
    expect(title.classes()).toContain('md:text-xl')
    expect(title.classes()).toContain('mb-3')
  })

  test('正常系_日本語テキスト_日本語タイトルが正常表示', () => {
    // Arrange & Act
    const wrapper = mount(Title, {
      props: {
        text: '名前を入力してください'
      }
    })

    // Assert
    const title = wrapper.find('h2')
    expect(title.text()).toBe('名前を入力してください')
  })

  test('正常系_長いテキスト_長いタイトルでも正常表示', () => {
    // Arrange & Act
    const longText = 'これは非常に長いタイトルテキストですが、正常に表示されるはずです'
    const wrapper = mount(Title, {
      props: {
        text: longText
      }
    })

    // Assert
    const title = wrapper.find('h2')
    expect(title.text()).toBe(longText)
  })

  test('正常系_絵文字テキスト_絵文字を含むタイトルが正常表示', () => {
    // Arrange & Act
    const wrapper = mount(Title, {
      props: {
        text: 'ようこそ！😊🎉'
      }
    })

    // Assert
    const title = wrapper.find('h2')
    expect(title.text()).toBe('ようこそ！😊🎉')
  })

  test('正常系_空文字テキスト_空文字でも正常レンダリング', () => {
    // Arrange & Act
    const wrapper = mount(Title, {
      props: {
        text: ''
      }
    })

    // Assert
    const title = wrapper.find('h2')
    expect(title.exists()).toBe(true)
    expect(title.text()).toBe('')
  })

  test('正常系_レスポンシブクラス_各レベルでレスポンシブクラスが適用される', () => {
    // Arrange & Act - h1
    const h1Wrapper = mount(Title, {
      props: {
        text: 'H1タイトル',
        level: 'h1'
      }
    })

    // Assert - h1
    const h1Title = h1Wrapper.find('h1')
    expect(h1Title.classes()).toContain('text-2xl')
    expect(h1Title.classes()).toContain('md:text-3xl')

    // Arrange & Act - h2
    const h2Wrapper = mount(Title, {
      props: {
        text: 'H2タイトル',
        level: 'h2'
      }
    })

    // Assert - h2
    const h2Title = h2Wrapper.find('h2')
    expect(h2Title.classes()).toContain('text-xl')
    expect(h2Title.classes()).toContain('md:text-2xl')

    // Arrange & Act - h3
    const h3Wrapper = mount(Title, {
      props: {
        text: 'H3タイトル',
        level: 'h3'
      }
    })

    // Assert - h3
    const h3Title = h3Wrapper.find('h3')
    expect(h3Title.classes()).toContain('text-lg')
    expect(h3Title.classes()).toContain('md:text-xl')
  })

  test('正常系_マージンクラス_各レベルで適切なマージンが適用される', () => {
    // Arrange & Act - h1
    const h1Wrapper = mount(Title, {
      props: {
        text: 'H1タイトル',
        level: 'h1'
      }
    })

    // Assert - h1
    expect(h1Wrapper.find('h1').classes()).toContain('mb-6')

    // Arrange & Act - h2
    const h2Wrapper = mount(Title, {
      props: {
        text: 'H2タイトル',
        level: 'h2'
      }
    })

    // Assert - h2
    expect(h2Wrapper.find('h2').classes()).toContain('mb-4')

    // Arrange & Act - h3
    const h3Wrapper = mount(Title, {
      props: {
        text: 'H3タイトル',
        level: 'h3'
      }
    })

    // Assert - h3
    expect(h3Wrapper.find('h3').classes()).toContain('mb-3')
  })

  test('正常系_基本スタイリング_共通CSSクラスが適用される', () => {
    // Arrange & Act
    const wrapper = mount(Title, {
      props: {
        text: 'スタイルテスト'
      }
    })

    // Assert
    const title = wrapper.find('h2')
    expect(title.classes()).toContain('font-bold')
    expect(title.classes()).toContain('text-center')
  })

  test('正常系_動的レベル変更_levelプロパティの動的変更', async () => {
    // Arrange
    const wrapper = mount(Title, {
      props: {
        text: 'テストタイトル',
        level: 'h2'
      }
    })

    // 初期状態確認
    expect(wrapper.find('h2').exists()).toBe(true)
    expect(wrapper.find('h1').exists()).toBe(false)

    // Act: level を h1 に変更
    await wrapper.setProps({ level: 'h1' })

    // Assert: h1 タグに変更される
    expect(wrapper.find('h1').exists()).toBe(true)
    expect(wrapper.find('h2').exists()).toBe(false)
    expect(wrapper.find('h1').text()).toBe('テストタイトル')
  })

  test('正常系_アクセシビリティ_セマンティックHTMLが使用される', () => {
    // Arrange & Act
    const wrapper = mount(Title, {
      props: {
        text: 'アクセシビリティテスト',
        level: 'h1'
      }
    })

    // Assert
    const title = wrapper.find('h1')
    expect(title.element.tagName).toBe('H1')
    // セマンティックなHTMLタグが使用されていることを確認
    expect(title.exists()).toBe(true)
  })
})
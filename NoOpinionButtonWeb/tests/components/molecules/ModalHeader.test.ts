import { describe, test, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import ModalHeader from '~/components/molecules/ModalHeader.vue'

// Mock the Title component
vi.mock('~/components/atoms/Title.vue', () => ({
  default: {
    name: 'Title',
    props: ['text', 'level'],
    template: `<component :is="level || 'h2'" data-testid="title">{{ text }}</component>`
  }
}))

describe('ModalHeader', () => {
  test('正常系_レンダリング_デフォルトプロパティで正常表示', () => {
    // Arrange & Act
    const wrapper = mount(ModalHeader, {
      props: {
        title: 'テストタイトル'
      }
    })

    // Assert
    expect(wrapper.find('header').exists()).toBe(true)
    expect(wrapper.find('[data-testid="title"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="title"]').text()).toBe('テストタイトル')
    expect(wrapper.find('h2').exists()).toBe(true) // デフォルトレベル
  })

  test('正常系_カスタムレベル_h1レベルで正常表示', () => {
    // Arrange & Act
    const wrapper = mount(ModalHeader, {
      props: {
        title: 'メインタイトル',
        level: 'h1'
      }
    })

    // Assert
    expect(wrapper.find('header').exists()).toBe(true)
    expect(wrapper.find('[data-testid="title"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="title"]').text()).toBe('メインタイトル')
    expect(wrapper.find('h1').exists()).toBe(true)
  })

  test('正常系_カスタムレベル_h2レベルで正常表示', () => {
    // Arrange & Act
    const wrapper = mount(ModalHeader, {
      props: {
        title: 'サブタイトル',
        level: 'h2'
      }
    })

    // Assert
    expect(wrapper.find('header').exists()).toBe(true)
    expect(wrapper.find('[data-testid="title"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="title"]').text()).toBe('サブタイトル')
    expect(wrapper.find('h2').exists()).toBe(true)
  })

  test('正常系_カスタムレベル_h3レベルで正常表示', () => {
    // Arrange & Act
    const wrapper = mount(ModalHeader, {
      props: {
        title: '小見出し',
        level: 'h3'
      }
    })

    // Assert
    expect(wrapper.find('header').exists()).toBe(true)
    expect(wrapper.find('[data-testid="title"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="title"]').text()).toBe('小見出し')
    expect(wrapper.find('h3').exists()).toBe(true)
  })

  test('正常系_日本語タイトル_日本語タイトルが正常表示', () => {
    // Arrange & Act
    const wrapper = mount(ModalHeader, {
      props: {
        title: '名前を入力してください'
      }
    })

    // Assert
    expect(wrapper.find('[data-testid="title"]').text()).toBe('名前を入力してください')
  })

  test('正常系_長いタイトル_長いタイトルでも正常表示', () => {
    // Arrange & Act
    const longTitle = 'これは非常に長いタイトルテキストですが、正常に表示されるはずです'
    const wrapper = mount(ModalHeader, {
      props: {
        title: longTitle
      }
    })

    // Assert
    expect(wrapper.find('[data-testid="title"]').text()).toBe(longTitle)
  })

  test('正常系_絵文字タイトル_絵文字を含むタイトルが正常表示', () => {
    // Arrange & Act
    const wrapper = mount(ModalHeader, {
      props: {
        title: 'ようこそ！😊🎉'
      }
    })

    // Assert
    expect(wrapper.find('[data-testid="title"]').text()).toBe('ようこそ！😊🎉')
  })

  test('正常系_空文字タイトル_空文字でも正常レンダリング', () => {
    // Arrange & Act
    const wrapper = mount(ModalHeader, {
      props: {
        title: ''
      }
    })

    // Assert
    expect(wrapper.find('header').exists()).toBe(true)
    expect(wrapper.find('[data-testid="title"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="title"]').text()).toBe('')
  })

  test('正常系_スタイリング_基本CSSクラスが適用される', () => {
    // Arrange & Act
    const wrapper = mount(ModalHeader, {
      props: {
        title: 'スタイルテスト'
      }
    })

    // Assert
    const header = wrapper.find('header')
    expect(header.classes()).toContain('text-center')
    expect(header.classes()).toContain('mb-6')
  })

  test('正常系_セマンティックHTML_headerタグが使用される', () => {
    // Arrange & Act
    const wrapper = mount(ModalHeader, {
      props: {
        title: 'セマンティックテスト'
      }
    })

    // Assert
    const header = wrapper.find('header')
    expect(header.element.tagName).toBe('HEADER')
    expect(header.exists()).toBe(true)
  })

  test('正常系_プロパティ変更_titleプロパティの動的変更', async () => {
    // Arrange
    const wrapper = mount(ModalHeader, {
      props: {
        title: '初期タイトル'
      }
    })

    // 初期状態確認
    expect(wrapper.find('[data-testid="title"]').text()).toBe('初期タイトル')

    // Act: title を変更
    await wrapper.setProps({ title: '変更後タイトル' })

    // Assert: タイトルが更新される
    expect(wrapper.find('[data-testid="title"]').text()).toBe('変更後タイトル')
  })

  test('正常系_プロパティ変更_levelプロパティの動的変更', async () => {
    // Arrange
    const wrapper = mount(ModalHeader, {
      props: {
        title: 'テストタイトル',
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
    expect(wrapper.find('[data-testid="title"]').text()).toBe('テストタイトル')
  })

  test('正常系_アクセシビリティ_適切なHTML構造', () => {
    // Arrange & Act
    const wrapper = mount(ModalHeader, {
      props: {
        title: 'アクセシビリティテスト',
        level: 'h1'
      }
    })

    // Assert
    // セマンティックなheaderタグが使用されている
    expect(wrapper.find('header').exists()).toBe(true)
    
    // 見出しタグが適切に使用されている
    expect(wrapper.find('h1').exists()).toBe(true)
    
    // タイトルが正しく表示されている
    expect(wrapper.find('[data-testid="title"]').text()).toBe('アクセシビリティテスト')
  })

  test('正常系_レスポンシブ対応_モバイル表示での確認', () => {
    // Arrange & Act
    const wrapper = mount(ModalHeader, {
      props: {
        title: 'レスポンシブテスト'
      }
    })

    // Assert
    // 基本的なレスポンシブクラスが適用されていることを確認
    const header = wrapper.find('header')
    expect(header.classes()).toContain('text-center')
    expect(header.classes()).toContain('mb-6')
    
    // Titleコンポーネントが正しく配置されている
    expect(wrapper.find('[data-testid="title"]').exists()).toBe(true)
  })
})
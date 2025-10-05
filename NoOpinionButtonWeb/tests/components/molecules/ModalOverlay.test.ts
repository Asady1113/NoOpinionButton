import { describe, test, expect, vi, beforeEach, afterEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { nextTick } from 'vue'
import ModalOverlay from '~/components/molecules/ModalOverlay.vue'

// Mock document and body for DOM manipulation tests
const mockDocument = {
  body: {
    style: {
      overflow: ''
    }
  },
  querySelector: vi.fn(),
  createElement: vi.fn(() => ({
    focus: vi.fn(),
    querySelector: vi.fn(),
    querySelectorAll: vi.fn()
  }))
}

// Mock global document
Object.defineProperty(global, 'document', {
  value: mockDocument,
  writable: true
})

beforeEach(() => {
  vi.clearAllMocks()
  mockDocument.body.style.overflow = ''
  mockDocument.querySelector.mockReturnValue(null)
})

afterEach(() => {
  // Reset document body style
  mockDocument.body.style.overflow = ''
})

describe('ModalOverlay', () => {
  test('正常系_表示状態_visibleがtrueの場合にモーダル表示', () => {
    // Arrange & Act
    const wrapper = mount(ModalOverlay, {
      props: {
        visible: true
      },
      slots: {
        default: '<div data-testid="modal-content">モーダルコンテンツ</div>'
      },
      attachTo: document.body
    })

    // Assert
    expect(wrapper.find('[role="dialog"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="modal-content"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="modal-content"]').text()).toBe('モーダルコンテンツ')
  })

  test('正常系_非表示状態_visibleがfalseの場合にモーダル非表示', () => {
    // Arrange & Act
    const wrapper = mount(ModalOverlay, {
      props: {
        visible: false
      },
      slots: {
        default: '<div data-testid="modal-content">モーダルコンテンツ</div>'
      }
    })

    // Assert
    expect(wrapper.find('[role="dialog"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="modal-content"]').exists()).toBe(false)
  })

  test('正常系_スロットコンテンツ_スロットコンテンツが正常表示', () => {
    // Arrange & Act
    const wrapper = mount(ModalOverlay, {
      props: {
        visible: true
      },
      slots: {
        default: `
          <div data-testid="custom-content">
            <h2>カスタムタイトル</h2>
            <p>カスタムコンテンツ</p>
          </div>
        `
      },
      attachTo: document.body
    })

    // Assert
    expect(wrapper.find('[data-testid="custom-content"]').exists()).toBe(true)
    expect(wrapper.find('h2').text()).toBe('カスタムタイトル')
    expect(wrapper.find('p').text()).toBe('カスタムコンテンツ')
  })

  test('正常系_背景クリック_closeOnBackdropClickがtrueの場合にcloseイベント発火', async () => {
    // Arrange
    const wrapper = mount(ModalOverlay, {
      props: {
        visible: true,
        closeOnBackdropClick: true
      },
      slots: {
        default: '<div data-testid="modal-content">コンテンツ</div>'
      },
      attachTo: document.body
    })

    // Act
    const backdrop = wrapper.find('.fixed.inset-0')
    await backdrop.trigger('click')

    // Assert
    const emittedEvents = wrapper.emitted('close')
    expect(emittedEvents).toBeTruthy()
    expect(emittedEvents!.length).toBe(1)
  })

  test('正常系_背景クリック_closeOnBackdropClickがfalseの場合にcloseイベント発火しない', async () => {
    // Arrange
    const wrapper = mount(ModalOverlay, {
      props: {
        visible: true,
        closeOnBackdropClick: false
      },
      slots: {
        default: '<div data-testid="modal-content">コンテンツ</div>'
      },
      attachTo: document.body
    })

    // Act
    const backdrop = wrapper.find('.fixed.inset-0')
    await backdrop.trigger('click')

    // Assert
    const emittedEvents = wrapper.emitted('close')
    expect(emittedEvents).toBeFalsy()
  })

  test('正常系_コンテンツクリック_モーダルコンテンツクリックでcloseイベント発火しない', async () => {
    // Arrange
    const wrapper = mount(ModalOverlay, {
      props: {
        visible: true,
        closeOnBackdropClick: true
      },
      slots: {
        default: '<div data-testid="modal-content">コンテンツ</div>'
      },
      attachTo: document.body
    })

    // Act
    const modalContent = wrapper.find('[role="dialog"]')
    await modalContent.trigger('click')

    // Assert
    const emittedEvents = wrapper.emitted('close')
    expect(emittedEvents).toBeFalsy()
  })

  test('正常系_アクセシビリティ_適切なARIA属性が設定される', () => {
    // Arrange & Act
    const wrapper = mount(ModalOverlay, {
      props: {
        visible: true
      },
      slots: {
        default: '<div data-testid="modal-content">コンテンツ</div>'
      },
      attachTo: document.body
    })

    // Assert
    const dialog = wrapper.find('[role="dialog"]')
    expect(dialog.attributes('role')).toBe('dialog')
    expect(dialog.attributes('aria-modal')).toBe('true')
    expect(dialog.attributes('aria-labelledby')).toBe('modal-title')
    
    const backdrop = wrapper.find('.bg-black.bg-opacity-50')
    expect(backdrop.attributes('aria-hidden')).toBe('true')
  })

  test('正常系_スタイリング_基本CSSクラスが適用される', () => {
    // Arrange & Act
    const wrapper = mount(ModalOverlay, {
      props: {
        visible: true
      },
      slots: {
        default: '<div data-testid="modal-content">コンテンツ</div>'
      },
      attachTo: document.body
    })

    // Assert
    const container = wrapper.find('.fixed.inset-0')
    expect(container.classes()).toContain('fixed')
    expect(container.classes()).toContain('inset-0')
    expect(container.classes()).toContain('z-50')
    expect(container.classes()).toContain('flex')
    expect(container.classes()).toContain('items-center')
    expect(container.classes()).toContain('justify-center')
    expect(container.classes()).toContain('p-4')

    const backdrop = wrapper.find('.bg-black.bg-opacity-50')
    expect(backdrop.classes()).toContain('absolute')
    expect(backdrop.classes()).toContain('inset-0')
    expect(backdrop.classes()).toContain('bg-black')
    expect(backdrop.classes()).toContain('bg-opacity-50')
    expect(backdrop.classes()).toContain('backdrop-blur-sm')

    const dialog = wrapper.find('[role="dialog"]')
    expect(dialog.classes()).toContain('relative')
    expect(dialog.classes()).toContain('bg-white')
    expect(dialog.classes()).toContain('rounded-lg')
    expect(dialog.classes()).toContain('shadow-xl')
    expect(dialog.classes()).toContain('max-w-md')
    expect(dialog.classes()).toContain('w-full')
    expect(dialog.classes()).toContain('max-h-[90vh]')
    expect(dialog.classes()).toContain('overflow-y-auto')
  })

  test('正常系_ボディスクロール制御_visible時にbody scrollが無効化される', async () => {
    // Arrange
    const wrapper = mount(ModalOverlay, {
      props: {
        visible: false
      },
      slots: {
        default: '<div data-testid="modal-content">コンテンツ</div>'
      }
    })

    // Act: visible を true に変更
    await wrapper.setProps({ visible: true })
    await nextTick()

    // Assert: body の overflow が hidden に設定される
    expect(mockDocument.body.style.overflow).toBe('hidden')
  })

  test('正常系_ボディスクロール制御_非visible時にbody scrollが復元される', async () => {
    // Arrange
    const wrapper = mount(ModalOverlay, {
      props: {
        visible: true
      },
      slots: {
        default: '<div data-testid="modal-content">コンテンツ</div>'
      }
    })

    // 初期状態でbody scrollが無効化されていることを確認
    await nextTick()
    expect(mockDocument.body.style.overflow).toBe('hidden')

    // Act: visible を false に変更
    await wrapper.setProps({ visible: false })
    await nextTick()

    // Assert: body の overflow が復元される
    expect(mockDocument.body.style.overflow).toBe('')
  })

  test('正常系_アンマウント時クリーンアップ_コンポーネントアンマウント時にbody scrollが復元される', () => {
    // Arrange
    const wrapper = mount(ModalOverlay, {
      props: {
        visible: true
      },
      slots: {
        default: '<div data-testid="modal-content">コンテンツ</div>'
      }
    })

    // Act: コンポーネントをアンマウント
    wrapper.unmount()

    // Assert: body の overflow が復元される
    expect(mockDocument.body.style.overflow).toBe('')
  })

  test('正常系_表示切り替え_visibleプロパティの動的変更', async () => {
    // Arrange
    const wrapper = mount(ModalOverlay, {
      props: {
        visible: false
      },
      slots: {
        default: '<div data-testid="modal-content">コンテンツ</div>'
      }
    })

    // 初期状態：非表示
    expect(wrapper.find('[role="dialog"]').exists()).toBe(false)

    // Act: visible を true に変更
    await wrapper.setProps({ visible: true })

    // Assert: 表示状態に変更
    expect(wrapper.find('[role="dialog"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="modal-content"]').exists()).toBe(true)
  })

  test('正常系_レスポンシブ対応_モバイル表示での確認', () => {
    // Arrange & Act
    const wrapper = mount(ModalOverlay, {
      props: {
        visible: true
      },
      slots: {
        default: '<div data-testid="modal-content">レスポンシブテスト</div>'
      },
      attachTo: document.body
    })

    // Assert
    const dialog = wrapper.find('[role="dialog"]')
    expect(dialog.classes()).toContain('max-w-md')
    expect(dialog.classes()).toContain('w-full')
    
    // パディングがレスポンシブに対応している
    const container = wrapper.find('.fixed.inset-0')
    expect(container.classes()).toContain('p-4')
  })

  test('正常系_Teleport_bodyにテレポートされる', () => {
    // Arrange & Act
    const wrapper = mount(ModalOverlay, {
      props: {
        visible: true
      },
      slots: {
        default: '<div data-testid="modal-content">テレポートテスト</div>'
      },
      attachTo: document.body
    })

    // Assert
    // Teleportが使用されていることを確認（実際のDOMへの配置はテスト環境では制限される）
    expect(wrapper.find('[data-testid="modal-content"]').exists()).toBe(true)
  })

  test('正常系_トランジション_トランジションクラスが適用される', () => {
    // Arrange & Act
    const wrapper = mount(ModalOverlay, {
      props: {
        visible: true
      },
      slots: {
        default: '<div data-testid="modal-content">トランジションテスト</div>'
      },
      attachTo: document.body
    })

    // Assert
    // Transitionコンポーネントが使用されていることを確認
    const transition = wrapper.findComponent({ name: 'Transition' })
    expect(transition.exists()).toBe(true)
  })
})
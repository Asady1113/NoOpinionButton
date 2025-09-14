import { describe, test, expect, vi, beforeEach, afterEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { nextTick } from 'vue'
import ParticipantNameModal from '~/components/organisms/ParticipantNameModal.vue'

// Mock child components
vi.mock('~/components/molecules/ModalOverlay.vue', () => ({
  default: {
    name: 'ModalOverlay',
    props: ['visible', 'closeOnBackdropClick'],
    emits: ['close'],
    template: `
      <div v-if="visible" data-testid="modal-overlay">
        <slot />
      </div>
    `
  }
}))

vi.mock('~/components/molecules/ModalHeader.vue', () => ({
  default: {
    name: 'ModalHeader',
    props: ['title', 'level'],
    template: `<header data-testid="modal-header">{{ title }}</header>`
  }
}))

vi.mock('~/components/molecules/NameInputForm.vue', () => ({
  default: {
    name: 'NameInputForm',
    props: ['participantId', 'placeholder', 'maxLength', 'buttonText'],
    emits: ['success', 'error'],
    template: `
      <form data-testid="name-input-form">
        <button type="button" @click="$emit('success')" data-testid="success-trigger">Success</button>
        <button type="button" @click="$emit('error', 'Test error')" data-testid="error-trigger">Error</button>
      </form>
    `
  }
}))

// Mock document and focus management
const mockDocument = {
  activeElement: null as HTMLElement | null,
  querySelector: vi.fn(),
  querySelectorAll: vi.fn(),
  createElement: vi.fn(() => ({
    focus: vi.fn(),
    querySelector: vi.fn(),
    querySelectorAll: vi.fn()
  }))
}

const mockElement = {
  focus: vi.fn(),
  querySelector: vi.fn(),
  querySelectorAll: vi.fn()
}

Object.defineProperty(global, 'document', {
  value: mockDocument,
  writable: true
})

beforeEach(() => {
  vi.clearAllMocks()
  mockDocument.activeElement = null
  mockElement.focus.mockClear()
  mockElement.querySelector.mockReturnValue(null)
  mockElement.querySelectorAll.mockReturnValue([])
})

afterEach(() => {
  mockDocument.activeElement = null
})

describe('ParticipantNameModal', () => {
  test('正常系_レンダリング_表示状態で正常レンダリング', () => {
    // Arrange & Act
    const wrapper = mount(ParticipantNameModal, {
      props: {
        isVisible: true,
        participantId: 'participant123'
      }
    })

    // Assert
    expect(wrapper.find('[data-testid="modal-overlay"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="modal-header"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="name-input-form"]').exists()).toBe(true)
    
    // ModalHeader のプロパティ確認
    const modalHeader = wrapper.findComponent({ name: 'ModalHeader' })
    expect(modalHeader.props('title')).toBe('ユーザー名を入力してください')
    expect(modalHeader.props('level')).toBe('h2')
    
    // NameInputForm のプロパティ確認
    const nameInputForm = wrapper.findComponent({ name: 'NameInputForm' })
    expect(nameInputForm.props('participantId')).toBe('participant123')
    expect(nameInputForm.props('placeholder')).toBe('ユーザー名を入力してください')
    expect(nameInputForm.props('maxLength')).toBe(50)
    expect(nameInputForm.props('buttonText')).toBe('決定')
  })

  test('正常系_非表示状態_非表示状態で正常レンダリング', () => {
    // Arrange & Act
    const wrapper = mount(ParticipantNameModal, {
      props: {
        isVisible: false,
        participantId: 'participant123'
      }
    })

    // Assert
    expect(wrapper.find('[data-testid="modal-overlay"]').exists()).toBe(false)
    
    // ModalOverlay のプロパティ確認
    const modalOverlay = wrapper.findComponent({ name: 'ModalOverlay' })
    expect(modalOverlay.props('visible')).toBe(false)
    expect(modalOverlay.props('closeOnBackdropClick')).toBe(false)
  })

  test('正常系_成功イベント_NameInputFormの成功イベントで適切なイベント発火', async () => {
    // Arrange
    const wrapper = mount(ParticipantNameModal, {
      props: {
        isVisible: true,
        participantId: 'participant123'
      }
    })

    // Act
    const successTrigger = wrapper.find('[data-testid="success-trigger"]')
    await successTrigger.trigger('click')

    // Assert
    const successEvents = wrapper.emitted('success')
    const closeEvents = wrapper.emitted('close')
    
    expect(successEvents).toBeTruthy()
    expect(successEvents!.length).toBe(1)
    expect(closeEvents).toBeTruthy()
    expect(closeEvents!.length).toBe(1)
  })

  test('正常系_エラーイベント_NameInputFormのエラーイベントで適切なイベント発火', async () => {
    // Arrange
    const wrapper = mount(ParticipantNameModal, {
      props: {
        isVisible: true,
        participantId: 'participant123'
      }
    })

    // Act
    const errorTrigger = wrapper.find('[data-testid="error-trigger"]')
    await errorTrigger.trigger('click')

    // Assert
    const errorEvents = wrapper.emitted('error')
    const closeEvents = wrapper.emitted('close')
    
    expect(errorEvents).toBeTruthy()
    expect(errorEvents!.length).toBe(1)
    expect(errorEvents![0]).toEqual(['Test error'])
    
    // エラー時はモーダルを閉じない
    expect(closeEvents).toBeFalsy()
  })

  test('正常系_アクセシビリティ_適切なARIA属性が設定される', () => {
    // Arrange & Act
    const wrapper = mount(ParticipantNameModal, {
      props: {
        isVisible: true,
        participantId: 'participant123'
      }
    })

    // Assert
    const modalContent = wrapper.find('[role="dialog"]')
    expect(modalContent.exists()).toBe(true)
    expect(modalContent.attributes('role')).toBe('dialog')
    expect(modalContent.attributes('aria-labelledby')).toBe('modal-title')
    expect(modalContent.attributes('aria-describedby')).toBe('modal-description')
    expect(modalContent.attributes('aria-modal')).toBe('true')
    expect(modalContent.attributes('tabindex')).toBe('-1')
    
    // スクリーンリーダー用の説明テキスト
    const description = wrapper.find('#modal-description')
    expect(description.exists()).toBe(true)
    expect(description.classes()).toContain('sr-only')
    expect(description.text()).toBe('ユーザー名を入力して決定ボタンを押してください')
    
    // タイトル要素
    const title = wrapper.find('#modal-title')
    expect(title.exists()).toBe(true)
  })

  test('正常系_キーボードナビゲーション_Escapeキーでモーダルが閉じない', async () => {
    // Arrange
    const wrapper = mount(ParticipantNameModal, {
      props: {
        isVisible: true,
        participantId: 'participant123'
      }
    })

    // Act
    const modalContent = wrapper.find('[role="dialog"]')
    await modalContent.trigger('keydown', { key: 'Escape' })

    // Assert
    const closeEvents = wrapper.emitted('close')
    expect(closeEvents).toBeFalsy()
  })

  test('正常系_フォーカストラップ_Tabキーでフォーカストラップが動作', async () => {
    // Arrange
    const mockFocusableElements = [
      { focus: vi.fn() },
      { focus: vi.fn() },
      { focus: vi.fn() }
    ]
    
    mockElement.querySelectorAll.mockReturnValue(mockFocusableElements as any)
    mockDocument.activeElement = mockFocusableElements[2] as any

    const wrapper = mount(ParticipantNameModal, {
      props: {
        isVisible: true,
        participantId: 'participant123'
      }
    })

    // モックの modalContent を設定
    const modalContentElement = wrapper.find('[role="dialog"]').element
    modalContentElement.querySelectorAll = mockElement.querySelectorAll

    // Act
    const modalContent = wrapper.find('[role="dialog"]')
    await modalContent.trigger('keydown', { key: 'Tab' })

    // Assert
    // 最後の要素から最初の要素にフォーカスが移動することを確認
    expect(mockFocusableElements[0].focus).toHaveBeenCalled()
  })

  test('正常系_フォーカストラップ_Shift+Tabキーで逆方向フォーカストラップが動作', async () => {
    // Arrange
    const mockFocusableElements = [
      { focus: vi.fn() },
      { focus: vi.fn() },
      { focus: vi.fn() }
    ]
    
    mockElement.querySelectorAll.mockReturnValue(mockFocusableElements as any)
    mockDocument.activeElement = mockFocusableElements[0] as any

    const wrapper = mount(ParticipantNameModal, {
      props: {
        isVisible: true,
        participantId: 'participant123'
      }
    })

    // モックの modalContent を設定
    const modalContentElement = wrapper.find('[role="dialog"]').element
    modalContentElement.querySelectorAll = mockElement.querySelectorAll

    // Act
    const modalContent = wrapper.find('[role="dialog"]')
    await modalContent.trigger('keydown', { key: 'Tab', shiftKey: true })

    // Assert
    // 最初の要素から最後の要素にフォーカスが移動することを確認
    expect(mockFocusableElements[2].focus).toHaveBeenCalled()
  })

  test('正常系_フォーカス管理_表示時に最初のフォーカス可能要素にフォーカス', async () => {
    // Arrange
    const mockFocusableElement = { focus: vi.fn() }
    const mockPreviousElement = { focus: vi.fn() }
    
    mockDocument.activeElement = mockPreviousElement as any
    mockElement.querySelector.mockReturnValue(mockFocusableElement as any)

    const wrapper = mount(ParticipantNameModal, {
      props: {
        isVisible: false,
        participantId: 'participant123'
      }
    })

    // モックの modalContent を設定
    const modalContentElement = wrapper.find('[role="dialog"]').element
    modalContentElement.querySelector = mockElement.querySelector

    // Act
    await wrapper.setProps({ isVisible: true })
    await nextTick()

    // Assert
    expect(mockFocusableElement.focus).toHaveBeenCalled()
  })

  test('正常系_フォーカス管理_非表示時に前のフォーカス要素に復元', async () => {
    // Arrange
    const mockPreviousElement = { focus: vi.fn() }
    const mockFocusableElement = { focus: vi.fn() }
    
    mockDocument.activeElement = mockPreviousElement as any
    mockElement.querySelector.mockReturnValue(mockFocusableElement as any)

    const wrapper = mount(ParticipantNameModal, {
      props: {
        isVisible: true,
        participantId: 'participant123'
      }
    })

    // モックの modalContent を設定
    const modalContentElement = wrapper.find('[role="dialog"]').element
    modalContentElement.querySelector = mockElement.querySelector

    // Act
    await wrapper.setProps({ isVisible: false })
    await nextTick()

    // Assert
    expect(mockPreviousElement.focus).toHaveBeenCalled()
  })

  test('正常系_プロパティ変更_participantIdの動的変更', async () => {
    // Arrange
    const wrapper = mount(ParticipantNameModal, {
      props: {
        isVisible: true,
        participantId: 'participant123'
      }
    })

    // 初期状態確認
    const nameInputForm = wrapper.findComponent({ name: 'NameInputForm' })
    expect(nameInputForm.props('participantId')).toBe('participant123')

    // Act
    await wrapper.setProps({ participantId: 'participant456' })

    // Assert
    expect(nameInputForm.props('participantId')).toBe('participant456')
  })

  test('正常系_表示切り替え_isVisibleプロパティの動的変更', async () => {
    // Arrange
    const wrapper = mount(ParticipantNameModal, {
      props: {
        isVisible: false,
        participantId: 'participant123'
      }
    })

    // 初期状態：非表示
    expect(wrapper.find('[data-testid="modal-overlay"]').exists()).toBe(false)

    // Act
    await wrapper.setProps({ isVisible: true })

    // Assert
    expect(wrapper.find('[data-testid="modal-overlay"]').exists()).toBe(true)
    
    const modalOverlay = wrapper.findComponent({ name: 'ModalOverlay' })
    expect(modalOverlay.props('visible')).toBe(true)
  })

  test('正常系_スタイリング_スクリーンリーダー専用クラスが適用される', () => {
    // Arrange & Act
    const wrapper = mount(ParticipantNameModal, {
      props: {
        isVisible: true,
        participantId: 'participant123'
      }
    })

    // Assert
    const srOnlyElement = wrapper.find('.sr-only')
    expect(srOnlyElement.exists()).toBe(true)
    expect(srOnlyElement.classes()).toContain('sr-only')
  })

  test('正常系_コンポーネント統合_子コンポーネントが正しく統合される', () => {
    // Arrange & Act
    const wrapper = mount(ParticipantNameModal, {
      props: {
        isVisible: true,
        participantId: 'participant123'
      }
    })

    // Assert
    // ModalOverlay の統合確認
    const modalOverlay = wrapper.findComponent({ name: 'ModalOverlay' })
    expect(modalOverlay.exists()).toBe(true)
    expect(modalOverlay.props('closeOnBackdropClick')).toBe(false)
    
    // ModalHeader の統合確認
    const modalHeader = wrapper.findComponent({ name: 'ModalHeader' })
    expect(modalHeader.exists()).toBe(true)
    
    // NameInputForm の統合確認
    const nameInputForm = wrapper.findComponent({ name: 'NameInputForm' })
    expect(nameInputForm.exists()).toBe(true)
  })

  test('正常系_アンマウント時クリーンアップ_フォーカスが適切に復元される', () => {
    // Arrange
    const mockPreviousElement = { focus: vi.fn() }
    mockDocument.activeElement = mockPreviousElement as any

    const wrapper = mount(ParticipantNameModal, {
      props: {
        isVisible: true,
        participantId: 'participant123'
      }
    })

    // Act
    wrapper.unmount()

    // Assert
    expect(mockPreviousElement.focus).toHaveBeenCalled()
  })
})
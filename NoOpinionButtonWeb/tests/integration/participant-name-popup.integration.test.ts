import { describe, test, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { nextTick } from 'vue'
import ParticipantPage from '~/pages/participant.vue'

// Mock the sign-in store
const mockSignInStore = {
  value: {
    id: '',
    meetingId: '',
    meetingName: '',
    isFacilitator: false
  }
}

vi.mock('~/composables/signIn/useSignIn', () => ({
  useSignInStore: () => mockSignInStore
}))

// Mock the ParticipantNameModal component
const mockModalEmit = vi.fn()
vi.mock('~/components/organisms/ParticipantNameModal.vue', () => ({
  default: {
    name: 'ParticipantNameModal',
    props: ['isVisible', 'participantId'],
    emits: ['close', 'success', 'error'],
    setup(props: any, { emit }: any) {
      mockModalEmit.mockImplementation((event: string, ...args: any[]) => {
        emit(event, ...args)
      })
      return { mockModalEmit, props }
    },
    template: `
      <div v-if="isVisible" data-testid="participant-name-modal">
        <div data-testid="participant-id">{{ participantId || '' }}</div>
        <button @click="mockModalEmit('close')" data-testid="close-button">Close</button>
        <button @click="mockModalEmit('success')" data-testid="success-button">Success</button>
        <button @click="mockModalEmit('error', 'Test error')" data-testid="error-button">Error</button>
      </div>
    `
  }
}))

beforeEach(() => {
  vi.clearAllMocks()
  mockModalEmit.mockClear()
  
  // Reset sign-in store to default state
  mockSignInStore.value = {
    id: '',
    meetingId: '',
    meetingName: '',
    isFacilitator: false
  }
})

function getMainContent(wrapper: any) {
  const divs = wrapper.findAll('div')
  return divs.length > 1 ? divs[1] : null
}

describe('Participant Page - Name Popup Integration', () => {
  test('正常系_ページロード_参加者IDがある場合にポップアップが表示される', async () => {
    // Arrange
    mockSignInStore.value.id = 'participant123'

    // Act
    const wrapper = mount(ParticipantPage)
    await nextTick()

    // Assert
    expect(wrapper.find('[data-testid="participant-name-modal"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="participant-id"]').text()).toBe('participant123')
    
    // メインコンテンツがブロックされていることを確認
    const mainContent = getMainContent(wrapper)
    expect(mainContent?.classes()).toContain('pointer-events-none')
  })

  test('正常系_ページロード_参加者IDがない場合にポップアップが表示されない', async () => {
    // Arrange
    mockSignInStore.value.id = ''

    // Act
    const wrapper = mount(ParticipantPage)
    await nextTick()

    // Assert
    expect(wrapper.find('[data-testid="participant-name-modal"]').exists()).toBe(false)
    
    // メインコンテンツがブロックされていないことを確認
    const mainContent = getMainContent(wrapper)
    expect(mainContent?.classes()).not.toContain('pointer-events-none')
  })

  test('正常系_ページロード_空白の参加者IDの場合にポップアップが表示されない', async () => {
    // Arrange
    mockSignInStore.value.id = '   '

    // Act
    const wrapper = mount(ParticipantPage)
    await nextTick()

    // Assert
    expect(wrapper.find('[data-testid="participant-name-modal"]').exists()).toBe(false)
  })

  test('正常系_成功フロー_名前登録成功でポップアップが閉じる', async () => {
    // Arrange
    mockSignInStore.value.id = 'participant123'
    const wrapper = mount(ParticipantPage)
    await nextTick()

    // 初期状態：ポップアップが表示されている
    expect(wrapper.find('[data-testid="participant-name-modal"]').exists()).toBe(true)
    
    // メインコンテンツがブロックされている
    let mainContent = getMainContent(wrapper)
    expect(mainContent?.classes()).toContain('pointer-events-none')

    // Act
    const successButton = wrapper.find('[data-testid="success-button"]')
    await successButton.trigger('click')
    await nextTick()

    // Assert
    expect(wrapper.find('[data-testid="participant-name-modal"]').exists()).toBe(false)
    
    // メインコンテンツのブロックが解除される
    mainContent = getMainContent(wrapper)
    expect(mainContent?.classes()).not.toContain('pointer-events-none')
  })

  test('正常系_エラーフロー_名前登録エラーでポップアップが開いたまま', async () => {
    // Arrange
    mockSignInStore.value.id = 'participant123'
    const wrapper = mount(ParticipantPage)
    await nextTick()

    // 初期状態：ポップアップが表示されている
    expect(wrapper.find('[data-testid="participant-name-modal"]').exists()).toBe(true)

    // Act
    const errorButton = wrapper.find('[data-testid="error-button"]')
    await errorButton.trigger('click')
    await nextTick()

    // Assert
    expect(wrapper.find('[data-testid="participant-name-modal"]').exists()).toBe(true)
    
    // メインコンテンツは引き続きブロックされている
    const mainContent = getMainContent(wrapper)
    expect(mainContent?.classes()).toContain('pointer-events-none')
  })

  test('正常系_クローズ試行_クローズボタンクリックでポップアップが開いたまま', async () => {
    // Arrange
    mockSignInStore.value.id = 'participant123'
    const wrapper = mount(ParticipantPage)
    await nextTick()

    // 初期状態：ポップアップが表示されている
    expect(wrapper.find('[data-testid="participant-name-modal"]').exists()).toBe(true)

    // Act
    const closeButton = wrapper.find('[data-testid="close-button"]')
    await closeButton.trigger('click')
    await nextTick()

    // Assert
    // 必須モーダルなので閉じない
    expect(wrapper.find('[data-testid="participant-name-modal"]').exists()).toBe(true)
    
    // メインコンテンツは引き続きブロックされている
    const mainContent = getMainContent(wrapper)
    expect(mainContent?.classes()).toContain('pointer-events-none')
  })

  test('正常系_動的サインイン_サインイン後にポップアップが表示される', async () => {
    // Arrange
    mockSignInStore.value.id = ''
    const wrapper = mount(ParticipantPage)
    await nextTick()

    // 初期状態：ポップアップが表示されていない
    expect(wrapper.find('[data-testid="participant-name-modal"]').exists()).toBe(false)

    // Act: サインイン状態を変更 - remount to simulate reactive change
    mockSignInStore.value.id = 'participant456'
    wrapper.unmount()
    const newWrapper = mount(ParticipantPage)
    await nextTick()

    // Assert: ポップアップが表示される
    expect(newWrapper.find('[data-testid="participant-name-modal"]').exists()).toBe(true)
    expect(newWrapper.find('[data-testid="participant-id"]').text()).toBe('participant456')
  })

  test('正常系_完全なユーザーフロー_サインインから名前登録完了まで', async () => {
    // Arrange: 初期状態（未サインイン）
    mockSignInStore.value.id = ''
    const wrapper = mount(ParticipantPage)
    await nextTick()

    // Step 1: 初期状態確認
    expect(wrapper.find('[data-testid="participant-name-modal"]').exists()).toBe(false)
    let mainContent = getMainContent(wrapper)
    expect(mainContent?.classes()).not.toContain('pointer-events-none')

    // Step 2: サインイン
    mockSignInStore.value.id = 'participant789'
    await nextTick()

    // Step 3: ポップアップ表示確認
    expect(wrapper.find('[data-testid="participant-name-modal"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="participant-id"]').text()).toBe('participant789')
    mainContent = getMainContent(wrapper)
    expect(mainContent?.classes()).toContain('pointer-events-none')

    // Step 4: 名前登録成功
    const successButton = wrapper.find('[data-testid="success-button"]')
    await successButton.trigger('click')
    await nextTick()

    // Step 5: 最終状態確認
    expect(wrapper.find('[data-testid="participant-name-modal"]').exists()).toBe(false)
    mainContent = getMainContent(wrapper)
    expect(mainContent?.classes()).not.toContain('pointer-events-none')
  })

  test('正常系_エラー回復フロー_エラー後の成功', async () => {
    // Arrange
    mockSignInStore.value.id = 'participant123'
    const wrapper = mount(ParticipantPage)
    await nextTick()

    // Step 1: エラー発生
    const errorButton = wrapper.find('[data-testid="error-button"]')
    await errorButton.trigger('click')
    await nextTick()

    // ポップアップが開いたまま
    expect(wrapper.find('[data-testid="participant-name-modal"]').exists()).toBe(true)

    // Step 2: 再試行で成功
    const successButton = wrapper.find('[data-testid="success-button"]')
    await successButton.trigger('click')
    await nextTick()

    // Step 3: ポップアップが閉じる
    expect(wrapper.find('[data-testid="participant-name-modal"]').exists()).toBe(false)
    const mainContent = getMainContent(wrapper)
    expect(mainContent?.classes()).not.toContain('pointer-events-none')
  })

  test('正常系_複数エラー処理_連続エラーでもポップアップが開いたまま', async () => {
    // Arrange
    mockSignInStore.value.id = 'participant123'
    const wrapper = mount(ParticipantPage)
    await nextTick()

    // Act: 複数回エラーを発生させる
    const errorButton = wrapper.find('[data-testid="error-button"]')
    
    await errorButton.trigger('click')
    await nextTick()
    expect(wrapper.find('[data-testid="participant-name-modal"]').exists()).toBe(true)

    await errorButton.trigger('click')
    await nextTick()
    expect(wrapper.find('[data-testid="participant-name-modal"]').exists()).toBe(true)

    await errorButton.trigger('click')
    await nextTick()

    // Assert: 複数エラー後もポップアップが開いている
    expect(wrapper.find('[data-testid="participant-name-modal"]').exists()).toBe(true)
    const mainContent = getMainContent(wrapper)
    expect(mainContent?.classes()).toContain('pointer-events-none')
  })

  test('正常系_ページコンテンツ_メインコンテンツが正常表示', () => {
    // Arrange & Act
    mockSignInStore.value.id = 'participant123'
    const wrapper = mount(ParticipantPage)

    // Assert
    expect(wrapper.text()).toContain('参加者ページ')
  })

  test('正常系_インタラクションブロック_ポップアップ表示中のメインコンテンツブロック', async () => {
    // Arrange
    mockSignInStore.value.id = 'participant123'
    const wrapper = mount(ParticipantPage)
    await nextTick()

    // Assert: ポップアップ表示中
    expect(wrapper.find('[data-testid="participant-name-modal"]').exists()).toBe(true)
    const mainContent = getMainContent(wrapper)
    expect(mainContent?.classes()).toContain('pointer-events-none')

    // Act: 成功後
    const successButton = wrapper.find('[data-testid="success-button"]')
    await successButton.trigger('click')
    await nextTick()

    // Assert: ブロック解除
    const updatedMainContent = getMainContent(wrapper)
    expect(updatedMainContent?.classes()).not.toContain('pointer-events-none')
  })

  test('正常系_参加者ID変更_参加者IDの動的変更でポップアップ更新', async () => {
    // Arrange
    mockSignInStore.value.id = 'participant123'
    const wrapper = mount(ParticipantPage)
    await nextTick()

    // 初期状態確認
    expect(wrapper.find('[data-testid="participant-id"]').text()).toBe('participant123')

    // Act: 参加者IDを変更 - remount to simulate reactive change
    mockSignInStore.value.id = 'participant999'
    wrapper.unmount()
    const newWrapper = mount(ParticipantPage)
    await nextTick()

    // Assert: ポップアップの参加者IDが更新される
    expect(newWrapper.find('[data-testid="participant-id"]').text()).toBe('participant999')
  })
})
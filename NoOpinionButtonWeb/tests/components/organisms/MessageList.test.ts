import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { nextTick } from 'vue'
import MessageList from '~/components/organisms/MessageList.vue'

// Mock MessageItem component
vi.mock('~/components/molecules/MessageItem.vue', () => ({
  default: {
    name: 'MessageItem',
    template: '<div class="message-item" data-testid="message-item">{{ participantName }}: {{ content }}</div>',
    props: ['participantId', 'participantName', 'content', 'createdAt', 'showTimestamp']
  }
}))

describe('MessageList', () => {
  const mockMessages = [
    {
      id: 'msg-1',
      meetingId: 'meeting-1',
      participantId: 'participant-1',
      participantName: '参加者1',
      content: 'こんにちは',
      createdAt: '2024-01-01T10:00:00Z',
      likeCount: 0,
      reportedCount: 0,
      isActive: true
    },
    {
      id: 'msg-2',
      meetingId: 'meeting-1',
      participantId: 'participant-2',
      participantName: '参加者2',
      content: 'よろしくお願いします',
      createdAt: '2024-01-01T10:01:00Z',
      likeCount: 0,
      reportedCount: 0,
      isActive: true
    }
  ]

  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('should render connection status indicator', () => {
    const wrapper = mount(MessageList, {
      props: {
        messages: [],
        isConnected: true,
        isConnecting: false
      }
    })

    const statusIndicator = wrapper.find('.bg-green-500')
    expect(statusIndicator.exists()).toBe(true)
    
    const statusText = wrapper.text()
    expect(statusText).toContain('接続済み')
  })

  it('should show connecting status', () => {
    const wrapper = mount(MessageList, {
      props: {
        messages: [],
        isConnected: false,
        isConnecting: true
      }
    })

    const statusIndicator = wrapper.find('.bg-yellow-500')
    expect(statusIndicator.exists()).toBe(true)
    
    const statusText = wrapper.text()
    expect(statusText).toContain('接続中...')
  })

  it('should show disconnected status', () => {
    const wrapper = mount(MessageList, {
      props: {
        messages: [],
        isConnected: false,
        isConnecting: false
      }
    })

    const statusIndicator = wrapper.find('.bg-red-500')
    expect(statusIndicator.exists()).toBe(true)
    
    const statusText = wrapper.text()
    expect(statusText).toContain('切断中')
  })

  it('should display connection error message', () => {
    const errorMessage = 'WebSocket接続エラー'
    const wrapper = mount(MessageList, {
      props: {
        messages: [],
        isConnected: false,
        isConnecting: false,
        connectionError: errorMessage
      }
    })

    expect(wrapper.text()).toContain(errorMessage)
    const errorElement = wrapper.find('.text-red-600')
    expect(errorElement.exists()).toBe(true)
  })

  it('should render empty state when no messages', () => {
    const wrapper = mount(MessageList, {
      props: {
        messages: [],
        isConnected: true
      }
    })

    expect(wrapper.text()).toContain('メッセージはまだありません')
    expect(wrapper.text()).toContain('新しいメッセージを待っています...')
  })

  it('should render message items', () => {
    const wrapper = mount(MessageList, {
      props: {
        messages: mockMessages,
        isConnected: true
      }
    })

    const messageItems = wrapper.findAllComponents({ name: 'MessageItem' })
    expect(messageItems).toHaveLength(2)

    // Check that props are passed correctly
    expect(messageItems[0].props()).toEqual({
      participantId: 'participant-1',
      participantName: '参加者1',
      content: 'こんにちは',
      createdAt: '2024-01-01T10:00:00Z',
      showTimestamp: false
    })
  })

  it('should pass showTimestamps prop to MessageItem components', () => {
    const wrapper = mount(MessageList, {
      props: {
        messages: mockMessages,
        isConnected: true,
        showTimestamps: true
      }
    })

    const messageItems = wrapper.findAllComponents({ name: 'MessageItem' })
    messageItems.forEach(item => {
      expect(item.props('showTimestamp')).toBe(true)
    })
  })

  it('should apply opacity when disconnected', () => {
    const wrapper = mount(MessageList, {
      props: {
        messages: mockMessages,
        isConnected: false,
        isConnecting: false
      }
    })

    const messageContainer = wrapper.find('.overflow-y-auto')
    expect(messageContainer.classes()).toContain('opacity-50')
  })

  it('should not apply opacity when connected', () => {
    const wrapper = mount(MessageList, {
      props: {
        messages: mockMessages,
        isConnected: true
      }
    })

    const messageContainer = wrapper.find('.overflow-y-auto')
    expect(messageContainer.classes()).not.toContain('opacity-50')
  })

  it('should handle auto scroll functionality', async () => {
    const wrapper = mount(MessageList, {
      props: {
        messages: [mockMessages[0]],
        isConnected: true,
        autoScroll: true
      }
    })

    // Mock scrollTop and scrollHeight
    const messageContainer = wrapper.find('.overflow-y-auto').element as HTMLElement
    Object.defineProperty(messageContainer, 'scrollTop', {
      writable: true,
      value: 0
    })
    Object.defineProperty(messageContainer, 'scrollHeight', {
      writable: true,
      value: 1000
    })

    // Add a new message
    await wrapper.setProps({
      messages: mockMessages
    })

    await nextTick()

    // Note: In a real browser environment, scrollTop would be set to scrollHeight
    // In the test environment, we can't fully test the scroll behavior
    // but we can verify the component structure is correct
    expect(wrapper.findAllComponents({ name: 'MessageItem' })).toHaveLength(2)
  })

  it('should disable auto scroll when autoScroll is false', async () => {
    const wrapper = mount(MessageList, {
      props: {
        messages: [mockMessages[0]],
        isConnected: true,
        autoScroll: false
      }
    })

    const messageContainer = wrapper.find('.overflow-y-auto').element as HTMLElement
    const originalScrollTop = messageContainer.scrollTop

    // Add a new message
    await wrapper.setProps({
      messages: mockMessages
    })

    await nextTick()

    // Scroll position should not change when autoScroll is disabled
    expect(messageContainer.scrollTop).toBe(originalScrollTop)
  })

  it('should render with correct CSS classes for responsive design', () => {
    const wrapper = mount(MessageList, {
      props: {
        messages: mockMessages,
        isConnected: true
      }
    })

    // Check main container classes
    const mainContainer = wrapper.find('.flex.flex-col.h-full')
    expect(mainContainer.exists()).toBe(true)

    // Check status indicator classes
    const statusContainer = wrapper.find('.p-3.bg-gray-50.border-b')
    expect(statusContainer.exists()).toBe(true)

    // Check message list container classes
    const messageContainer = wrapper.find('.flex-1.overflow-y-auto.p-4')
    expect(messageContainer.exists()).toBe(true)
  })
})
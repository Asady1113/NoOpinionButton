import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import MessageItem from '~/components/molecules/MessageItem.vue'

describe('MessageItem', () => {
  const defaultProps = {
    participantId: 'participant-123',
    participantName: 'テストユーザー',
    content: 'これはテストメッセージです',
    createdAt: '2024-01-01T12:00:00Z'
  }

  it('renders message content correctly', () => {
    const wrapper = mount(MessageItem, {
      props: defaultProps
    })

    expect(wrapper.text()).toContain('これはテストメッセージです')
  })

  it('displays participant name avatar with first 2 characters', () => {
    const wrapper = mount(MessageItem, {
      props: defaultProps
    })

    const avatar = wrapper.find('.w-12.h-12.bg-gray-300.rounded-full')
    expect(avatar.exists()).toBe(true)
    expect(avatar.text()).toBe('テス') // First 2 characters of 'テストユーザー'
  })

  it('applies correct styling classes', () => {
    const wrapper = mount(MessageItem, {
      props: defaultProps
    })

    // Check message container styling
    const container = wrapper.find('.flex.items-start.space-x-2.mb-4')
    expect(container.exists()).toBe(true)

    // Check avatar styling
    const avatar = wrapper.find('.w-12.h-12.bg-gray-300.rounded-full.flex.items-center.justify-center.text-sm.font-medium.flex-shrink-0')
    expect(avatar.exists()).toBe(true)

    // Check message bubble styling
    const bubble = wrapper.find('.bg-green-200.rounded-lg.px-4.py-2.ml-3.max-w-xs.lg\\:max-w-md')
    expect(bubble.exists()).toBe(true)
  })

  it('does not show timestamp by default', () => {
    const wrapper = mount(MessageItem, {
      props: defaultProps
    })

    const timestamp = wrapper.find('.text-xs.text-gray-600.mt-1')
    expect(timestamp.exists()).toBe(false)
  })

  it('shows formatted timestamp when showTimestamp is true', () => {
    const wrapper = mount(MessageItem, {
      props: {
        ...defaultProps,
        showTimestamp: true
      }
    })

    const timestamp = wrapper.find('.text-xs.text-gray-600.mt-1')
    expect(timestamp.exists()).toBe(true)
    // The exact format depends on locale, but should contain time
    expect(timestamp.text()).toMatch(/\d{2}:\d{2}/)
  })

  it('handles invalid timestamp gracefully', () => {
    const wrapper = mount(MessageItem, {
      props: {
        ...defaultProps,
        createdAt: 'invalid-date',
        showTimestamp: true
      }
    })

    const timestamp = wrapper.find('.text-xs.text-gray-600.mt-1')
    expect(timestamp.exists()).toBe(true)
    expect(timestamp.text()).toBe('')
  })

  it('handles short participant names correctly', () => {
    const wrapper = mount(MessageItem, {
      props: {
        ...defaultProps,
        participantName: 'A'
      }
    })

    const avatar = wrapper.find('.w-12.h-12.bg-gray-300.rounded-full')
    expect(avatar.text()).toBe('A')
  })

  it('falls back to participant ID when name is empty', () => {
    const wrapper = mount(MessageItem, {
      props: {
        ...defaultProps,
        participantName: ''
      }
    })

    const avatar = wrapper.find('.w-12.h-12.bg-gray-300.rounded-full')
    expect(avatar.text()).toBe('PA') // Falls back to participant ID
  })

  it('falls back to participant ID when name is only whitespace', () => {
    const wrapper = mount(MessageItem, {
      props: {
        ...defaultProps,
        participantName: '   '
      }
    })

    const avatar = wrapper.find('.w-12.h-12.bg-gray-300.rounded-full')
    expect(avatar.text()).toBe('PA') // Falls back to participant ID
  })

  it('handles empty participant ID correctly when name is also empty', () => {
    const wrapper = mount(MessageItem, {
      props: {
        ...defaultProps,
        participantId: '',
        participantName: ''
      }
    })

    const avatar = wrapper.find('.w-12.h-12.bg-gray-300.rounded-full')
    expect(avatar.text()).toBe('')
  })

  it('displays message content with proper text styling', () => {
    const wrapper = mount(MessageItem, {
      props: defaultProps
    })

    const messageText = wrapper.find('.text-sm.text-gray-900')
    expect(messageText.exists()).toBe(true)
    expect(messageText.text()).toBe('これはテストメッセージです')
  })

  it('has responsive design classes for message bubble', () => {
    const wrapper = mount(MessageItem, {
      props: defaultProps
    })

    const bubble = wrapper.find('.max-w-xs.lg\\:max-w-md')
    expect(bubble.exists()).toBe(true)
  })

  it('displays English participant names correctly', () => {
    const wrapper = mount(MessageItem, {
      props: {
        ...defaultProps,
        participantName: 'John Doe'
      }
    })

    const avatar = wrapper.find('.w-12.h-12.bg-gray-300.rounded-full')
    expect(avatar.text()).toBe('JO') // First 2 characters uppercased
  })

  it('handles mixed language participant names', () => {
    const wrapper = mount(MessageItem, {
      props: {
        ...defaultProps,
        participantName: 'A田中'
      }
    })

    const avatar = wrapper.find('.w-12.h-12.bg-gray-300.rounded-full')
    expect(avatar.text()).toBe('A田') // First 2 characters
  })
})
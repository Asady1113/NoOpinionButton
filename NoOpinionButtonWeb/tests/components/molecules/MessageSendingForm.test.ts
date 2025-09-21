import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import MessageSendingForm from '~/components/molecules/MessageSendingForm.vue'
import type { ApiError } from '~/types/error'
import { ApiErrorType } from '~/types/error'

// Mock the composable
const mockMessageSending = {
  state: {
    content: '',
    isSubmitting: false,
    error: null as ApiError | null,
    canSubmit: false
  },
  updateContent: vi.fn(),
  submitMessage: vi.fn(),
  clearForm: vi.fn(),
  clearError: vi.fn()
}

vi.mock('~/composables/message/useMessageSending', () => ({
  useMessageSending: () => mockMessageSending
}))

describe('MessageSendingForm', () => {
  const defaultProps = {
    meetingId: 'test-meeting-id',
    participantId: 'test-participant-id'
  }

  beforeEach(() => {
    vi.clearAllMocks()
    // Reset mock state
    mockMessageSending.state = {
      content: '',
      isSubmitting: false,
      error: null as ApiError | null,
      canSubmit: false
    }
  })

  it('renders with default placeholder', () => {
    const wrapper = mount(MessageSendingForm, {
      props: defaultProps
    })

    const input = wrapper.find('input')
    expect(input.attributes('placeholder')).toBe('匿名意見を入力してください')
  })

  it('renders with custom placeholder', () => {
    const wrapper = mount(MessageSendingForm, {
      props: {
        ...defaultProps,
        placeholder: 'カスタムプレースホルダー'
      }
    })

    const input = wrapper.find('input')
    expect(input.attributes('placeholder')).toBe('カスタムプレースホルダー')
  })

  it('calls updateContent when input changes', async () => {
    const wrapper = mount(MessageSendingForm, {
      props: defaultProps
    })

    const input = wrapper.find('input')
    await input.setValue('test message')

    expect(mockMessageSending.updateContent).toHaveBeenCalledWith('test message')
  })

  it('disables submit button when canSubmit is false', () => {
    mockMessageSending.state.canSubmit = false
    
    const wrapper = mount(MessageSendingForm, {
      props: defaultProps
    })

    const submitButton = wrapper.find('button[type="submit"]')
    expect(submitButton.attributes('disabled')).toBeDefined()
  })

  it('enables submit button when canSubmit is true', () => {
    mockMessageSending.state.canSubmit = true
    
    const wrapper = mount(MessageSendingForm, {
      props: defaultProps
    })

    const submitButton = wrapper.find('button[type="submit"]')
    expect(submitButton.attributes('disabled')).toBeUndefined()
  })

  it('shows loading spinner when submitting', () => {
    mockMessageSending.state.isSubmitting = true
    
    const wrapper = mount(MessageSendingForm, {
      props: defaultProps
    })

    const spinner = wrapper.find('.animate-spin')
    expect(spinner.exists()).toBe(true)
    
    const sendIcon = wrapper.find('svg:not(.animate-spin)')
    expect(sendIcon.exists()).toBe(false)
  })

  it('shows send icon when not submitting', () => {
    mockMessageSending.state.isSubmitting = false
    
    const wrapper = mount(MessageSendingForm, {
      props: defaultProps
    })

    const spinner = wrapper.find('.animate-spin')
    expect(spinner.exists()).toBe(false)
    
    const sendIcon = wrapper.find('button svg:not(.animate-spin)')
    expect(sendIcon.exists()).toBe(true)
  })

  it('displays error message when error exists', () => {
    mockMessageSending.state.error = {
      type: ApiErrorType.BadRequest,
      message: 'テストエラーメッセージ',
      statusCode: 400
    }
    
    const wrapper = mount(MessageSendingForm, {
      props: defaultProps
    })

    // Check that ApiErrorMessage component is rendered
    const apiErrorMessage = wrapper.findComponent({ name: 'ApiErrorMessage' })
    expect(apiErrorMessage.exists()).toBe(true)
    expect(apiErrorMessage.props('error')).toEqual(mockMessageSending.state.error)
  })

  it('does not display error message when no error', () => {
    mockMessageSending.state.error = null
    
    const wrapper = mount(MessageSendingForm, {
      props: defaultProps
    })

    // Check that ApiErrorMessage component is not rendered
    const apiErrorMessage = wrapper.findComponent({ name: 'ApiErrorMessage' })
    expect(apiErrorMessage.exists()).toBe(false)
  })

  it('calls submitMessage on form submit', async () => {
    mockMessageSending.state.canSubmit = true
    mockMessageSending.submitMessage.mockResolvedValue(undefined)
    
    const wrapper = mount(MessageSendingForm, {
      props: defaultProps
    })

    const form = wrapper.find('form')
    await form.trigger('submit')

    expect(mockMessageSending.submitMessage).toHaveBeenCalledWith(
      'test-meeting-id',
      'test-participant-id'
    )
  })

  it('emits messageSent event on successful submission', async () => {
    mockMessageSending.state.canSubmit = true
    mockMessageSending.submitMessage.mockResolvedValue(undefined)
    
    const wrapper = mount(MessageSendingForm, {
      props: defaultProps
    })

    const form = wrapper.find('form')
    await form.trigger('submit')

    expect(wrapper.emitted('messageSent')).toBeTruthy()
  })

  it('handles Enter key press to submit', async () => {
    mockMessageSending.state.canSubmit = true
    mockMessageSending.submitMessage.mockResolvedValue(undefined)
    
    const wrapper = mount(MessageSendingForm, {
      props: defaultProps
    })

    const input = wrapper.find('input')
    await input.trigger('keydown.enter')

    expect(mockMessageSending.submitMessage).toHaveBeenCalledWith(
      'test-meeting-id',
      'test-participant-id'
    )
  })

  it('does not submit when disabled prop is true', async () => {
    mockMessageSending.state.canSubmit = true
    
    const wrapper = mount(MessageSendingForm, {
      props: {
        ...defaultProps,
        disabled: true
      }
    })

    const form = wrapper.find('form')
    await form.trigger('submit')

    expect(mockMessageSending.submitMessage).not.toHaveBeenCalled()
  })

  it('disables input and button when disabled prop is true', () => {
    const wrapper = mount(MessageSendingForm, {
      props: {
        ...defaultProps,
        disabled: true
      }
    })

    const input = wrapper.find('input')
    const button = wrapper.find('button[type="submit"]')
    
    expect(input.attributes('disabled')).toBeDefined()
    expect(button.attributes('disabled')).toBeDefined()
  })

  it('handles submission error gracefully', async () => {
    mockMessageSending.state.canSubmit = true
    const error = new Error('Submission failed')
    mockMessageSending.submitMessage.mockRejectedValue(error)
    
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {})
    
    const wrapper = mount(MessageSendingForm, {
      props: defaultProps
    })

    const form = wrapper.find('form')
    await form.trigger('submit')

    expect(consoleSpy).toHaveBeenCalledWith('Message sending failed:', error)
    
    consoleSpy.mockRestore()
  })
})
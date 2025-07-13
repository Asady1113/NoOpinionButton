export const useSignInStore = () =>
  useState('signin', () => ({
    id: '',
    meetingId: '',
    meetingName: ''
  }))
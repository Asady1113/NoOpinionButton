import { useState } from '#app'

export const useSignInStore = () =>
  useState('signin', () => ({
    id: '',
    meetingId: '',
    meetingName: ''
  }))
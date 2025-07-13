import { useRuntimeConfig } from '#app'

export function useSignInApi() {
  const config = useRuntimeConfig()

  async function signIn(meetingId: string, password: string) {
    const response = await fetch(`${config.public.apiBaseUrl}/signin`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ meetingId, password })
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    return await response.json();
  }

  return { signIn };
}
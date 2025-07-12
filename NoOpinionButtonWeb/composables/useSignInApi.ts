export function useSignInApi() {
  async function signIn(meetingId: string, password: string) {
    const response = await fetch('http://127.0.0.1:3000/signin', {
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
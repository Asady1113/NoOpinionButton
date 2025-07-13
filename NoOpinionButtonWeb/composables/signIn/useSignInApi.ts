import { useRuntimeConfig } from '#app'
import { ApiErrorType } from '~/types/error'
import type { ApiError } from '~/types/error'

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
      let type: ApiErrorType

      switch (response.status) {
        case 400:
          type = ApiErrorType.BadRequest
        case 401:
          type = ApiErrorType.Unauthorized
          break
        case 404:
          type = ApiErrorType.NotFound
          break
        default:
          type = ApiErrorType.Server
          break
      }

      const responseBody = await response.json()

      const error: ApiError = {
        type,
        message: responseBody?.Error ?? `Request failed with status ${response.status}`,
        statusCode: response.status,
      }
      
      throw error;
    }

    return await response.json();
  }

  return { signIn };
}
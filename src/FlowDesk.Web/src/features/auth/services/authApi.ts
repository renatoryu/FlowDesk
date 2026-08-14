import {
  apiRequest,
  authenticatedApiRequest,
} from '../../../shared/api/apiClient'

import type {
  CurrentUserResponse,
  LoginRequest,
  LoginResponse,
  RefreshSessionRequest,
  RefreshSessionResponse,
} from '../types/authTypes'

const refreshRequests = new Map<
  string,
  Promise<RefreshSessionResponse>
>()

export function login(
  request: LoginRequest,
): Promise<LoginResponse> {
  return apiRequest<LoginResponse>('/auth/login', {
    method: 'POST',
    body: JSON.stringify(request),
  })
}

export function refreshSession(
  request: RefreshSessionRequest,
): Promise<RefreshSessionResponse> {
  const currentRequest = refreshRequests.get(
    request.refreshToken,
  )

  if (currentRequest) {
    return currentRequest
  }

  const newRequest =
    apiRequest<RefreshSessionResponse>(
      '/auth/refresh',
      {
        method: 'POST',
        body: JSON.stringify(request),
      },
    ).finally(() => {
      refreshRequests.delete(request.refreshToken)
    })

  refreshRequests.set(
    request.refreshToken,
    newRequest,
  )

  return newRequest
}

export function getCurrentUser(
  accessToken: string,
): Promise<CurrentUserResponse> {
  return authenticatedApiRequest<CurrentUserResponse>(
    '/auth/me',
    accessToken,
  )
}

import type {
  AuthSession,
  LoginResponse,
  RefreshSessionResponse,
} from '../types/authTypes'

const sessionKey = 'flowdesk.auth.session'

export function createAuthSession(
  response: LoginResponse,
): AuthSession {
  return {
    user: {
      id: response.id,
      fullName: response.fullName,
      email: response.email,
      role: response.role,
    },
    accessToken: response.accessToken,
    accessTokenExpiresAtUtc:
      response.accessTokenExpiresAtUtc,
    refreshToken: response.refreshToken,
    refreshTokenExpiresAtUtc:
      response.refreshTokenExpiresAtUtc,
  }
}

export function renewAuthSession(
  session: AuthSession,
  response: RefreshSessionResponse,
): AuthSession {
  return {
    ...session,
    accessToken: response.accessToken,
    accessTokenExpiresAtUtc:
      response.accessTokenExpiresAtUtc,
    refreshToken: response.refreshToken,
    refreshTokenExpiresAtUtc:
      response.refreshTokenExpiresAtUtc,
  }
}

export function saveAuthSession(
  session: AuthSession,
): void {
  sessionStorage.setItem(
    sessionKey,
    JSON.stringify(session),
  )
}

export function getAuthSession(): AuthSession | null {
  const storedSession =
    sessionStorage.getItem(sessionKey)

  if (!storedSession) {
    return null
  }

  try {
    return JSON.parse(storedSession) as AuthSession
  } catch {
    sessionStorage.removeItem(sessionKey)
    return null
  }
}

export function clearAuthSession(): void {
  sessionStorage.removeItem(sessionKey)
}

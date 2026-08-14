import {
  beforeEach,
  describe,
  expect,
  it,
} from 'vitest'
import {
  clearAuthSession,
  createAuthSession,
  getAuthSession,
  renewAuthSession,
  saveAuthSession,
} from './authSession'
import type {
  LoginResponse,
  RefreshSessionResponse,
} from '../types/authTypes'

const loginResponse: LoginResponse = {
  id: '11111111-1111-1111-1111-111111111111',
  fullName: 'FlowDesk Customer',
  email: 'customer@flowdesk.local',
  role: 'Customer',
  accessToken: 'access-token',
  accessTokenExpiresAtUtc:
    '2026-08-14T15:00:00.000Z',
  refreshToken: 'refresh-token',
  refreshTokenExpiresAtUtc:
    '2026-08-21T15:00:00.000Z',
}

const refreshResponse: RefreshSessionResponse = {
  accessToken: 'renewed-access-token',
  accessTokenExpiresAtUtc:
    '2026-08-14T16:00:00.000Z',
  refreshToken: 'renewed-refresh-token',
  refreshTokenExpiresAtUtc:
    '2026-08-21T16:00:00.000Z',
}

describe('authSession', () => {
  beforeEach(() => {
    sessionStorage.clear()
  })

  it('creates a session from a login response', () => {
    const session = createAuthSession(loginResponse)

    expect(session.user).toEqual({
      id: loginResponse.id,
      fullName: loginResponse.fullName,
      email: loginResponse.email,
      role: loginResponse.role,
    })

    expect(session.accessToken).toBe(
      loginResponse.accessToken,
    )
  })

  it('renews tokens while preserving the user', () => {
    const session = createAuthSession(loginResponse)

    const renewedSession = renewAuthSession(
      session,
      refreshResponse,
    )

    expect(renewedSession.user).toEqual(
      session.user,
    )
    expect(renewedSession.accessToken).toBe(
      refreshResponse.accessToken,
    )
    expect(renewedSession.refreshToken).toBe(
      refreshResponse.refreshToken,
    )
  })

  it('stores and retrieves a valid session', () => {
    const session = createAuthSession(loginResponse)

    saveAuthSession(session)

    expect(getAuthSession()).toEqual(session)
  })

  it('removes invalid stored data safely', () => {
    sessionStorage.setItem(
      'flowdesk.auth.session',
      '{invalid-json',
    )

    expect(getAuthSession()).toBeNull()
    expect(
      sessionStorage.getItem(
        'flowdesk.auth.session',
      ),
    ).toBeNull()
  })

  it('clears the stored session', () => {
    saveAuthSession(
      createAuthSession(loginResponse),
    )

    clearAuthSession()

    expect(getAuthSession()).toBeNull()
  })
})

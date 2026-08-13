import {
  useCallback,
  useEffect,
  useMemo,
  useState,
  type PropsWithChildren,
} from 'react'
import { refreshSession } from '../services/authApi'
import {
  clearAuthSession,
  getAuthSession,
  renewAuthSession,
  saveAuthSession,
} from '../services/authSession'
import type { AuthSession } from '../types/authTypes'
import {
  AuthContext,
  type AuthContextValue,
} from './authContext'

const refreshLeadTimeMs = 60_000

function AuthProvider({
  children,
}: PropsWithChildren) {
  const [session, setSession] =
    useState<AuthSession | null>(getAuthSession)

  const authenticate = useCallback(
    (authenticatedSession: AuthSession) => {
      saveAuthSession(authenticatedSession)
      setSession(authenticatedSession)
    },
    [],
  )

  const signOut = useCallback(() => {
    clearAuthSession()
    setSession(null)
  }, [])

  useEffect(() => {
    if (!session) {
      return
    }

    const now = Date.now()
    const accessTokenExpiresAt = Date.parse(
      session.accessTokenExpiresAtUtc,
    )
    const refreshTokenExpiresAt = Date.parse(
      session.refreshTokenExpiresAtUtc,
    )

    if (
      !Number.isFinite(accessTokenExpiresAt) ||
      !Number.isFinite(refreshTokenExpiresAt) ||
      refreshTokenExpiresAt <= now
    ) {
      const invalidSessionTimerId =
        window.setTimeout(signOut, 0)

      return () => {
        window.clearTimeout(invalidSessionTimerId)
      }
    }

    const refreshDelay = Math.max(
      accessTokenExpiresAt -
      now -
      refreshLeadTimeMs,
      0,
    )

    let cancelled = false

    const timerId = window.setTimeout(() => {
      void refreshSession({
        refreshToken: session.refreshToken,
      })
        .then((response) => {
          if (cancelled) {
            return
          }

          const renewedSession =
            renewAuthSession(session, response)

          saveAuthSession(renewedSession)
          setSession(renewedSession)
        })
        .catch(() => {
          if (!cancelled) {
            signOut()
          }
        })
    }, refreshDelay)

    return () => {
      cancelled = true
      window.clearTimeout(timerId)
    }
  }, [session, signOut])

  const value = useMemo<AuthContextValue>(
    () => ({
      session,
      authenticate,
      signOut,
    }),
    [authenticate, session, signOut],
  )

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  )
}

export default AuthProvider

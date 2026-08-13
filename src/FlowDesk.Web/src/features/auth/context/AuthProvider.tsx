import {
  useMemo,
  useState,
  type PropsWithChildren,
} from 'react'
import {
  clearAuthSession,
  getAuthSession,
  saveAuthSession,
} from '../services/authSession'
import type { AuthSession } from '../types/authTypes'
import {
  AuthContext,
  type AuthContextValue,
} from './authContext'

function AuthProvider({ children }: PropsWithChildren) {
  const [session, setSession] =
    useState<AuthSession | null>(getAuthSession)

  const value = useMemo<AuthContextValue>(
    () => ({
      session,
      authenticate: (authenticatedSession) => {
        saveAuthSession(authenticatedSession)
        setSession(authenticatedSession)
      },
      signOut: () => {
        clearAuthSession()
        setSession(null)
      },
    }),
    [session],
  )

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  )
}

export default AuthProvider

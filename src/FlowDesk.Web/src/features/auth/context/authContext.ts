import { createContext } from 'react'
import type { AuthSession } from '../types/authTypes'

export interface AuthContextValue {
  session: AuthSession | null
  authenticate: (session: AuthSession) => void
  signOut: () => void
}

export const AuthContext =
  createContext<AuthContextValue | null>(null)

export type UserRole = 'Customer' | 'Agent' | 'Admin'

export interface LoginRequest {
  email: string
  password: string
}

export interface LoginResponse {
  id: string
  fullName: string
  email: string
  role: UserRole
  accessToken: string
  accessTokenExpiresAtUtc: string
  refreshToken: string
  refreshTokenExpiresAtUtc: string
}

export interface AuthUser {
  id: string
  fullName: string
  email: string
  role: UserRole
}

export interface AuthSession {
  user: AuthUser
  accessToken: string
  accessTokenExpiresAtUtc: string
  refreshToken: string
  refreshTokenExpiresAtUtc: string
}

export interface RefreshSessionRequest {
  refreshToken: string
}

export interface RefreshSessionResponse {
  accessToken: string
  accessTokenExpiresAtUtc: string
  refreshToken: string
  refreshTokenExpiresAtUtc: string
}

export type CurrentUserResponse = AuthUser

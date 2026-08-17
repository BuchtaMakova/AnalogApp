export type UserRole = 'User' | 'Admin'

export interface AuthUser {
  id: string
  email: string
  role: UserRole
}

export interface AuthResult {
  token: string
  expiresAtUtc: string
  userId: string
  email: string
  role: UserRole
}

export interface AuthCredentials {
  email: string
  password: string
}

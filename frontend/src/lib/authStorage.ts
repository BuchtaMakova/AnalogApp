import type { AuthResult, AuthUser } from '@/types/auth'

const TOKEN_KEY = 'analoghub_token'
const USER_KEY = 'analoghub_user'

export function getStoredToken(): string | null {
  return localStorage.getItem(TOKEN_KEY)
}

export function getStoredUser(): AuthUser | null {
  const raw = localStorage.getItem(USER_KEY)
  if (!raw) return null
  try {
    return JSON.parse(raw) as AuthUser
  } catch {
    return null
  }
}

export function storeAuthResult(result: AuthResult): AuthUser {
  const user: AuthUser = { id: result.userId, email: result.email, role: result.role }
  localStorage.setItem(TOKEN_KEY, result.token)
  localStorage.setItem(USER_KEY, JSON.stringify(user))
  return user
}

export function clearStoredAuth(): void {
  localStorage.removeItem(TOKEN_KEY)
  localStorage.removeItem(USER_KEY)
}

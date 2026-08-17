import { beforeEach, describe, expect, it } from 'vitest'
import { clearStoredAuth, getStoredToken, getStoredUser, storeAuthResult } from '@/lib/authStorage'
import type { AuthResult } from '@/types/auth'

const sampleResult: AuthResult = {
  token: 'sample-token',
  expiresAtUtc: '2026-01-01T00:00:00Z',
  userId: 'user-1',
  email: 'photographer@example.com',
  role: 'Admin',
}

describe('authStorage', () => {
  beforeEach(() => localStorage.clear())

  it('returns null when nothing is stored', () => {
    expect(getStoredToken()).toBeNull()
    expect(getStoredUser()).toBeNull()
  })

  it('persists the token and derived user from an auth result', () => {
    const user = storeAuthResult(sampleResult)

    expect(user).toEqual({ id: 'user-1', email: 'photographer@example.com', role: 'Admin' })
    expect(getStoredToken()).toBe('sample-token')
    expect(getStoredUser()).toEqual(user)
  })

  it('clears both the token and user', () => {
    storeAuthResult(sampleResult)

    clearStoredAuth()

    expect(getStoredToken()).toBeNull()
    expect(getStoredUser()).toBeNull()
  })

  it('returns null for corrupted stored user JSON instead of throwing', () => {
    localStorage.setItem('analoghub_user', '{not-json')

    expect(getStoredUser()).toBeNull()
  })
})

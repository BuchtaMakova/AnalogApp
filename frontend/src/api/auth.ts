import { apiClient } from '@/api/client'
import type { AuthCredentials, AuthResult } from '@/types/auth'

export async function register(credentials: AuthCredentials): Promise<AuthResult> {
  const { data } = await apiClient.post<AuthResult>('/auth/register', credentials)
  return data
}

export async function login(credentials: AuthCredentials): Promise<AuthResult> {
  const { data } = await apiClient.post<AuthResult>('/auth/login', credentials)
  return data
}

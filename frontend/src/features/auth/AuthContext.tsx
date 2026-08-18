import { useQueryClient } from '@tanstack/react-query'
import { createContext, useContext, useMemo, useState, type PropsWithChildren } from 'react'
import * as authApi from '@/api/auth'
import { clearStoredAuth, getStoredUser, storeAuthResult } from '@/lib/authStorage'
import type { AuthCredentials, AuthUser } from '@/types/auth'

interface AuthContextValue {
  user: AuthUser | null
  isAuthenticated: boolean
  login: (credentials: AuthCredentials) => Promise<void>
  register: (credentials: AuthCredentials) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: PropsWithChildren) {
  const [user, setUser] = useState<AuthUser | null>(() => getStoredUser())
  const queryClient = useQueryClient()

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      isAuthenticated: user !== null,
      login: async (credentials) => {
        // Query cache is keyed without a user id, so a previous account's data would
        // otherwise stay visible (stale-while-revalidate) until something else refetches it.
        queryClient.clear()
        setUser(storeAuthResult(await authApi.login(credentials)))
      },
      register: async (credentials) => {
        queryClient.clear()
        setUser(storeAuthResult(await authApi.register(credentials)))
      },
      logout: () => {
        clearStoredAuth()
        queryClient.clear()
        setUser(null)
      },
    }),
    [user, queryClient],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext)
  if (!context) throw new Error('useAuth must be used within an AuthProvider')
  return context
}

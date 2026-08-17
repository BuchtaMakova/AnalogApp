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

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      isAuthenticated: user !== null,
      login: async (credentials) => setUser(storeAuthResult(await authApi.login(credentials))),
      register: async (credentials) => setUser(storeAuthResult(await authApi.register(credentials))),
      logout: () => {
        clearStoredAuth()
        setUser(null)
      },
    }),
    [user],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext)
  if (!context) throw new Error('useAuth must be used within an AuthProvider')
  return context
}

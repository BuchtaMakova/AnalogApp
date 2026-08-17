import { useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { Aperture } from 'lucide-react'
import { useAuth } from '@/features/auth/AuthContext'
import { Button } from '@/components/ui/Button'
import { FormField, Input } from '@/components/ui/Field'
import type { ProblemDetails } from '@/api/client'

export function AuthPage() {
  const { login, register } = useAuth()
  const navigate = useNavigate()

  const [mode, setMode] = useState<'login' | 'register'>('login')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setError(null)
    setIsSubmitting(true)
    try {
      await (mode === 'login' ? login({ email, password }) : register({ email, password }))
      navigate('/', { replace: true })
    } catch (err) {
      const problem = (err as { response?: { data?: ProblemDetails } }).response?.data
      setError(problem?.errors?.Email?.[0] ?? problem?.title ?? 'Something went wrong. Please try again.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="flex h-screen items-center justify-center bg-neutral-950 px-4">
      <div className="w-full max-w-sm rounded-xl border border-neutral-800 bg-neutral-900/50 p-8">
        <div className="mb-6 flex flex-col items-center gap-2 text-center">
          <Aperture size={28} className="text-white" />
          <h1 className="text-lg font-semibold text-white">Analog Hub</h1>
          <p className="text-sm text-neutral-400">
            {mode === 'login' ? 'Sign in to your darkroom.' : 'Create an account to get started.'}
          </p>
        </div>

        <form onSubmit={handleSubmit} className="flex flex-col gap-4">
          <FormField label="Email">
            <Input
              type="email"
              required
              autoComplete="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
            />
          </FormField>
          <FormField label="Password">
            <Input
              type="password"
              required
              minLength={8}
              autoComplete={mode === 'login' ? 'current-password' : 'new-password'}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
            />
          </FormField>

          {error && <p className="text-sm text-red-400">{error}</p>}

          <Button type="submit" variant="primary" disabled={isSubmitting} className="mt-2 justify-center">
            {isSubmitting ? 'Please wait…' : mode === 'login' ? 'Sign in' : 'Create account'}
          </Button>
        </form>

        <button
          type="button"
          onClick={() => {
            setMode(mode === 'login' ? 'register' : 'login')
            setError(null)
          }}
          className="mt-5 w-full text-center text-xs text-neutral-400 hover:text-neutral-200"
        >
          {mode === 'login' ? "Don't have an account? Register" : 'Already have an account? Sign in'}
        </button>
      </div>
    </div>
  )
}

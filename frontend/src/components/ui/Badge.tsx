import type { PropsWithChildren } from 'react'
import { cn } from '@/lib/cn'

interface BadgeProps extends PropsWithChildren {
  tone?: 'neutral' | 'ai' | 'success' | 'warning' | 'danger'
  className?: string
}

const toneClasses: Record<NonNullable<BadgeProps['tone']>, string> = {
  neutral: 'bg-neutral-800 text-neutral-300',
  ai: 'bg-violet-500/15 text-violet-300',
  success: 'bg-emerald-500/15 text-emerald-300',
  warning: 'bg-amber-500/15 text-amber-300',
  danger: 'bg-red-500/15 text-red-300',
}

export function Badge({ tone = 'neutral', className, children }: BadgeProps) {
  return (
    <span
      className={cn(
        'inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium',
        toneClasses[tone],
        className,
      )}
    >
      {children}
    </span>
  )
}

import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { Badge } from '@/components/ui/Badge'

describe('Badge', () => {
  it('renders its children text', () => {
    render(<Badge>portrait</Badge>)
    expect(screen.getByText('portrait')).toBeInTheDocument()
  })

  it('defaults to the neutral tone', () => {
    render(<Badge>tag</Badge>)
    expect(screen.getByText('tag')).toHaveClass('bg-neutral-800')
  })

  it('applies the ai tone class for AI-suggested tags', () => {
    render(<Badge tone="ai">golden-hour</Badge>)
    expect(screen.getByText('golden-hour')).toHaveClass('text-violet-300')
  })

  it('merges a custom className with the tone classes', () => {
    render(<Badge tone="success" className="ml-2">Ready</Badge>)
    const badge = screen.getByText('Ready')
    expect(badge).toHaveClass('ml-2')
    expect(badge).toHaveClass('text-emerald-300')
  })
})

import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { StarRating } from '@/components/ui/StarRating'

describe('StarRating', () => {
  it('renders five star buttons', () => {
    render(<StarRating value={3} />)
    expect(screen.getAllByRole('button')).toHaveLength(5)
  })

  it('calls onChange with the clicked star value', async () => {
    const user = userEvent.setup()
    const onChange = vi.fn()
    render(<StarRating value={0} onChange={onChange} />)

    await user.click(screen.getByLabelText('Rate 4 stars'))

    expect(onChange).toHaveBeenCalledWith(4)
  })

  it('clicking the currently-set star clears the rating to zero', async () => {
    const user = userEvent.setup()
    const onChange = vi.fn()
    render(<StarRating value={3} onChange={onChange} />)

    await user.click(screen.getByLabelText('Rate 3 stars'))

    expect(onChange).toHaveBeenCalledWith(0)
  })

  it('disables all buttons in read-only mode', () => {
    render(<StarRating value={2} readOnly />)

    for (const button of screen.getAllByRole('button')) {
      expect(button).toBeDisabled()
    }
  })

  it('read-only mode never calls onChange even if clicked', async () => {
    const user = userEvent.setup()
    const onChange = vi.fn()
    render(<StarRating value={2} onChange={onChange} readOnly />)

    await user.click(screen.getByLabelText('Rate 5 stars'))

    expect(onChange).not.toHaveBeenCalled()
  })
})

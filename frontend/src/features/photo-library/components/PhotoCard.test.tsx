import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { PhotoCard } from '@/features/photo-library/components/PhotoCard'
import type { PhotoListItem } from '@/types/photo'

const photo: PhotoListItem = {
  id: 'photo-1',
  filmRollId: 'roll-1',
  thumbnailUrl: null,
  blurHash: null,
  widthPx: null,
  heightPx: null,
  rating: 0,
  rotationDegrees: 0,
  processingStatus: 'Ready',
  captureDateUtc: null,
  tags: [],
}

describe('PhotoCard', () => {
  it('clicking the card opens the lightbox when not in selection mode', async () => {
    const user = userEvent.setup()
    const onClick = vi.fn()
    const onToggleSelect = vi.fn()
    render(
      <PhotoCard
        photo={photo}
        size={100}
        onClick={onClick}
        selected={false}
        selectionMode={false}
        onToggleSelect={onToggleSelect}
      />,
    )

    await user.click(screen.getByRole('button'))

    expect(onClick).toHaveBeenCalledOnce()
    expect(onToggleSelect).not.toHaveBeenCalled()
  })

  it('clicking the checkbox toggles selection instead of opening the lightbox', async () => {
    const user = userEvent.setup()
    const onClick = vi.fn()
    const onToggleSelect = vi.fn()
    render(
      <PhotoCard
        photo={photo}
        size={100}
        onClick={onClick}
        selected={false}
        selectionMode={false}
        onToggleSelect={onToggleSelect}
      />,
    )

    await user.click(screen.getByRole('checkbox'))

    expect(onToggleSelect).toHaveBeenCalledOnce()
    expect(onClick).not.toHaveBeenCalled()
  })

  it('clicking the card toggles selection instead of opening the lightbox once selection mode is active', async () => {
    const user = userEvent.setup()
    const onClick = vi.fn()
    const onToggleSelect = vi.fn()
    render(
      <PhotoCard
        photo={photo}
        size={100}
        onClick={onClick}
        selected={false}
        selectionMode={true}
        onToggleSelect={onToggleSelect}
      />,
    )

    await user.click(screen.getByRole('button'))

    expect(onToggleSelect).toHaveBeenCalledOnce()
    expect(onClick).not.toHaveBeenCalled()
  })

  it('reflects the selected state on the checkbox', () => {
    render(
      <PhotoCard
        photo={photo}
        size={100}
        onClick={vi.fn()}
        selected={true}
        selectionMode={true}
        onToggleSelect={vi.fn()}
      />,
    )

    expect(screen.getByRole('checkbox')).toHaveAttribute('aria-checked', 'true')
  })
})

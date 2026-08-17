import { useVirtualizer } from '@tanstack/react-virtual'
import { useEffect, useRef, useState } from 'react'
import { Images } from 'lucide-react'
import { PhotoCard } from '@/features/photo-library/components/PhotoCard'
import { Spinner } from '@/components/ui/Spinner'
import type { PhotoListItem } from '@/types/photo'

const GAP = 4
const MIN_CARD_SIZE = 170

interface PhotoGridProps {
  photos: PhotoListItem[]
  isLoading: boolean
  hasNextPage: boolean
  isFetchingNextPage: boolean
  onFetchNextPage: () => void
  onSelectPhoto: (photoId: string) => void
}

/**
 * Uniform square-crop grid (not true masonry) so row heights are predictable and cheap to
 * virtualize with TanStack Virtual — the grid only ever renders the rows near the viewport.
 */
export function PhotoGrid({
  photos,
  isLoading,
  hasNextPage,
  isFetchingNextPage,
  onFetchNextPage,
  onSelectPhoto,
}: PhotoGridProps) {
  const scrollRef = useRef<HTMLDivElement>(null)
  const [containerWidth, setContainerWidth] = useState(0)

  useEffect(() => {
    const el = scrollRef.current
    if (!el) return
    const observer = new ResizeObserver((entries) => setContainerWidth(entries[0].contentRect.width))
    observer.observe(el)
    return () => observer.disconnect()
  }, [])

  const columns = Math.max(2, Math.floor((containerWidth + GAP) / (MIN_CARD_SIZE + GAP))) || 4
  const cardSize = columns > 0 ? (containerWidth - GAP * (columns - 1)) / columns : MIN_CARD_SIZE
  const rowCount = Math.ceil(photos.length / columns)

  const rowVirtualizer = useVirtualizer({
    count: rowCount,
    getScrollElement: () => scrollRef.current,
    estimateSize: () => cardSize + GAP,
    overscan: 4,
  })

  const virtualRows = rowVirtualizer.getVirtualItems()

  useEffect(() => {
    const lastRow = virtualRows.at(-1)
    if (!lastRow) return
    if (lastRow.index >= rowCount - 2 && hasNextPage && !isFetchingNextPage) {
      onFetchNextPage()
    }
  }, [virtualRows, rowCount, hasNextPage, isFetchingNextPage, onFetchNextPage])

  if (isLoading) {
    return (
      <div className="flex flex-1 items-center justify-center text-neutral-500">
        <Spinner size={24} />
      </div>
    )
  }

  if (photos.length === 0) {
    return (
      <div className="flex flex-1 flex-col items-center justify-center gap-3 text-neutral-600">
        <Images size={40} />
        <p className="text-sm">No photos match the current filters.</p>
      </div>
    )
  }

  return (
    <div ref={scrollRef} className="flex-1 overflow-y-auto px-4 pb-4">
      <div style={{ height: rowVirtualizer.getTotalSize(), position: 'relative' }}>
        {virtualRows.map((virtualRow) => {
          const startIndex = virtualRow.index * columns
          const rowPhotos = photos.slice(startIndex, startIndex + columns)

          return (
            <div
              key={virtualRow.key}
              style={{
                position: 'absolute',
                top: 0,
                left: 0,
                width: '100%',
                height: virtualRow.size,
                transform: `translateY(${virtualRow.start}px)`,
                display: 'flex',
                gap: GAP,
              }}
            >
              {rowPhotos.map((photo) => (
                <PhotoCard key={photo.id} photo={photo} size={cardSize} onClick={() => onSelectPhoto(photo.id)} />
              ))}
            </div>
          )
        })}
      </div>

      {isFetchingNextPage && (
        <div className="flex justify-center py-4 text-neutral-500">
          <Spinner size={18} />
        </div>
      )}
    </div>
  )
}

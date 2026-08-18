import { useVirtualizer } from '@tanstack/react-virtual'
import { useEffect, useMemo, useRef, useState } from 'react'
import { ChevronDown, Images } from 'lucide-react'
import { PhotoCard } from '@/features/photo-library/components/PhotoCard'
import { Spinner } from '@/components/ui/Spinner'
import { cn } from '@/lib/cn'
import type { PhotoListItem } from '@/types/photo'

const GAP = 4
const MIN_CARD_SIZE = 170
const DIVIDER_HEIGHT = 36

type GridRow =
  | { type: 'divider'; filmRollId: string; label: string; count: number; collapsed: boolean }
  | { type: 'photos'; items: PhotoListItem[] }

interface PhotoGridProps {
  photos: PhotoListItem[]
  filmRollNames: Map<string, string>
  isLoading: boolean
  hasNextPage: boolean
  isFetchingNextPage: boolean
  onFetchNextPage: () => void
  onSelectPhoto: (photoId: string) => void
  selectedIds: Set<string>
  onToggleSelect: (photoId: string) => void
}

/**
 * Uniform square-crop grid (not true masonry) so row heights are predictable and cheap to
 * virtualize with TanStack Virtual — the grid only ever renders the rows near the viewport.
 */
export function PhotoGrid({
  photos,
  filmRollNames,
  isLoading,
  hasNextPage,
  isFetchingNextPage,
  onFetchNextPage,
  onSelectPhoto,
  selectedIds,
  onToggleSelect,
}: PhotoGridProps) {
  const scrollRef = useRef<HTMLDivElement>(null)
  const [containerWidth, setContainerWidth] = useState(0)
  const [collapsedRollIds, setCollapsedRollIds] = useState<Set<string>>(new Set())

  const toggleRollCollapsed = (filmRollId: string) => {
    setCollapsedRollIds((prev) => {
      const next = new Set(prev)
      if (next.has(filmRollId)) next.delete(filmRollId)
      else next.add(filmRollId)
      return next
    })
  }

  // scrollRef must stay attached to the DOM across every render state (loading/empty/populated) —
  // this effect only runs once on mount, so if the ref were conditionally absent on first paint
  // (e.g. still isLoading on a cold page load), it would never observe anything and the grid would
  // stay collapsed at containerWidth 0 forever, even once data arrives.
  useEffect(() => {
    const el = scrollRef.current
    if (!el) return
    const observer = new ResizeObserver((entries) => setContainerWidth(entries[0].contentRect.width))
    observer.observe(el)
    return () => observer.disconnect()
  }, [])

  const columns = Math.max(2, Math.floor((containerWidth + GAP) / (MIN_CARD_SIZE + GAP))) || 4
  const cardSize = columns > 0 ? (containerWidth - GAP * (columns - 1)) / columns : MIN_CARD_SIZE

  const rollPhotoCounts = useMemo(() => {
    const counts = new Map<string, number>()
    for (const photo of photos) {
      counts.set(photo.filmRollId, (counts.get(photo.filmRollId) ?? 0) + 1)
    }
    return counts
  }, [photos])

  // Photos arrive sorted by capture date, and a roll's frames are shot within a tight time
  // window, so they land contiguously here. Whenever the roll changes we close out the current
  // row (even if it's not full) so the divider always starts on a fresh line. A collapsed roll
  // still gets its divider (so it can be expanded again) but contributes no photo rows.
  const rows = useMemo<GridRow[]>(() => {
    const result: GridRow[] = []
    let currentRollId: string | null = null
    let currentRollCollapsed = false
    let currentRowItems: PhotoListItem[] = []

    const flushRow = () => {
      if (currentRowItems.length > 0) {
        result.push({ type: 'photos', items: currentRowItems })
        currentRowItems = []
      }
    }

    for (const photo of photos) {
      if (photo.filmRollId !== currentRollId) {
        flushRow()
        currentRollId = photo.filmRollId
        currentRollCollapsed = collapsedRollIds.has(currentRollId)
        result.push({
          type: 'divider',
          filmRollId: currentRollId,
          label: filmRollNames.get(currentRollId) ?? 'Unknown roll',
          count: rollPhotoCounts.get(currentRollId) ?? 0,
          collapsed: currentRollCollapsed,
        })
      }
      if (!currentRollCollapsed) {
        currentRowItems.push(photo)
        if (currentRowItems.length === columns) flushRow()
      }
    }
    flushRow()

    return result
  }, [photos, filmRollNames, rollPhotoCounts, collapsedRollIds, columns])

  const rowVirtualizer = useVirtualizer({
    count: rows.length,
    getScrollElement: () => scrollRef.current,
    estimateSize: (index) => (rows[index]?.type === 'divider' ? DIVIDER_HEIGHT : cardSize + GAP),
    overscan: 4,
  })

  const virtualRows = rowVirtualizer.getVirtualItems()

  useEffect(() => {
    const lastRow = virtualRows.at(-1)
    if (!lastRow) return
    if (lastRow.index >= rows.length - 2 && hasNextPage && !isFetchingNextPage) {
      onFetchNextPage()
    }
  }, [virtualRows, rows.length, hasNextPage, isFetchingNextPage, onFetchNextPage])

  return (
    <div ref={scrollRef} className="flex-1 overflow-y-auto px-4 pb-4">
      {isLoading && (
        <div className="flex h-full items-center justify-center text-neutral-500">
          <Spinner size={24} />
        </div>
      )}

      {!isLoading && photos.length === 0 && (
        <div className="flex h-full flex-col items-center justify-center gap-3 text-neutral-600">
          <Images size={40} />
          <p className="text-sm">No photos match the current filters.</p>
        </div>
      )}

      {!isLoading && photos.length > 0 && (
        <div style={{ height: rowVirtualizer.getTotalSize(), position: 'relative' }}>
          {virtualRows.map((virtualRow) => {
            const row = rows[virtualRow.index]
            if (!row) return null

            if (row.type === 'divider') {
              return (
                <button
                  key={virtualRow.key}
                  type="button"
                  onClick={() => toggleRollCollapsed(row.filmRollId)}
                  style={{
                    position: 'absolute',
                    top: 0,
                    left: 0,
                    width: '100%',
                    height: virtualRow.size,
                    transform: `translateY(${virtualRow.start}px)`,
                  }}
                  className="flex w-full items-center gap-2 pb-1.5 text-left"
                  aria-expanded={!row.collapsed}
                >
                  <ChevronDown
                    size={14}
                    className={cn('shrink-0 text-neutral-500 transition-transform', row.collapsed && '-rotate-90')}
                  />
                  <h3 className="whitespace-nowrap text-xs font-semibold uppercase tracking-wide text-neutral-500">
                    {row.label}
                  </h3>
                  {row.collapsed && (
                    <span className="whitespace-nowrap text-xs text-neutral-600">
                      {row.count} photo{row.count === 1 ? '' : 's'}
                    </span>
                  )}
                  <div className="h-px flex-1 bg-neutral-800" />
                </button>
              )
            }

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
                {row.items.map((photo) => (
                  <PhotoCard
                    key={photo.id}
                    photo={photo}
                    size={cardSize}
                    onClick={() => onSelectPhoto(photo.id)}
                    selected={selectedIds.has(photo.id)}
                    selectionMode={selectedIds.size > 0}
                    onToggleSelect={() => onToggleSelect(photo.id)}
                  />
                ))}
              </div>
            )
          })}
        </div>
      )}

      {isFetchingNextPage && (
        <div className="flex justify-center py-4 text-neutral-500">
          <Spinner size={18} />
        </div>
      )}
    </div>
  )
}

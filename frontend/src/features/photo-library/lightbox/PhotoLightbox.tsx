import { useEffect, useState, type CSSProperties } from 'react'
import { ChevronLeft, ChevronRight, Download, RotateCcw, RotateCw, X } from 'lucide-react'
import { usePhotoDetail, useUpdatePhotoRotation } from '@/features/photo-library/hooks/usePhotoDetail'
import { ExifTab } from '@/features/photo-library/lightbox/ExifTab'
import { CritiqueTab } from '@/features/photo-library/lightbox/CritiqueTab'
import { OrganizeTab } from '@/features/photo-library/lightbox/OrganizeTab'
import { Tabs } from '@/components/ui/Tabs'
import { Spinner } from '@/components/ui/Spinner'

interface PhotoLightboxProps {
  photoId: string | null
  /** Ordered ids of the photos currently loaded in the library grid, for prev/next navigation. */
  photoIds: string[]
  hasNextPage: boolean
  onFetchNextPage: () => void
  onNavigate: (photoId: string) => void
  onClose: () => void
}

const TAB_ITEMS = [
  { key: 'exif', label: 'Gear & EXIF' },
  { key: 'critique', label: 'AI Critique' },
  { key: 'organize', label: 'Organize' },
]

export function PhotoLightbox({ photoId, photoIds, hasNextPage, onFetchNextPage, onNavigate, onClose }: PhotoLightboxProps) {
  const [activeTab, setActiveTab] = useState('exif')
  const { data: photo, isLoading } = usePhotoDetail(photoId)
  const updateRotation = useUpdatePhotoRotation(photoId)

  const currentIndex = photoId ? photoIds.indexOf(photoId) : -1
  const prevId = currentIndex > 0 ? photoIds[currentIndex - 1] : null
  const nextId = currentIndex >= 0 && currentIndex < photoIds.length - 1 ? photoIds[currentIndex + 1] : null

  // Clicking "next" on the last loaded photo should feel seamless even though the next page
  // hasn't arrived yet — kick off the fetch and land on the first newly-loaded photo once it does,
  // rather than making the user click twice.
  const [awaitingNextPage, setAwaitingNextPage] = useState(false)

  useEffect(() => {
    if (!awaitingNextPage) return
    const idx = photoId ? photoIds.indexOf(photoId) : -1
    if (idx >= 0 && idx < photoIds.length - 1) {
      onNavigate(photoIds[idx + 1])
      setAwaitingNextPage(false)
    }
  }, [awaitingNextPage, photoIds, photoId, onNavigate])

  const goToPrev = () => {
    if (prevId) onNavigate(prevId)
  }

  const goToNext = () => {
    if (nextId) {
      onNavigate(nextId)
    } else if (hasNextPage) {
      setAwaitingNextPage(true)
      onFetchNextPage()
    }
  }

  useEffect(() => {
    if (!photoId) return
    const onKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose()
      if (e.key === 'ArrowLeft') goToPrev()
      if (e.key === 'ArrowRight') goToNext()
    }
    window.addEventListener('keydown', onKeyDown)
    return () => window.removeEventListener('keydown', onKeyDown)
  }, [photoId, onClose, prevId, nextId, hasNextPage, onNavigate, onFetchNextPage])

  if (!photoId) return null

  const rotation = photo?.rotationDegrees ?? 0
  const rotateBy = (delta: number) => updateRotation.mutate((rotation + delta + 360) % 360)
  // A 90/270 rotation swaps which axis the image's un-rotated box needs to fit into. Sized via CSS
  // container query units (resolved during layout, no JS measurement step) rather than a
  // ResizeObserver — a rotated image's effective footprint depends on the *container's* box, which
  // plain content-based max-width/max-height can't express on their own.
  const isSideways = rotation % 180 !== 0
  const imageStyle: CSSProperties = {
    transform: `rotate(${rotation}deg)`,
    maxWidth: isSideways ? '100cqh' : '100cqw',
    maxHeight: isSideways ? '100cqw' : '100cqh',
  }

  return (
    <div className="fixed inset-0 z-50 flex bg-black/95">
      <div
        className="relative flex flex-1 items-center justify-center overflow-hidden p-6"
        style={{ containerType: 'size' }}
      >
        <div className="absolute right-4 top-4 z-10 flex items-center gap-2">
          {photo && (
            <>
              <button
                type="button"
                onClick={() => rotateBy(-90)}
                disabled={updateRotation.isPending}
                className="rounded-full bg-black/50 p-2 text-white hover:bg-black/70 disabled:opacity-50"
                aria-label="Rotate left"
              >
                <RotateCcw size={18} />
              </button>
              <button
                type="button"
                onClick={() => rotateBy(90)}
                disabled={updateRotation.isPending}
                className="rounded-full bg-black/50 p-2 text-white hover:bg-black/70 disabled:opacity-50"
                aria-label="Rotate right"
              >
                <RotateCw size={18} />
              </button>
              <a
                href={photo.downloadUrl}
                className="rounded-full bg-black/50 p-2 text-white hover:bg-black/70"
                aria-label="Download original"
              >
                <Download size={18} />
              </a>
            </>
          )}
          <button
            type="button"
            onClick={onClose}
            className="rounded-full bg-black/50 p-2 text-white hover:bg-black/70"
            aria-label="Close"
          >
            <X size={20} />
          </button>
        </div>

        {prevId && (
          <button
            type="button"
            onClick={goToPrev}
            className="absolute left-4 top-1/2 z-10 -translate-y-1/2 rounded-full bg-black/50 p-2 text-white hover:bg-black/70"
            aria-label="Previous photo"
          >
            <ChevronLeft size={22} />
          </button>
        )}

        {(nextId || hasNextPage) && (
          <button
            type="button"
            onClick={goToNext}
            disabled={awaitingNextPage}
            className="absolute right-4 top-1/2 z-10 -translate-y-1/2 rounded-full bg-black/50 p-2 text-white hover:bg-black/70 disabled:opacity-50"
            aria-label="Next photo"
          >
            <ChevronRight size={22} />
          </button>
        )}

        {isLoading && <Spinner size={28} className="text-neutral-400" />}
        {photo && (
          <img src={photo.previewUrl ?? photo.originalUrl ?? undefined} alt="" style={imageStyle} className="object-contain" />
        )}
      </div>

      <div className="flex w-96 shrink-0 flex-col border-l border-neutral-800 bg-neutral-950">
        <Tabs items={TAB_ITEMS} active={activeTab} onChange={setActiveTab} />
        <div className="flex-1 overflow-y-auto px-4 py-3">
          {!photo && !isLoading && <p className="text-sm text-neutral-500">Photo not found.</p>}
          {photo && activeTab === 'exif' && <ExifTab photo={photo} />}
          {photo && activeTab === 'critique' && <CritiqueTab photo={photo} />}
          {photo && activeTab === 'organize' && <OrganizeTab photo={photo} onDeleted={onClose} />}
        </div>
      </div>
    </div>
  )
}

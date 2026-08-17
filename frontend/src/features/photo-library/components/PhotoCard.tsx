import { useMemo, useState } from 'react'
import { AlertTriangle, Loader2, Star } from 'lucide-react'
import { blurHashToDataUrl } from '@/lib/blurhash'
import { cn } from '@/lib/cn'
import type { PhotoListItem } from '@/types/photo'

interface PhotoCardProps {
  photo: PhotoListItem
  size: number
  onClick: () => void
}

export function PhotoCard({ photo, size, onClick }: PhotoCardProps) {
  const [loaded, setLoaded] = useState(false)
  const placeholder = useMemo(
    () => (photo.blurHash ? blurHashToDataUrl(photo.blurHash) : ''),
    [photo.blurHash],
  )

  const isProcessing = photo.processingStatus === 'Processing' || photo.processingStatus === 'Uploaded'
  const isFailed = photo.processingStatus === 'Failed'

  return (
    <button
      type="button"
      onClick={onClick}
      style={{ width: size, height: size }}
      className="group relative overflow-hidden rounded-md bg-neutral-900 focus:outline-none focus-visible:ring-2 focus-visible:ring-white"
    >
      {placeholder && (
        <img
          src={placeholder}
          alt=""
          aria-hidden
          className={cn('absolute inset-0 h-full w-full object-cover transition-opacity', loaded ? 'opacity-0' : 'opacity-100')}
        />
      )}

      {photo.thumbnailUrl && (
        <img
          src={photo.thumbnailUrl}
          alt=""
          loading="lazy"
          onLoad={() => setLoaded(true)}
          className={cn('absolute inset-0 h-full w-full object-cover transition-opacity duration-300', loaded ? 'opacity-100' : 'opacity-0')}
        />
      )}

      {(isProcessing || isFailed) && (
        <div className="absolute inset-0 flex items-center justify-center bg-black/40">
          {isProcessing ? (
            <Loader2 size={20} className="animate-spin text-white/80" />
          ) : (
            <AlertTriangle size={20} className="text-amber-400" />
          )}
        </div>
      )}

      {photo.rating > 0 && (
        <div className="absolute bottom-1.5 left-1.5 flex items-center gap-0.5 rounded bg-black/60 px-1.5 py-0.5 opacity-0 transition-opacity group-hover:opacity-100">
          <Star size={11} className="fill-amber-400 text-amber-400" />
          <span className="text-[11px] font-medium text-white">{photo.rating}</span>
        </div>
      )}
    </button>
  )
}

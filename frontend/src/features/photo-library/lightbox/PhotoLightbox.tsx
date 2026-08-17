import { useEffect, useState } from 'react'
import { X } from 'lucide-react'
import { usePhotoDetail } from '@/features/photo-library/hooks/usePhotoDetail'
import { ExifTab } from '@/features/photo-library/lightbox/ExifTab'
import { CritiqueTab } from '@/features/photo-library/lightbox/CritiqueTab'
import { OrganizeTab } from '@/features/photo-library/lightbox/OrganizeTab'
import { Tabs } from '@/components/ui/Tabs'
import { Spinner } from '@/components/ui/Spinner'

interface PhotoLightboxProps {
  photoId: string | null
  onClose: () => void
}

const TAB_ITEMS = [
  { key: 'exif', label: 'Gear & EXIF' },
  { key: 'critique', label: 'AI Critique' },
  { key: 'organize', label: 'Organize' },
]

export function PhotoLightbox({ photoId, onClose }: PhotoLightboxProps) {
  const [activeTab, setActiveTab] = useState('exif')
  const { data: photo, isLoading } = usePhotoDetail(photoId)

  useEffect(() => {
    if (!photoId) return
    const onKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose()
    }
    window.addEventListener('keydown', onKeyDown)
    return () => window.removeEventListener('keydown', onKeyDown)
  }, [photoId, onClose])

  if (!photoId) return null

  return (
    <div className="fixed inset-0 z-50 flex bg-black/95">
      <button
        type="button"
        onClick={onClose}
        className="absolute right-4 top-4 z-10 rounded-full bg-black/50 p-2 text-white hover:bg-black/70"
        aria-label="Close"
      >
        <X size={20} />
      </button>

      <div className="flex flex-1 items-center justify-center overflow-hidden p-6">
        {isLoading && <Spinner size={28} className="text-neutral-400" />}
        {photo && (
          <img
            src={photo.previewUrl ?? photo.originalUrl ?? undefined}
            alt=""
            className="max-h-full max-w-full object-contain"
          />
        )}
      </div>

      <div className="flex w-96 shrink-0 flex-col border-l border-neutral-800 bg-neutral-950">
        <Tabs items={TAB_ITEMS} active={activeTab} onChange={setActiveTab} />
        <div className="flex-1 overflow-y-auto px-4 py-3">
          {!photo && !isLoading && <p className="text-sm text-neutral-500">Photo not found.</p>}
          {photo && activeTab === 'exif' && <ExifTab photo={photo} />}
          {photo && activeTab === 'critique' && <CritiqueTab photo={photo} />}
          {photo && activeTab === 'organize' && <OrganizeTab photo={photo} />}
        </div>
      </div>
    </div>
  )
}

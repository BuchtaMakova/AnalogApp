import { useState } from 'react'
import { Upload } from 'lucide-react'
import { usePhotos } from '@/features/photo-library/hooks/usePhotos'
import { PhotoGrid } from '@/features/photo-library/components/PhotoGrid'
import { FilterBar } from '@/features/photo-library/components/FilterBar'
import { UploadDropzone } from '@/features/photo-library/components/UploadDropzone'
import { PhotoLightbox } from '@/features/photo-library/lightbox/PhotoLightbox'
import { Button } from '@/components/ui/Button'
import type { GetPhotosFilters } from '@/types/photo'

export function PhotoLibraryPage() {
  const [filters, setFilters] = useState<GetPhotosFilters>({})
  const [uploadOpen, setUploadOpen] = useState(false)
  const [selectedPhotoId, setSelectedPhotoId] = useState<string | null>(null)

  const { photos, totalCount, isLoading, hasNextPage, isFetchingNextPage, fetchNextPage } = usePhotos(filters)

  return (
    <div className="flex h-full flex-col">
      <div className="flex items-center justify-between px-6 pt-6 pb-2">
        <div>
          <h2 className="text-xl font-semibold text-white">Photo Library</h2>
          <p className="mt-1 text-sm text-neutral-400">{totalCount} photos</p>
        </div>
        <Button variant="primary" onClick={() => setUploadOpen(true)}>
          <Upload size={16} />
          Upload scans
        </Button>
      </div>

      <FilterBar filters={filters} onChange={setFilters} />

      <PhotoGrid
        photos={photos}
        isLoading={isLoading}
        hasNextPage={hasNextPage}
        isFetchingNextPage={isFetchingNextPage}
        onFetchNextPage={fetchNextPage}
        onSelectPhoto={setSelectedPhotoId}
      />

      <UploadDropzone open={uploadOpen} onClose={() => setUploadOpen(false)} />

      <PhotoLightbox photoId={selectedPhotoId} onClose={() => setSelectedPhotoId(null)} />
    </div>
  )
}

import { useMemo, useState } from 'react'
import { Download, Trash2, Upload, X } from 'lucide-react'
import {
  useBulkDeletePhotos,
  useDownloadPhotosAsZip,
  useDownloadPhotosSeparately,
  useLibraryFilterOptions,
  usePhotos,
} from '@/features/photo-library/hooks/usePhotos'
import { PhotoGrid } from '@/features/photo-library/components/PhotoGrid'
import { FilterBar } from '@/features/photo-library/components/FilterBar'
import { UploadDropzone } from '@/features/photo-library/components/UploadDropzone'
import { DownloadSelectionDialog } from '@/features/photo-library/components/DownloadSelectionDialog'
import { PhotoLightbox } from '@/features/photo-library/lightbox/PhotoLightbox'
import { Button } from '@/components/ui/Button'
import type { GetPhotosFilters } from '@/types/photo'

export function PhotoLibraryPage() {
  const [filters, setFilters] = useState<GetPhotosFilters>({})
  const [uploadOpen, setUploadOpen] = useState(false)
  const [downloadDialogOpen, setDownloadDialogOpen] = useState(false)
  const [selectedPhotoId, setSelectedPhotoId] = useState<string | null>(null)
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set())

  const { photos, totalCount, isLoading, hasNextPage, isFetchingNextPage, fetchNextPage } = usePhotos(filters)
  const bulkDelete = useBulkDeletePhotos()
  const downloadSeparately = useDownloadPhotosSeparately()
  const downloadZip = useDownloadPhotosAsZip()
  // Shares the ['film-rolls', 'all'] query FilterBar already fetches, so this is a cache hit, not
  // an extra request.
  const { filmRolls } = useLibraryFilterOptions()
  const filmRollNames = useMemo(
    () => new Map(filmRolls.data?.map((roll) => [roll.id, roll.name]) ?? []),
    [filmRolls.data],
  )

  const toggleSelect = (photoId: string) => {
    setSelectedIds((prev) => {
      const next = new Set(prev)
      if (next.has(photoId)) next.delete(photoId)
      else next.add(photoId)
      return next
    })
  }

  const clearSelection = () => setSelectedIds(new Set())

  const deleteSelected = () => {
    if (!confirm(`Delete ${selectedIds.size} photo${selectedIds.size === 1 ? '' : 's'}? This cannot be undone.`)) {
      return
    }
    bulkDelete.mutate([...selectedIds], { onSuccess: () => clearSelection() })
  }

  return (
    <div className="flex h-full flex-col">
      <div className="flex items-center justify-between px-6 pt-6 pb-2">
        {selectedIds.size > 0 ? (
          <>
            <div className="flex items-center gap-3">
              <button
                type="button"
                onClick={clearSelection}
                className="rounded-md p-1.5 text-neutral-400 hover:bg-neutral-800 hover:text-white"
                aria-label="Clear selection"
              >
                <X size={18} />
              </button>
              <p className="text-sm font-medium text-white">{selectedIds.size} selected</p>
            </div>
            <div className="flex items-center gap-2">
              <Button variant="secondary" onClick={() => setDownloadDialogOpen(true)}>
                <Download size={16} />
                Download selected
              </Button>
              <Button variant="danger" onClick={deleteSelected} disabled={bulkDelete.isPending}>
                <Trash2 size={16} />
                Delete selected
              </Button>
            </div>
          </>
        ) : (
          <>
            <div>
              <h2 className="text-xl font-semibold text-white">Photo Library</h2>
              <p className="mt-1 text-sm text-neutral-400">{totalCount} photos</p>
            </div>
            <Button variant="primary" onClick={() => setUploadOpen(true)}>
              <Upload size={16} />
              Upload scans
            </Button>
          </>
        )}
      </div>

      <FilterBar filters={filters} onChange={setFilters} />

      <PhotoGrid
        photos={photos}
        filmRollNames={filmRollNames}
        isLoading={isLoading}
        hasNextPage={hasNextPage}
        isFetchingNextPage={isFetchingNextPage}
        onFetchNextPage={fetchNextPage}
        onSelectPhoto={setSelectedPhotoId}
        selectedIds={selectedIds}
        onToggleSelect={toggleSelect}
      />

      <UploadDropzone open={uploadOpen} onClose={() => setUploadOpen(false)} />

      <DownloadSelectionDialog
        open={downloadDialogOpen}
        count={selectedIds.size}
        onClose={() => setDownloadDialogOpen(false)}
        isDownloading={downloadSeparately.isPending || downloadZip.isPending}
        onDownloadSeparately={() =>
          downloadSeparately.mutate([...selectedIds], { onSuccess: () => setDownloadDialogOpen(false) })
        }
        onDownloadZip={() => downloadZip.mutate([...selectedIds], { onSuccess: () => setDownloadDialogOpen(false) })}
      />

      <PhotoLightbox
        photoId={selectedPhotoId}
        photoIds={photos.map((p) => p.id)}
        hasNextPage={hasNextPage}
        onFetchNextPage={fetchNextPage}
        onNavigate={setSelectedPhotoId}
        onClose={() => setSelectedPhotoId(null)}
      />
    </div>
  )
}

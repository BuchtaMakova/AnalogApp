import { useInfiniteQuery, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useMemo } from 'react'
import { deletePhoto, downloadPhotosAsZip, getPhotoDownloadUrls, getPhotos } from '@/api/photos'
import { listCameraBodies, listFilmRolls, listLenses } from '@/api/gear'
import { getTags } from '@/api/tags'
import { downloadUrlsSequentially, triggerBlobDownload } from '@/lib/download'
import type { GetPhotosFilters } from '@/types/photo'

const PAGE_SIZE = 60

export function usePhotos(filters: GetPhotosFilters) {
  const query = useInfiniteQuery({
    queryKey: ['photos', filters],
    queryFn: ({ pageParam }) => getPhotos(filters, pageParam, PAGE_SIZE),
    initialPageParam: 1,
    getNextPageParam: (lastPage) => (lastPage.hasMore ? lastPage.page + 1 : undefined),
    // Background derivative generation (thumbnail/preview/BlurHash) finishes a few seconds after
    // upload, but nothing else tells the client — without this, a photo's "Processing" spinner
    // sits there until some unrelated refetch happens to run (tab refocus, another mutation, ...).
    // Stops polling once nothing is still Processing/Uploaded, so it's not a permanent 2s tax.
    refetchInterval: (query) => {
      const pages = query.state.data?.pages ?? []
      const stillProcessing = pages.some((page) =>
        page.items.some((p) => p.processingStatus === 'Processing' || p.processingStatus === 'Uploaded'),
      )
      return stillProcessing ? 2000 : false
    },
  })

  const photos = useMemo(() => query.data?.pages.flatMap((page) => page.items) ?? [], [query.data])
  const totalCount = query.data?.pages[0]?.totalCount ?? 0

  return { ...query, photos, totalCount }
}

/** Deletes each selected photo individually (no bulk endpoint) and reports which ids failed. */
export function useBulkDeletePhotos() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (photoIds: string[]) => {
      const results = await Promise.allSettled(photoIds.map((id) => deletePhoto(id)))
      const failedIds = photoIds.filter((_, i) => results[i].status === 'rejected')
      return { failedIds }
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: ['photos'] })
      queryClient.invalidateQueries({ queryKey: ['albums'] })
    },
  })
}

/** Downloads each selected photo as its own file, via presigned URLs (no bytes flow through our API). */
export function useDownloadPhotosSeparately() {
  return useMutation({
    mutationFn: async (photoIds: string[]) => {
      const links = await getPhotoDownloadUrls(photoIds)
      await downloadUrlsSequentially(links.map((link) => link.url))
    },
  })
}

/** Downloads the selected photos bundled into a single ZIP. */
export function useDownloadPhotosAsZip() {
  return useMutation({
    mutationFn: async (photoIds: string[]) => {
      const blob = await downloadPhotosAsZip(photoIds)
      triggerBlobDownload(blob, 'analoghub-photos.zip')
    },
  })
}

/** Shared lookups for the filter bar and upload metadata pickers. */
export function useLibraryFilterOptions() {
  const filmRolls = useQuery({ queryKey: ['film-rolls', 'all'], queryFn: () => listFilmRolls() })
  const cameraBodies = useQuery({ queryKey: ['camera-bodies'], queryFn: () => listCameraBodies() })
  const lenses = useQuery({ queryKey: ['lenses'], queryFn: () => listLenses() })
  const tags = useQuery({ queryKey: ['tags'], queryFn: getTags })

  return { filmRolls, cameraBodies, lenses, tags }
}

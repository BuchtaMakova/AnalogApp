import { useInfiniteQuery, useQuery } from '@tanstack/react-query'
import { useMemo } from 'react'
import { getPhotos } from '@/api/photos'
import { listCameraBodies, listFilmRolls, listLenses } from '@/api/gear'
import { getTags } from '@/api/tags'
import type { GetPhotosFilters } from '@/types/photo'

const PAGE_SIZE = 60

export function usePhotos(filters: GetPhotosFilters) {
  const query = useInfiniteQuery({
    queryKey: ['photos', filters],
    queryFn: ({ pageParam }) => getPhotos(filters, pageParam, PAGE_SIZE),
    initialPageParam: 1,
    getNextPageParam: (lastPage) => (lastPage.hasMore ? lastPage.page + 1 : undefined),
  })

  const photos = useMemo(() => query.data?.pages.flatMap((page) => page.items) ?? [], [query.data])
  const totalCount = query.data?.pages[0]?.totalCount ?? 0

  return { ...query, photos, totalCount }
}

/** Shared lookups for the filter bar and upload metadata pickers. */
export function useLibraryFilterOptions() {
  const filmRolls = useQuery({ queryKey: ['film-rolls', 'all'], queryFn: () => listFilmRolls() })
  const cameraBodies = useQuery({ queryKey: ['camera-bodies'], queryFn: () => listCameraBodies() })
  const lenses = useQuery({ queryKey: ['lenses'], queryFn: () => listLenses() })
  const tags = useQuery({ queryKey: ['tags'], queryFn: getTags })

  return { filmRolls, cameraBodies, lenses, tags }
}

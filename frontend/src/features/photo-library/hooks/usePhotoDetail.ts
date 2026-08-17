import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { addPhotoTag, analyzePhoto, getPhotoById, removePhotoTag, updatePhotoRating } from '@/api/photos'

export function usePhotoDetail(photoId: string | null) {
  return useQuery({
    queryKey: ['photo', photoId],
    queryFn: () => getPhotoById(photoId as string),
    enabled: !!photoId,
  })
}

export function useAnalyzePhoto(photoId: string | null) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: () => analyzePhoto(photoId as string),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['photo', photoId] })
      queryClient.invalidateQueries({ queryKey: ['photos'] })
      queryClient.invalidateQueries({ queryKey: ['tags'] })
    },
  })
}

export function useUpdatePhotoRating(photoId: string | null) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (rating: number) => updatePhotoRating(photoId as string, rating),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['photo', photoId] })
      queryClient.invalidateQueries({ queryKey: ['photos'] })
    },
  })
}

export function usePhotoTagMutations(photoId: string | null) {
  const queryClient = useQueryClient()

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ['photo', photoId] })
    queryClient.invalidateQueries({ queryKey: ['photos'] })
    queryClient.invalidateQueries({ queryKey: ['tags'] })
  }

  const addTag = useMutation({
    mutationFn: (tagName: string) => addPhotoTag(photoId as string, tagName),
    onSuccess: invalidate,
  })

  const removeTag = useMutation({
    mutationFn: (tagId: string) => removePhotoTag(photoId as string, tagId),
    onSuccess: invalidate,
  })

  return { addTag, removeTag }
}

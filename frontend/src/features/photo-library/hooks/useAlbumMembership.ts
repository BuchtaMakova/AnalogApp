import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { addPhotoToAlbum, getAlbums, removePhotoFromAlbum } from '@/api/albums'

export function useAlbums() {
  return useQuery({ queryKey: ['albums'], queryFn: getAlbums })
}

export function useAlbumMembership(photoId: string | null) {
  const queryClient = useQueryClient()

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ['photo', photoId] })
    queryClient.invalidateQueries({ queryKey: ['albums'] })
  }

  const addToAlbum = useMutation({
    mutationFn: (albumId: string) => addPhotoToAlbum(albumId, photoId as string),
    onSuccess: invalidate,
  })

  const removeFromAlbum = useMutation({
    mutationFn: (albumId: string) => removePhotoFromAlbum(albumId, photoId as string),
    onSuccess: invalidate,
  })

  return { addToAlbum, removeFromAlbum }
}

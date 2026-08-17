import type { PhotoListItem } from '@/types/photo'

export interface Album {
  id: string
  name: string
  description: string | null
  coverPhotoId: string | null
  coverPhotoThumbnailUrl: string | null
  photoCount: number
  createdAtUtc: string
}

export interface AlbumDetail {
  id: string
  name: string
  description: string | null
  coverPhotoId: string | null
  createdAtUtc: string
  photos: PhotoListItem[]
}

export interface CreateAlbumPayload {
  name: string
  description?: string | null
}

export interface UpdateAlbumPayload {
  id: string
  name: string
  description?: string | null
  coverPhotoId?: string | null
}

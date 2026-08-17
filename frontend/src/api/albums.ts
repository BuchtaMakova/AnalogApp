import { apiClient } from '@/api/client'
import type { Album, AlbumDetail, CreateAlbumPayload, UpdateAlbumPayload } from '@/types/album'

export async function getAlbums(): Promise<Album[]> {
  const { data } = await apiClient.get<Album[]>('/albums')
  return data
}

export async function getAlbumById(id: string): Promise<AlbumDetail> {
  const { data } = await apiClient.get<AlbumDetail>(`/albums/${id}`)
  return data
}

export async function createAlbum(payload: CreateAlbumPayload): Promise<Album> {
  const { data } = await apiClient.post<Album>('/albums', payload)
  return data
}

export async function updateAlbum(payload: UpdateAlbumPayload): Promise<Album> {
  const { data } = await apiClient.put<Album>(`/albums/${payload.id}`, payload)
  return data
}

export async function deleteAlbum(id: string): Promise<void> {
  await apiClient.delete(`/albums/${id}`)
}

export async function addPhotoToAlbum(albumId: string, photoId: string): Promise<void> {
  await apiClient.post(`/albums/${albumId}/photos/${photoId}`)
}

export async function removePhotoFromAlbum(albumId: string, photoId: string): Promise<void> {
  await apiClient.delete(`/albums/${albumId}/photos/${photoId}`)
}

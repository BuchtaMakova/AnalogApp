import axios from 'axios'
import { apiClient } from '@/api/client'
import type {
  AnalyzePhotoResult,
  GetPhotosFilters,
  PagedResult,
  Photo,
  PhotoDetail,
  PhotoDownloadLink,
  PhotoListItem,
  PhotoTag,
  RegisterPhotoPayload,
  RequestPhotoUploadUrlPayload,
  RequestPhotoUploadUrlResult,
} from '@/types/photo'

export async function requestPhotoUploadUrl(
  payload: RequestPhotoUploadUrlPayload,
): Promise<RequestPhotoUploadUrlResult> {
  const { data } = await apiClient.post<RequestPhotoUploadUrlResult>('/photos/upload-url', payload)
  return data
}

export async function registerPhoto(payload: RegisterPhotoPayload): Promise<Photo> {
  const { data } = await apiClient.post<Photo>('/photos', payload)
  return data
}

/**
 * Full upload flow: request a presigned URL, stream the file straight to storage, then register
 * the photo so the backend can enqueue thumbnail/BlurHash generation.
 */
export async function uploadAndRegisterPhoto(
  file: File,
  filmRollId: string,
  metadata: Omit<RegisterPhotoPayload, 'filmRollId' | 'storageKey' | 'contentType' | 'fileSizeBytes'>,
  onUploadProgress?: (percent: number) => void,
): Promise<Photo> {
  const { uploadUrl, storageKey } = await requestPhotoUploadUrl({
    filmRollId,
    originalFileName: file.name,
    contentType: file.type,
  })

  await axios.put(uploadUrl, file, {
    headers: { 'Content-Type': file.type },
    onUploadProgress: (event) => {
      if (onUploadProgress && event.total) {
        onUploadProgress(Math.round((event.loaded / event.total) * 100))
      }
    },
  })

  return registerPhoto({
    filmRollId,
    storageKey,
    contentType: file.type,
    fileSizeBytes: file.size,
    ...metadata,
  })
}

export async function getPhotos(
  filters: GetPhotosFilters,
  page: number,
  pageSize = 60,
): Promise<PagedResult<PhotoListItem>> {
  const { data } = await apiClient.get<PagedResult<PhotoListItem>>('/photos', {
    params: { ...filters, page, pageSize },
  })
  return data
}

export async function getPhotoById(id: string): Promise<PhotoDetail> {
  const { data } = await apiClient.get<PhotoDetail>(`/photos/${id}`)
  return data
}

export async function analyzePhoto(id: string): Promise<AnalyzePhotoResult> {
  const { data } = await apiClient.post<AnalyzePhotoResult>(`/photos/${id}/analyze`)
  return data
}

export async function updatePhotoRating(id: string, rating: number): Promise<void> {
  await apiClient.put(`/photos/${id}/rating`, { rating })
}

export async function updatePhotoRotation(id: string, rotationDegrees: number): Promise<void> {
  await apiClient.put(`/photos/${id}/rotation`, { rotationDegrees })
}

export async function getPhotoDownloadUrls(photoIds: string[]): Promise<PhotoDownloadLink[]> {
  const { data } = await apiClient.post<PhotoDownloadLink[]>('/photos/download-urls', { photoIds })
  return data
}

/** Fetches the ZIP as a blob (needs the Authorization header, so a plain link/navigation won't work). */
export async function downloadPhotosAsZip(photoIds: string[]): Promise<Blob> {
  const { data } = await apiClient.post('/photos/download-zip', { photoIds }, { responseType: 'blob' })
  return data
}

export async function addPhotoTag(id: string, tagName: string): Promise<PhotoTag> {
  const { data } = await apiClient.post<PhotoTag>(`/photos/${id}/tags`, { tagName })
  return data
}

export async function removePhotoTag(id: string, tagId: string): Promise<void> {
  await apiClient.delete(`/photos/${id}/tags/${tagId}`)
}

export async function deletePhoto(id: string): Promise<void> {
  await apiClient.delete(`/photos/${id}`)
}

import { apiClient } from '@/api/client'
import type {
  CameraBody,
  CreateCameraBodyPayload,
  CreateFilmRollPayload,
  CreateFlashPayload,
  CreateLensPayload,
  FilmRoll,
  FilmRollStatus,
  Flash,
  Lens,
  UpdateCameraBodyPayload,
  UpdateFilmRollPayload,
  UpdateFlashPayload,
  UpdateLensPayload,
} from '@/types/gear'

export async function listCameraBodies(includeInactive = false): Promise<CameraBody[]> {
  const { data } = await apiClient.get<CameraBody[]>('/gear/camera-bodies', { params: { includeInactive } })
  return data
}

export async function createCameraBody(payload: CreateCameraBodyPayload): Promise<CameraBody> {
  const { data } = await apiClient.post<CameraBody>('/gear/camera-bodies', payload)
  return data
}

export async function updateCameraBody(payload: UpdateCameraBodyPayload): Promise<CameraBody> {
  const { data } = await apiClient.put<CameraBody>(`/gear/camera-bodies/${payload.id}`, payload)
  return data
}

export async function deleteCameraBody(id: string): Promise<void> {
  await apiClient.delete(`/gear/camera-bodies/${id}`)
}

export async function listLenses(includeInactive = false): Promise<Lens[]> {
  const { data } = await apiClient.get<Lens[]>('/gear/lenses', { params: { includeInactive } })
  return data
}

export async function createLens(payload: CreateLensPayload): Promise<Lens> {
  const { data } = await apiClient.post<Lens>('/gear/lenses', payload)
  return data
}

export async function updateLens(payload: UpdateLensPayload): Promise<Lens> {
  const { data } = await apiClient.put<Lens>(`/gear/lenses/${payload.id}`, payload)
  return data
}

export async function deleteLens(id: string): Promise<void> {
  await apiClient.delete(`/gear/lenses/${id}`)
}

export async function listFlashes(includeInactive = false): Promise<Flash[]> {
  const { data } = await apiClient.get<Flash[]>('/gear/flashes', { params: { includeInactive } })
  return data
}

export async function createFlash(payload: CreateFlashPayload): Promise<Flash> {
  const { data } = await apiClient.post<Flash>('/gear/flashes', payload)
  return data
}

export async function updateFlash(payload: UpdateFlashPayload): Promise<Flash> {
  const { data } = await apiClient.put<Flash>(`/gear/flashes/${payload.id}`, payload)
  return data
}

export async function deleteFlash(id: string): Promise<void> {
  await apiClient.delete(`/gear/flashes/${id}`)
}

export async function listFilmRolls(status?: FilmRollStatus): Promise<FilmRoll[]> {
  const { data } = await apiClient.get<FilmRoll[]>('/film-rolls', { params: { status } })
  return data
}

export async function createFilmRoll(payload: CreateFilmRollPayload): Promise<FilmRoll> {
  const { data } = await apiClient.post<FilmRoll>('/film-rolls', payload)
  return data
}

export async function updateFilmRoll(payload: UpdateFilmRollPayload): Promise<FilmRoll> {
  const { data } = await apiClient.put<FilmRoll>(`/film-rolls/${payload.id}`, payload)
  return data
}

export async function deleteFilmRoll(id: string): Promise<void> {
  await apiClient.delete(`/film-rolls/${id}`)
}

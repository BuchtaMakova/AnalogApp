import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  createCameraBody,
  createFlash,
  createLens,
  deleteCameraBody,
  deleteFlash,
  deleteLens,
  listCameraBodies,
  listFlashes,
  listLenses,
  updateCameraBody,
  updateFlash,
  updateLens,
} from '@/api/gear'
import type {
  CreateCameraBodyPayload,
  CreateFlashPayload,
  CreateLensPayload,
  UpdateCameraBodyPayload,
  UpdateFlashPayload,
  UpdateLensPayload,
} from '@/types/gear'

export function useCameraBodies() {
  const queryClient = useQueryClient()
  const query = useQuery({ queryKey: ['camera-bodies', 'all'], queryFn: () => listCameraBodies(true) })

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['camera-bodies'] })

  const create = useMutation({
    mutationFn: (payload: CreateCameraBodyPayload) => createCameraBody(payload),
    onSuccess: invalidate,
  })
  const update = useMutation({
    mutationFn: (payload: UpdateCameraBodyPayload) => updateCameraBody(payload),
    onSuccess: invalidate,
  })
  const remove = useMutation({
    mutationFn: (id: string) => deleteCameraBody(id),
    onSuccess: invalidate,
  })

  return { ...query, create, update, remove }
}

export function useLenses() {
  const queryClient = useQueryClient()
  const query = useQuery({ queryKey: ['lenses', 'all'], queryFn: () => listLenses(true) })

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['lenses'] })

  const create = useMutation({
    mutationFn: (payload: CreateLensPayload) => createLens(payload),
    onSuccess: invalidate,
  })
  const update = useMutation({
    mutationFn: (payload: UpdateLensPayload) => updateLens(payload),
    onSuccess: invalidate,
  })
  const remove = useMutation({
    mutationFn: (id: string) => deleteLens(id),
    onSuccess: invalidate,
  })

  return { ...query, create, update, remove }
}

export function useFlashes() {
  const queryClient = useQueryClient()
  const query = useQuery({ queryKey: ['flashes', 'all'], queryFn: () => listFlashes(true) })

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['flashes'] })

  const create = useMutation({
    mutationFn: (payload: CreateFlashPayload) => createFlash(payload),
    onSuccess: invalidate,
  })
  const update = useMutation({
    mutationFn: (payload: UpdateFlashPayload) => updateFlash(payload),
    onSuccess: invalidate,
  })
  const remove = useMutation({
    mutationFn: (id: string) => deleteFlash(id),
    onSuccess: invalidate,
  })

  return { ...query, create, update, remove }
}

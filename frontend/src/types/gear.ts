export type FilmFormat = 'ThirtyFiveMm' | 'OneTwentyMm' | 'OneTenMm' | 'LargeFormat4x5' | 'LargeFormat8x10'
export type FilmRollStatus = 'Loaded' | 'InProgress' | 'ShotCompleted' | 'SentToLab' | 'Developed' | 'Scanned' | 'Archived'

export const FILM_FORMATS: FilmFormat[] = ['ThirtyFiveMm', 'OneTwentyMm', 'OneTenMm', 'LargeFormat4x5', 'LargeFormat8x10']
export const FILM_ROLL_STATUSES: FilmRollStatus[] = [
  'Loaded', 'InProgress', 'ShotCompleted', 'SentToLab', 'Developed', 'Scanned', 'Archived',
]

interface GearBase {
  id: string
  name: string
  brand: string
  model: string
  serialNumber: string | null
  mountType: string | null
  notes: string | null
  isActive: boolean
  acquiredOn: string | null
}

export interface CameraBody extends GearBase {
  supportedFormats: FilmFormat[]
}

export interface Lens extends GearBase {
  focalLengthMinMm: number | null
  focalLengthMaxMm: number | null
  maxAperture: number | null
  minAperture: number | null
}

export interface Flash extends GearBase {
  guideNumber: number | null
  hasTtl: boolean
}

export interface FilmRoll {
  id: string
  name: string
  brand: string
  format: FilmFormat
  nominalIso: number
  exposedAtIso: number | null
  frameCount: number
  status: FilmRollStatus
  cameraBodyId: string | null
  cameraBodyName: string | null
  dateLoaded: string | null
  dateFinished: string | null
  dateDeveloped: string | null
  labName: string | null
  developerNotes: string | null
  notes: string | null
}

export interface CreateCameraBodyPayload {
  name: string
  brand: string
  model: string
  serialNumber?: string | null
  mountType?: string | null
  notes?: string | null
  acquiredOn?: string | null
  supportedFormats: FilmFormat[]
}

export type UpdateCameraBodyPayload = CreateCameraBodyPayload & { id: string; isActive: boolean }

export interface CreateLensPayload {
  name: string
  brand: string
  model: string
  serialNumber?: string | null
  mountType?: string | null
  notes?: string | null
  acquiredOn?: string | null
  focalLengthMinMm?: number | null
  focalLengthMaxMm?: number | null
  maxAperture?: number | null
  minAperture?: number | null
}

export type UpdateLensPayload = CreateLensPayload & { id: string; isActive: boolean }

export interface CreateFlashPayload {
  name: string
  brand: string
  model: string
  serialNumber?: string | null
  mountType?: string | null
  notes?: string | null
  acquiredOn?: string | null
  guideNumber?: number | null
  hasTtl: boolean
}

export type UpdateFlashPayload = CreateFlashPayload & { id: string; isActive: boolean }

export interface CreateFilmRollPayload {
  name: string
  brand: string
  format: FilmFormat
  nominalIso: number
  exposedAtIso?: number | null
  frameCount: number
  cameraBodyId?: string | null
  dateLoaded?: string | null
  notes?: string | null
}

export interface UpdateFilmRollPayload {
  id: string
  name: string
  brand: string
  format: FilmFormat
  nominalIso: number
  exposedAtIso?: number | null
  frameCount: number
  status: FilmRollStatus
  cameraBodyId?: string | null
  dateLoaded?: string | null
  dateFinished?: string | null
  dateDeveloped?: string | null
  labName?: string | null
  developerNotes?: string | null
  notes?: string | null
}

export type PhotoProcessingStatus = 'PendingUpload' | 'Uploaded' | 'Processing' | 'Ready' | 'Failed'

export interface ExifData {
  aperture: string | null
  shutterSpeed: string | null
  isoUsed: number | null
  focalLengthMm: number | null
  flashFired: boolean | null
  meteringMode: string | null
  scannerModel: string | null
  gpsLatitude: number | null
  gpsLongitude: number | null
}

export interface Photo {
  id: string
  filmRollId: string
  cameraBodyId: string | null
  lensId: string | null
  flashId: string | null
  frameNumber: number | null
  captureDateUtc: string | null
  originalStorageKey: string
  previewStorageKey: string | null
  thumbnailStorageKey: string | null
  blurHash: string | null
  widthPx: number | null
  heightPx: number | null
  fileSizeBytes: number
  contentType: string
  rating: number
  processingStatus: PhotoProcessingStatus
  exif: ExifData
  createdAtUtc: string
}

export interface RequestPhotoUploadUrlPayload {
  filmRollId: string
  originalFileName: string
  contentType: string
}

export interface RequestPhotoUploadUrlResult {
  uploadUrl: string
  storageKey: string
  expiresAtUtc: string
}

export interface RegisterPhotoPayload {
  filmRollId: string
  storageKey: string
  contentType: string
  fileSizeBytes: number
  cameraBodyId?: string | null
  lensId?: string | null
  flashId?: string | null
  frameNumber?: number | null
  captureDateUtc?: string | null
  exif?: Partial<ExifData> | null
}

export interface PhotoListItem {
  id: string
  filmRollId: string
  thumbnailUrl: string | null
  blurHash: string | null
  widthPx: number | null
  heightPx: number | null
  rating: number
  rotationDegrees: number
  processingStatus: PhotoProcessingStatus
  captureDateUtc: string | null
  tags: string[]
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  hasMore: boolean
}

export interface PhotoTag {
  tagId: string
  name: string
  slug: string
  isAiSuggested: boolean
}

export interface PhotoCritique {
  photoId: string
  model: string
  compositionScore: number
  compositionNotes: string
  lightingNotes: string
  posingNotes: string | null
  recommendations: string[]
  suggestedTags: string[]
  createdAtUtc: string
}

export interface AnalyzePhotoResult {
  critique: PhotoCritique
  appliedTags: string[]
}

export interface PhotoDetail {
  id: string
  filmRollId: string
  filmRollName: string
  cameraBodyId: string | null
  cameraBodyName: string | null
  lensId: string | null
  lensName: string | null
  flashId: string | null
  flashName: string | null
  frameNumber: number | null
  captureDateUtc: string | null
  originalUrl: string | null
  previewUrl: string | null
  thumbnailUrl: string | null
  downloadUrl: string
  blurHash: string | null
  widthPx: number | null
  heightPx: number | null
  fileSizeBytes: number
  contentType: string
  rating: number
  rotationDegrees: number
  processingStatus: PhotoProcessingStatus
  exif: ExifData
  critique: PhotoCritique | null
  tags: PhotoTag[]
  albumIds: string[]
  createdAtUtc: string
}

export interface PhotoDownloadLink {
  photoId: string
  url: string
  fileName: string
}

export interface GetPhotosFilters {
  filmRollId?: string
  cameraBodyId?: string
  lensId?: string
  flashId?: string
  tag?: string
  minRating?: number
}

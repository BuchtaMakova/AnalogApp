import { formatDateTime, formatFileSize } from '@/lib/format'
import type { PhotoDetail } from '@/types/photo'

function Row({ label, value }: { label: string; value: string | number | null | undefined }) {
  if (value === null || value === undefined || value === '') return null
  return (
    <div className="flex items-center justify-between border-b border-neutral-800/60 py-2 text-sm">
      <span className="text-neutral-500">{label}</span>
      <span className="text-neutral-200">{value}</span>
    </div>
  )
}

export function ExifTab({ photo }: { photo: PhotoDetail }) {
  return (
    <div className="flex flex-col">
      <h3 className="mb-1 mt-2 text-xs font-semibold uppercase tracking-wide text-neutral-500">Gear</h3>
      <Row label="Film roll" value={photo.filmRollName} />
      <Row label="Camera" value={photo.cameraBodyName} />
      <Row label="Lens" value={photo.lensName} />
      <Row label="Flash" value={photo.flashName} />
      <Row label="Frame #" value={photo.frameNumber} />
      <Row label="Captured" value={formatDateTime(photo.captureDateUtc)} />

      <h3 className="mb-1 mt-4 text-xs font-semibold uppercase tracking-wide text-neutral-500">Exposure</h3>
      <Row label="Aperture" value={photo.exif.aperture} />
      <Row label="Shutter speed" value={photo.exif.shutterSpeed} />
      <Row label="ISO" value={photo.exif.isoUsed} />
      <Row label="Focal length" value={photo.exif.focalLengthMm ? `${photo.exif.focalLengthMm}mm` : null} />
      <Row label="Flash fired" value={photo.exif.flashFired === null ? null : photo.exif.flashFired ? 'Yes' : 'No'} />
      <Row label="Metering" value={photo.exif.meteringMode} />
      <Row label="Scanner" value={photo.exif.scannerModel} />

      <h3 className="mb-1 mt-4 text-xs font-semibold uppercase tracking-wide text-neutral-500">File</h3>
      <Row label="Dimensions" value={photo.widthPx && photo.heightPx ? `${photo.widthPx}×${photo.heightPx}` : null} />
      <Row label="Size" value={formatFileSize(photo.fileSizeBytes)} />
      <Row label="Type" value={photo.contentType} />
    </div>
  )
}

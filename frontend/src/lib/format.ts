import type { FilmFormat, FilmRollStatus } from '@/types/gear'

export function formatDate(iso: string | null): string {
  if (!iso) return '—'
  return new Intl.DateTimeFormat(undefined, { dateStyle: 'medium' }).format(new Date(iso))
}

export function formatDateTime(iso: string | null): string {
  if (!iso) return '—'
  return new Intl.DateTimeFormat(undefined, { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(iso))
}

export function formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  const units = ['KB', 'MB', 'GB']
  let value = bytes / 1024
  let unitIndex = 0
  while (value >= 1024 && unitIndex < units.length - 1) {
    value /= 1024
    unitIndex += 1
  }
  return `${value.toFixed(1)} ${units[unitIndex]}`
}

const FILM_FORMAT_LABELS: Record<FilmFormat, string> = {
  ThirtyFiveMm: '35mm',
  OneTwentyMm: '120',
  OneTenMm: '110',
  LargeFormat4x5: '4×5',
  LargeFormat8x10: '8×10',
}

export function formatFilmFormat(format: FilmFormat): string {
  return FILM_FORMAT_LABELS[format] ?? format
}

const FILM_ROLL_STATUS_LABELS: Record<FilmRollStatus, string> = {
  Loaded: 'Loaded',
  InProgress: 'In progress',
  ShotCompleted: 'Shot completed',
  SentToLab: 'Sent to lab',
  Developed: 'Developed',
  Scanned: 'Scanned',
  Archived: 'Archived',
}

export function formatFilmRollStatus(status: FilmRollStatus): string {
  return FILM_ROLL_STATUS_LABELS[status] ?? status
}

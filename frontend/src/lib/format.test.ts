import { describe, expect, it } from 'vitest'
import { formatDate, formatFileSize, formatFilmFormat, formatFilmRollStatus } from '@/lib/format'

describe('formatDate', () => {
  it('returns an em dash for null', () => {
    expect(formatDate(null)).toBe('—')
  })

  it('formats an ISO date string', () => {
    const result = formatDate('2026-06-15T00:00:00Z')
    expect(result).not.toBe('—')
    expect(result.length).toBeGreaterThan(0)
  })
})

describe('formatFileSize', () => {
  it('renders sub-1KB sizes in bytes', () => {
    expect(formatFileSize(512)).toBe('512 B')
  })

  it('renders kilobyte-range sizes with one decimal', () => {
    expect(formatFileSize(2048)).toBe('2.0 KB')
  })

  it('renders megabyte-range sizes', () => {
    expect(formatFileSize(5 * 1024 * 1024)).toBe('5.0 MB')
  })

  it('renders gigabyte-range sizes and stops climbing units', () => {
    expect(formatFileSize(3 * 1024 * 1024 * 1024)).toBe('3.0 GB')
  })

  it('handles zero bytes', () => {
    expect(formatFileSize(0)).toBe('0 B')
  })
})

describe('formatFilmFormat', () => {
  it('maps known formats to their display labels', () => {
    expect(formatFilmFormat('ThirtyFiveMm')).toBe('35mm')
    expect(formatFilmFormat('OneTwentyMm')).toBe('120')
    expect(formatFilmFormat('LargeFormat4x5')).toBe('4×5')
  })
})

describe('formatFilmRollStatus', () => {
  it('maps known statuses to human-readable labels', () => {
    expect(formatFilmRollStatus('InProgress')).toBe('In progress')
    expect(formatFilmRollStatus('SentToLab')).toBe('Sent to lab')
    expect(formatFilmRollStatus('Archived')).toBe('Archived')
  })
})

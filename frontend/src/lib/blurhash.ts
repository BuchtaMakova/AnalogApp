import { decode } from 'blurhash'

const cache = new Map<string, string>()

/** Decodes a BlurHash string into a small data: URL suitable for an <img> or CSS background placeholder. */
export function blurHashToDataUrl(hash: string, size = 32): string {
  const cached = cache.get(hash)
  if (cached) return cached

  try {
    const pixels = decode(hash, size, size)
    const canvas = document.createElement('canvas')
    canvas.width = size
    canvas.height = size

    const ctx = canvas.getContext('2d')
    if (!ctx) return ''

    const imageData = ctx.createImageData(size, size)
    imageData.data.set(pixels)
    ctx.putImageData(imageData, 0, 0)

    const dataUrl = canvas.toDataURL()
    cache.set(hash, dataUrl)
    return dataUrl
  } catch {
    return ''
  }
}

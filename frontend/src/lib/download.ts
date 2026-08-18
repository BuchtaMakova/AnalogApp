/**
 * Triggers a native download for a URL that already carries a `Content-Disposition: attachment`
 * header (our presigned storage URLs do) — a plain anchor click, no fetch/blob/CORS involved.
 */
function triggerUrlDownload(url: string) {
  const anchor = document.createElement('a')
  anchor.href = url
  anchor.rel = 'noopener'
  document.body.appendChild(anchor)
  anchor.click()
  anchor.remove()
}

/** Triggers a download for in-memory content (e.g. a ZIP blob fetched with an auth header). */
export function triggerBlobDownload(blob: Blob, fileName: string) {
  const objectUrl = URL.createObjectURL(blob)
  const anchor = document.createElement('a')
  anchor.href = objectUrl
  anchor.download = fileName
  document.body.appendChild(anchor)
  anchor.click()
  anchor.remove()
  URL.revokeObjectURL(objectUrl)
}

/**
 * Fires one download per URL. Browsers throttle/prompt when a page triggers many downloads back
 * to back, so these are spaced out slightly rather than fired in the same tick.
 */
export async function downloadUrlsSequentially(urls: string[]) {
  for (const url of urls) {
    triggerUrlDownload(url)
    await new Promise((resolve) => setTimeout(resolve, 400))
  }
}

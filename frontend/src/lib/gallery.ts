export type ImageSize = 'large' | 'thumb'

export function galleryImageUrl(imageId: string, size: ImageSize): string {
  return `/api/gallery/images/${imageId}/${size}`
}

/** "2026-09-20" -> "20 de septiembre de 2026", without the UTC shift `new Date("2026-09-20")` would cause. */
export function formatEventDate(isoDate: string): string {
  const [year, month, day] = isoDate.split('-').map(Number)
  return new Date(year, month - 1, day).toLocaleDateString('es-MX', { day: 'numeric', month: 'long', year: 'numeric' })
}

export interface PreparedImage {
  large: Blob
  thumbnail: Blob
  width: number
  height: number
}

const LARGE_MAX_SIDE = 1920
const THUMB_MAX_SIDE = 600

/**
 * Resizes a photo in the browser before upload: a 1920px version for the lightbox and a
 * 600px thumbnail for the grid, both JPEG. A phone photo goes from ~5 MB to ~400 KB, so
 * uploading a whole event over mobile data is quick — and re-encoding through a canvas drops
 * the EXIF metadata, including the GPS location phones embed.
 */
export async function prepareImage(file: File): Promise<PreparedImage> {
  let bitmap: ImageBitmap
  try {
    // 'from-image' applies the EXIF rotation, so portrait phone photos don't come out sideways.
    bitmap = await createImageBitmap(file, { imageOrientation: 'from-image' })
  } catch {
    throw new Error('Este navegador no puede leer ese formato de imagen. Usa JPG o PNG.')
  }

  try {
    const large = await toJpeg(bitmap, LARGE_MAX_SIDE, 0.85)
    const thumbnail = await toJpeg(bitmap, THUMB_MAX_SIDE, 0.8)
    return { large: large.blob, thumbnail: thumbnail.blob, width: large.width, height: large.height }
  } finally {
    bitmap.close()
  }
}

function toJpeg(bitmap: ImageBitmap, maxSide: number, quality: number): Promise<{ blob: Blob; width: number; height: number }> {
  const scale = Math.min(1, maxSide / Math.max(bitmap.width, bitmap.height))
  const width = Math.round(bitmap.width * scale)
  const height = Math.round(bitmap.height * scale)

  const canvas = document.createElement('canvas')
  canvas.width = width
  canvas.height = height
  const context = canvas.getContext('2d')
  if (!context) return Promise.reject(new Error('No se pudo procesar la imagen.'))

  // JPEG has no transparency: paint white first, or a transparent PNG would turn black.
  context.fillStyle = '#ffffff'
  context.fillRect(0, 0, width, height)
  context.drawImage(bitmap, 0, 0, width, height)

  return new Promise((resolve, reject) =>
    canvas.toBlob((blob) => (blob ? resolve({ blob, width, height }) : reject(new Error('No se pudo procesar la imagen.'))), 'image/jpeg', quality),
  )
}

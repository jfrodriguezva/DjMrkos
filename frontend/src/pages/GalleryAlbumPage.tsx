import { useQuery } from '@tanstack/react-query'
import { useCallback, useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { ErrorState, LoadingState } from '../components/ui/PageState'
import { api } from '../lib/apiClient'
import { formatEventDate, galleryImageUrl } from '../lib/gallery'
import type { GalleryAlbumDetail, GalleryImage } from '../types/api'
import styles from './GalleryAlbumPage.module.css'

export function GalleryAlbumPage() {
  const { slug = '' } = useParams()
  const { data, isLoading, isError } = useQuery({
    queryKey: ['gallery-album', slug],
    queryFn: () => api.get<GalleryAlbumDetail>(`/gallery/albums/${encodeURIComponent(slug)}`),
  })
  const [openIndex, setOpenIndex] = useState<number | null>(null)

  if (isLoading) return <LoadingState label="Cargando fotos…" />
  if (isError || !data)
    return (
      <div className="container section">
        <ErrorState title="No encontramos este evento" description="Puede que el enlace haya cambiado." action={<Link to="/galeria">Ver toda la galería</Link>} />
      </div>
    )

  const { album, images } = data

  return (
    <>
      <div className={styles.header}>
        <div className="container">
          <Link to="/galeria" className={styles.back}>
            ← Galería
          </Link>
          <h1>{album.title}</h1>
          {album.eventDate && <p className={styles.date}>{formatEventDate(album.eventDate)}</p>}
          {album.description && <p className={styles.description}>{album.description}</p>}
        </div>
      </div>

      <div className="container section">
        <div className={styles.grid}>
          {images.map((image, index) => (
            <button key={image.id} type="button" className={styles.thumb} onClick={() => setOpenIndex(index)} aria-label={image.caption ?? `Foto ${index + 1}`}>
              <img src={galleryImageUrl(image.id, 'thumb')} alt={image.caption ?? ''} loading="lazy" />
            </button>
          ))}
        </div>
      </div>

      {openIndex !== null && <Lightbox images={images} index={openIndex} onChange={setOpenIndex} onClose={() => setOpenIndex(null)} />}
    </>
  )
}

function Lightbox({ images, index, onChange, onClose }: { images: GalleryImage[]; index: number; onChange: (i: number) => void; onClose: () => void }) {
  const image = images[index]
  const hasMany = images.length > 1
  const prev = useCallback(() => onChange((index - 1 + images.length) % images.length), [index, images.length, onChange])
  const next = useCallback(() => onChange((index + 1) % images.length), [index, images.length, onChange])

  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose()
      else if (e.key === 'ArrowLeft') prev()
      else if (e.key === 'ArrowRight') next()
    }
    window.addEventListener('keydown', onKey)
    // Freeze the page behind the photo while it's open.
    const previousOverflow = document.body.style.overflow
    document.body.style.overflow = 'hidden'
    return () => {
      window.removeEventListener('keydown', onKey)
      document.body.style.overflow = previousOverflow
    }
  }, [onClose, prev, next])

  return (
    <div className={styles.lightbox} role="dialog" aria-modal="true" onClick={onClose}>
      <button type="button" className={styles.close} onClick={onClose} aria-label="Cerrar">
        ✕
      </button>
      {hasMany && (
        <button
          type="button"
          className={`${styles.nav} ${styles.navPrev}`}
          onClick={(e) => {
            e.stopPropagation()
            prev()
          }}
          aria-label="Foto anterior"
        >
          ‹
        </button>
      )}

      <figure className={styles.figure} onClick={(e) => e.stopPropagation()}>
        <img src={galleryImageUrl(image.id, 'large')} alt={image.caption ?? ''} width={image.width} height={image.height} />
        <figcaption>
          {image.caption && <span>{image.caption}</span>}
          <span className={styles.counter}>
            {index + 1} / {images.length}
          </span>
        </figcaption>
      </figure>

      {hasMany && (
        <button
          type="button"
          className={`${styles.nav} ${styles.navNext}`}
          onClick={(e) => {
            e.stopPropagation()
            next()
          }}
          aria-label="Foto siguiente"
        >
          ›
        </button>
      )}
    </div>
  )
}

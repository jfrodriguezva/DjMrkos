import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { EmptyState, LoadingState } from '../components/ui/PageState'
import { api } from '../lib/apiClient'
import { formatEventDate, galleryImageUrl } from '../lib/gallery'
import type { GalleryAlbum } from '../types/api'
import styles from './GalleryPage.module.css'

export function GalleryPage() {
  const { data: albums, isLoading } = useQuery({ queryKey: ['gallery-albums'], queryFn: () => api.get<GalleryAlbum[]>('/gallery/albums') })

  return (
    <>
      <div className={styles.header}>
        <div className="container">
          <span className="eyebrow">Eventos reales</span>
          <h1>Galería</h1>
          <p className={styles.lead}>Montajes, pistas llenas y momentos de los eventos que hemos musicalizado. Elige un evento para ver sus fotos.</p>
        </div>
      </div>

      <div className="container section">
        {isLoading && <LoadingState label="Cargando galería…" />}

        {!isLoading && (!albums || albums.length === 0) && (
          <EmptyState title="Pronto verás aquí nuestros eventos" description="Estamos seleccionando las mejores fotos." />
        )}

        {albums && albums.length > 0 && (
          <div className={styles.grid}>
            {albums.map((album) => (
              <Link key={album.id} to={`/galeria/${album.slug}`} className={`card ${styles.albumCard}`}>
                <div className={styles.cover}>
                  {album.coverImageId && <img src={galleryImageUrl(album.coverImageId, 'thumb')} alt="" loading="lazy" />}
                </div>
                <div className={styles.albumInfo}>
                  <h3>{album.title}</h3>
                  <p className={styles.meta}>
                    {album.eventDate ? `${formatEventDate(album.eventDate)} · ` : ''}
                    {album.imageCount} {album.imageCount === 1 ? 'foto' : 'fotos'}
                  </p>
                </div>
              </Link>
            ))}
          </div>
        )}
      </div>
    </>
  )
}

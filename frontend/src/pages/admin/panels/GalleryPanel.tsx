import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { Button } from '../../../components/ui/Button'
import { EmptyState, LoadingState } from '../../../components/ui/PageState'
import { ApiError, api } from '../../../lib/apiClient'
import { formatEventDate, galleryImageUrl, prepareImage } from '../../../lib/gallery'
import type { GalleryAlbum, GalleryAlbumDetail, GalleryImage } from '../../../types/api'
import catalogStyles from './CatalogPanel.module.css'
import styles from './GalleryPanel.module.css'

/**
 * Un álbum por evento. Se crea como borrador: subes todas las fotos, eliges portada y
 * hasta entonces lo publicas — así nadie ve un álbum a medias en /galeria.
 */
export function GalleryPanel() {
  const queryClient = useQueryClient()
  const [selectedId, setSelectedId] = useState<string | null>(null)
  const { data: albums, isLoading } = useQuery({ queryKey: ['admin-gallery-albums'], queryFn: () => api.get<GalleryAlbum[]>('/admin/gallery/albums', true) })

  const refreshLists = () => {
    queryClient.invalidateQueries({ queryKey: ['admin-gallery-albums'] })
    queryClient.invalidateQueries({ queryKey: ['gallery-albums'] })
  }

  return (
    <div className={catalogStyles.layout}>
      <div className={`card ${catalogStyles.panel}`}>
        <h2>Álbumes</h2>
        {isLoading && <LoadingState label="Cargando álbumes…" />}
        {!isLoading && (!albums || albums.length === 0) && <EmptyState title="Aún no hay álbumes" description="Crea el primero abajo." />}

        <div className={catalogStyles.moduleList}>
          {albums?.map((a) => (
            <button
              key={a.id}
              type="button"
              className={`${catalogStyles.moduleRow} ${selectedId === a.id ? catalogStyles.moduleRowActive : ''}`}
              onClick={() => setSelectedId(a.id)}
            >
              <div>
                <div className={catalogStyles.moduleRowName}>{a.title}</div>
                <div className={catalogStyles.moduleRowMeta}>
                  {a.eventDate ? `${formatEventDate(a.eventDate)} · ` : ''}
                  {a.imageCount} {a.imageCount === 1 ? 'foto' : 'fotos'}
                </div>
              </div>
              <span className="badge">{a.isPublished ? 'Publicado' : 'Borrador'}</span>
            </button>
          ))}
        </div>

        <NewAlbumForm
          onCreated={(album) => {
            refreshLists()
            setSelectedId(album.id)
          }}
        />
      </div>

      <div className={`card ${catalogStyles.panel}`}>
        {selectedId ? (
          <AlbumEditor
            key={selectedId}
            albumId={selectedId}
            onChanged={refreshLists}
            onDeleted={() => {
              setSelectedId(null)
              refreshLists()
            }}
          />
        ) : (
          <EmptyState title="Elige un álbum" description="O crea uno nuevo para empezar a subir fotos." />
        )}
      </div>
    </div>
  )
}

function NewAlbumForm({ onCreated }: { onCreated: (album: GalleryAlbum) => void }) {
  const [title, setTitle] = useState('')
  const [eventDate, setEventDate] = useState('')
  const [description, setDescription] = useState('')

  const create = useMutation({
    mutationFn: () => api.post<GalleryAlbum>('/admin/gallery/albums', { title, eventDate: eventDate || null, description: description || null }, true),
    onSuccess: (album) => {
      setTitle('')
      setEventDate('')
      setDescription('')
      onCreated(album)
    },
  })

  return (
    <form
      className={catalogStyles.inlineForm}
      onSubmit={(e) => {
        e.preventDefault()
        if (title.trim()) create.mutate()
      }}
    >
      <div className="field">
        <label htmlFor="albumTitle">Nuevo álbum</label>
        <input id="albumTitle" value={title} onChange={(e) => setTitle(e.target.value)} required maxLength={200} placeholder="Ej. Boda Ana y Luis" />
      </div>
      <div className="field">
        <label htmlFor="albumDate">Fecha del evento (opcional)</label>
        <input id="albumDate" type="date" value={eventDate} onChange={(e) => setEventDate(e.target.value)} />
      </div>
      <div className="field">
        <label htmlFor="albumDescription">Descripción (opcional)</label>
        <textarea id="albumDescription" value={description} onChange={(e) => setDescription(e.target.value)} maxLength={1000} rows={2} />
      </div>
      <Button type="submit" size="sm" disabled={create.isPending || !title.trim()}>
        {create.isPending ? 'Creando…' : 'Crear álbum'}
      </Button>
    </form>
  )
}

/** Rendered only once the album has loaded, so the inputs start from its current values. */
function AlbumDetailsForm({ album, hasImages, onSaved }: { album: GalleryAlbum; hasImages: boolean; onSaved: () => void }) {
  const [title, setTitle] = useState(album.title)
  const [eventDate, setEventDate] = useState(album.eventDate ?? '')
  const [description, setDescription] = useState(album.description ?? '')
  const [isPublished, setIsPublished] = useState(album.isPublished)

  const save = useMutation({
    mutationFn: () => api.put(`/admin/gallery/albums/${album.id}`, { title, eventDate: eventDate || null, description: description || null, isPublished }, true),
    onSuccess: onSaved,
  })

  return (
    <form
      className={catalogStyles.inlineForm}
      style={{ marginTop: 0, paddingTop: 0, borderTop: 'none' }}
      onSubmit={(e) => {
        e.preventDefault()
        save.mutate()
      }}
    >
      <div className={catalogStyles.inlineFormRow}>
        <div className="field">
          <label htmlFor="editTitle">Título</label>
          <input id="editTitle" value={title} onChange={(e) => setTitle(e.target.value)} required maxLength={200} />
        </div>
        <div className="field">
          <label htmlFor="editDate">Fecha del evento</label>
          <input id="editDate" type="date" value={eventDate} onChange={(e) => setEventDate(e.target.value)} />
        </div>
      </div>
      <div className="field">
        <label htmlFor="editDescription">Descripción</label>
        <textarea id="editDescription" value={description} onChange={(e) => setDescription(e.target.value)} maxLength={1000} rows={2} />
      </div>
      <div className={catalogStyles.categoryFooter}>
        <label className={catalogStyles.toggle}>
          <input type="checkbox" checked={isPublished} onChange={(e) => setIsPublished(e.target.checked)} />
          Publicado (visible en /galeria)
        </label>
        <Button type="submit" size="sm" disabled={save.isPending || !title.trim()}>
          {save.isPending ? 'Guardando…' : 'Guardar'}
        </Button>
      </div>
      {save.isSuccess && <p className={styles.hint} style={{ color: 'var(--success)' }}>Guardado.</p>}
      {isPublished && !hasImages && <p className={styles.hint}>Un álbum publicado sin fotos no se muestra hasta que subas al menos una.</p>}
    </form>
  )
}

interface UploadState {
  total: number
  done: number
  failed: string[]
}

function AlbumEditor({ albumId, onChanged, onDeleted }: { albumId: string; onChanged: () => void; onDeleted: () => void }) {
  const queryClient = useQueryClient()
  const queryKey = ['admin-gallery-album', albumId]
  const { data, isLoading } = useQuery({ queryKey, queryFn: () => api.get<GalleryAlbumDetail>(`/admin/gallery/albums/${albumId}`, true) })

  const [upload, setUpload] = useState<UploadState | null>(null)

  const refresh = () => {
    queryClient.invalidateQueries({ queryKey })
    queryClient.invalidateQueries({ queryKey: ['gallery-album'] })
    onChanged()
  }

  const setCover = useMutation({
    mutationFn: (imageId: string) => api.put(`/admin/gallery/albums/${albumId}/cover`, { imageId }, true),
    onSuccess: refresh,
  })

  const removeImage = useMutation({
    mutationFn: (imageId: string) => api.del(`/admin/gallery/images/${imageId}`, true),
    onSuccess: refresh,
  })

  const removeAlbum = useMutation({
    mutationFn: () => api.del(`/admin/gallery/albums/${albumId}`, true),
    onSuccess: onDeleted,
  })

  // One file at a time: progress stays accurate and one bad photo doesn't sink the whole batch.
  const uploadFiles = async (files: File[]) => {
    const state: UploadState = { total: files.length, done: 0, failed: [] }
    setUpload({ ...state })

    for (const file of files) {
      try {
        const prepared = await prepareImage(file)
        const form = new FormData()
        form.append('image', prepared.large, 'image.jpg')
        form.append('thumbnail', prepared.thumbnail, 'thumb.jpg')
        form.append('width', String(prepared.width))
        form.append('height', String(prepared.height))
        await api.upload<GalleryImage>(`/admin/gallery/albums/${albumId}/images`, form)
      } catch (error) {
        const reason = error instanceof ApiError ? error.problem.title : error instanceof Error ? error.message : 'error desconocido'
        state.failed.push(`${file.name}: ${reason}`)
      }
      state.done += 1
      setUpload({ ...state })
    }

    refresh()
  }

  if (isLoading || !data) return <LoadingState label="Cargando álbum…" />

  const { album, images } = data
  const uploading = upload !== null && upload.done < upload.total

  return (
    <div>
      <div className={styles.editorHeader}>
        <h2 style={{ margin: 0 }}>{album.title}</h2>
        {album.isPublished && (
          <a href={`/galeria/${album.slug}`} target="_blank" rel="noopener" className={styles.viewLink}>
            Ver en el sitio ↗
          </a>
        )}
      </div>

      <AlbumDetailsForm album={album} hasImages={images.length > 0} onSaved={refresh} />

      <div className={styles.uploadBox}>
        <label className={`btn btn--primary btn--sm ${uploading ? styles.disabled : ''}`}>
          {uploading ? `Subiendo ${upload.done + 1} de ${upload.total}…` : 'Subir fotos'}
          <input
            type="file"
            accept="image/*"
            multiple
            hidden
            disabled={uploading}
            onChange={(e) => {
              const files = Array.from(e.target.files ?? [])
              e.target.value = ''
              if (files.length > 0) void uploadFiles(files)
            }}
          />
        </label>
        <span className={styles.hint}>Puedes elegir varias a la vez. Se achican antes de subir y se les quita la ubicación GPS.</span>
      </div>

      {upload && !uploading && (
        <p className={styles.hint} style={{ color: upload.failed.length ? 'var(--red-bright)' : 'var(--success)' }}>
          {upload.total - upload.failed.length} de {upload.total} fotos subidas.
          {upload.failed.length > 0 && ` No se pudieron subir: ${upload.failed.join(' · ')}`}
        </p>
      )}

      {images.length === 0 ? (
        <EmptyState title="Este álbum no tiene fotos" />
      ) : (
        <div className={styles.imageGrid}>
          {images.map((image) => {
            const isCover = album.coverImageId === image.id
            return (
              <div key={image.id} className={`${styles.imageCard} ${isCover ? styles.imageCardCover : ''}`}>
                <img src={galleryImageUrl(image.id, 'thumb')} alt={image.caption ?? ''} loading="lazy" />
                {isCover && <span className={`badge ${styles.coverBadge}`}>Portada</span>}
                <div className={styles.imageActions}>
                  {!isCover && (
                    <button type="button" onClick={() => setCover.mutate(image.id)} disabled={setCover.isPending}>
                      Portada
                    </button>
                  )}
                  <button
                    type="button"
                    onClick={() => {
                      if (confirm('¿Eliminar esta foto?')) removeImage.mutate(image.id)
                    }}
                    disabled={removeImage.isPending}
                  >
                    Eliminar
                  </button>
                </div>
              </div>
            )
          })}
        </div>
      )}

      <div className={styles.danger}>
        <Button
          size="sm"
          variant="ghost"
          disabled={removeAlbum.isPending}
          onClick={() => {
            if (confirm(`¿Eliminar el álbum "${album.title}" y sus ${images.length} fotos? No se puede deshacer.`)) removeAlbum.mutate()
          }}
        >
          Eliminar álbum
        </Button>
      </div>
    </div>
  )
}

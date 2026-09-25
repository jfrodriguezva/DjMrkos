import { useMutation, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { useParams } from 'react-router-dom'
import { Button } from '../components/ui/Button'
import { Equalizer } from '../components/ui/Equalizer'
import { LoadingState } from '../components/ui/PageState'
import { ApiError, api } from '../lib/apiClient'
import type { ApiProblem, EventPublic, SongRequest } from '../types/api'
import styles from './SongRequestPage.module.css'

/**
 * What a guest's phone shows after scanning the event's QR — deliberately outside the
 * site's header/footer shell, full-screen and thumb-friendly.
 */
export function SongRequestPage() {
  const { token } = useParams<{ token: string }>()

  const eventQuery = useQuery({
    queryKey: ['qr-event', token],
    queryFn: () => api.get<EventPublic>(`/events/qr/${token}`),
    retry: false,
  })

  const [songTitle, setSongTitle] = useState('')
  const [artist, setArtist] = useState('')
  const [requesterName, setRequesterName] = useState('')
  const [dedication, setDedication] = useState('')

  const submit = useMutation({
    mutationFn: () =>
      api.post<SongRequest>(`/events/qr/${token}/song-requests`, {
        songTitle,
        artist: artist || null,
        requesterName: requesterName || null,
        dedication: dedication || null,
      }),
  })

  if (eventQuery.isLoading) {
    return (
      <div className={styles.screen}>
        <LoadingState label="Buscando tu evento…" />
      </div>
    )
  }

  if (eventQuery.isError) {
    const isGone = eventQuery.error instanceof ApiError && eventQuery.error.status === 410
    return (
      <div className={styles.stateScreen}>
        <h1>{isGone ? 'Este QR ya no está activo' : 'No encontramos este evento'}</h1>
        <p>
          {isGone
            ? 'La ventana para pedir canciones de este evento ya cerró. ¡Gracias por celebrar con DJ MrKos!'
            : 'Revisa que escaneaste el código correcto, o pregúntale al DJ.'}
        </p>
      </div>
    )
  }

  const event = eventQuery.data!

  if (!event.isRequestWindowOpen) {
    return (
      <div className={styles.stateScreen}>
        <h1>Todavía no empieza la fiesta</h1>
        <p>Las peticiones de canciones se abren un poco antes de que arranque {event.clientName}. Vuelve a intentarlo pronto.</p>
      </div>
    )
  }

  if (submit.isSuccess) {
    return (
      <div className={styles.stateScreen}>
        <div className={styles.confirmed}>♫</div>
        <h1>¡Tu canción va en camino!</h1>
        <p>El DJ la tiene en la cola. Gracias por pedirla — disfruta {event.clientName}.</p>
        <Button variant="ghost" className={styles.requestAnother} onClick={() => submit.reset()}>
          Pedir otra canción
        </Button>
      </div>
    )
  }

  const problem = submit.error instanceof ApiError ? (submit.error.problem as ApiProblem) : null
  const isRateLimited = submit.error instanceof ApiError && submit.error.status === 429

  return (
    <div className={styles.screen}>
      <Equalizer />
      <div className={styles.brand}>
        DJ <span>MrKos</span>
      </div>

      <div className={`card ${styles.card}`}>
        <div className={styles.eventName}>{event.clientName.toUpperCase()}</div>
        <h1 style={{ fontSize: 22 }}>Pide tu canción</h1>

        <form
          className={styles.form}
          onSubmit={(e) => {
            e.preventDefault()
            submit.mutate()
          }}
        >
          <div className="field">
            <label htmlFor="songTitle">Canción</label>
            <input
              id="songTitle"
              value={songTitle}
              onChange={(e) => setSongTitle(e.target.value)}
              required
              maxLength={150}
              placeholder="Ej. La Bikina"
              autoFocus
            />
          </div>

          <div className="field">
            <label htmlFor="artist">Artista (opcional)</label>
            <input id="artist" value={artist} onChange={(e) => setArtist(e.target.value)} maxLength={120} placeholder="Ej. Luis Miguel" />
          </div>

          <div className="field">
            <label htmlFor="requesterName">Tu nombre (opcional)</label>
            <input id="requesterName" value={requesterName} onChange={(e) => setRequesterName(e.target.value)} maxLength={80} />
          </div>

          <div className="field">
            <label htmlFor="dedication">Dedicatoria (opcional)</label>
            <textarea id="dedication" value={dedication} onChange={(e) => setDedication(e.target.value)} maxLength={300} rows={2} />
          </div>

          <div className={styles.submitRow}>
            <Button type="submit" disabled={submit.isPending} style={{ width: '100%' }}>
              {submit.isPending ? 'Enviando…' : 'Pedir canción'}
            </Button>
          </div>

          {isRateLimited && <p className="field-error">Ya pediste varias canciones — espera un momento antes de pedir otra.</p>}
          {problem && !isRateLimited && <p className="field-error">{problem.title}</p>}
        </form>
      </div>
    </div>
  )
}

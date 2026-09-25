import { useMutation, useQuery } from '@tanstack/react-query'
import type { HubConnection } from '@microsoft/signalr'
import { useEffect, useState } from 'react'
import { Badge } from '../../../components/ui/Badge'
import { Button } from '../../../components/ui/Button'
import { EmptyState, LoadingState } from '../../../components/ui/PageState'
import { api } from '../../../lib/apiClient'
import { createSongRequestConnection } from '../../../lib/signalr'
import type { EventAdmin, SongRequest, SongRequestStatus } from '../../../types/api'
import styles from './LiveQueuePanel.module.css'

const COLUMNS: { status: SongRequestStatus; title: string }[] = [
  { status: 0, title: 'Pendientes' },
  { status: 1, title: 'En cola' },
  { status: 2, title: 'Tocadas' },
  { status: 3, title: 'Rechazadas' },
]

const ACTION_FOR_STATUS: Record<number, number> = { 1: 0, 2: 1, 3: 2 } // target status -> SongRequestAction

function normalizeTitle(request: SongRequest) {
  return `${request.songTitle}::${request.artist ?? ''}`.trim().toLowerCase()
}

export function LiveQueuePanel() {
  const { data: events } = useQuery({ queryKey: ['admin-events'], queryFn: () => api.get<EventAdmin[]>('/admin/events', true) })
  const [eventId, setEventId] = useState<string>('')
  const [requests, setRequests] = useState<SongRequest[]>([])
  const [connected, setConnected] = useState(false)

  useEffect(() => {
    if (events && events.length > 0 && !eventId) setEventId(events[0].id)
  }, [events, eventId])

  const { data: initialRequests, isLoading } = useQuery({
    queryKey: ['song-requests', eventId],
    queryFn: () => api.get<SongRequest[]>(`/admin/events/${eventId}/song-requests`, true),
    enabled: !!eventId,
  })

  // The fetched page is only the seed for the board — every update after this comes from SignalR.
  useEffect(() => {
    setRequests(initialRequests ?? [])
  }, [initialRequests])

  useEffect(() => {
    if (!eventId) return

    let connection: HubConnection | null = createSongRequestConnection()
    connection.onreconnecting(() => setConnected(false))
    connection.onreconnected(() => setConnected(true))
    connection.onclose(() => setConnected(false))

    connection.on('songRequestCreated', (request: SongRequest) => {
      if (request.eventId !== eventId) return
      setRequests((prev) => [request, ...prev.filter((r) => r.id !== request.id)])
    })

    connection.on('songRequestUpdated', (request: SongRequest) => {
      if (request.eventId !== eventId) return
      setRequests((prev) => prev.map((r) => (r.id === request.id ? request : r)))
    })

    connection
      .start()
      .then(() => {
        setConnected(true)
        return connection?.invoke('JoinEventGroup', eventId)
      })
      .catch(() => setConnected(false))

    return () => {
      connection?.invoke('LeaveEventGroup', eventId).catch(() => {})
      connection?.stop()
      connection = null
    }
  }, [eventId])

  const updateStatus = useMutation({
    mutationFn: ({ id, action }: { id: string; action: number }) =>
      api.patch<SongRequest>(`/admin/song-requests/${id}/status`, { action }, true),
  })

  const countsByGroup = requests.reduce<Record<string, number>>((acc, r) => {
    const key = normalizeTitle(r)
    acc[key] = (acc[key] ?? 0) + 1
    return acc
  }, {})

  return (
    <div>
      <div className={styles.toolbar}>
        <select value={eventId} onChange={(e) => setEventId(e.target.value)} aria-label="Evento">
          {(events ?? []).map((e) => (
            <option key={e.id} value={e.id}>
              {e.clientName}
            </option>
          ))}
        </select>
        <span className={styles.connectionDot}>
          <span className={`${styles.dot} ${connected ? styles.dotConnected : ''}`} />
          {connected ? 'Conectado en vivo' : 'Conectando…'}
        </span>
      </div>

      {!eventId && <EmptyState title="Crea un evento primero" description="La cola en vivo necesita un evento seleccionado." />}
      {eventId && isLoading && <LoadingState label="Cargando cola…" />}

      {eventId && !isLoading && (
        <div className={styles.columns}>
          {COLUMNS.map((column) => {
            const items = requests.filter((r) => r.status === column.status)
            return (
              <div key={column.status} className={styles.column}>
                <h3>
                  {column.title} · {items.length}
                </h3>
                <div className={styles.columnList}>
                  {items.length === 0 && <EmptyState title="—" />}
                  {items.map((r) => {
                    const duplicateCount = countsByGroup[normalizeTitle(r)]
                    return (
                      <div key={r.id} className={`card ${styles.requestCard}`}>
                        <div className={styles.songTitle}>
                          {r.songTitle}
                          {duplicateCount > 1 && (
                            <Badge variant="live">
                              <span className={styles.countBadge}>×{duplicateCount}</span>
                            </Badge>
                          )}
                        </div>
                        {r.artist && <div className={styles.artist}>{r.artist}</div>}
                        {r.dedication && <div className={styles.dedication}>"{r.dedication}"</div>}
                        <div className={styles.meta}>
                          {r.requesterName ?? 'Anónimo'} · {new Date(r.createdAtUtc).toLocaleTimeString('es-MX')}
                        </div>
                        <div className={styles.actions}>
                          {column.status !== 1 && (
                            <Button size="sm" variant="ghost" onClick={() => updateStatus.mutate({ id: r.id, action: ACTION_FOR_STATUS[1] })}>
                              Cola
                            </Button>
                          )}
                          {column.status !== 2 && (
                            <Button size="sm" onClick={() => updateStatus.mutate({ id: r.id, action: ACTION_FOR_STATUS[2] })}>
                              Tocada
                            </Button>
                          )}
                          {column.status !== 3 && (
                            <Button size="sm" variant="ghost" onClick={() => updateStatus.mutate({ id: r.id, action: ACTION_FOR_STATUS[3] })}>
                              Rechazar
                            </Button>
                          )}
                        </div>
                      </div>
                    )
                  })}
                </div>
              </div>
            )
          })}
        </div>
      )}
    </div>
  )
}

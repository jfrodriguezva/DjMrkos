import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { Button } from '../../../components/ui/Button'
import { Badge } from '../../../components/ui/Badge'
import { EmptyState, LoadingState } from '../../../components/ui/PageState'
import { api } from '../../../lib/apiClient'
import type { EventAdmin, EventWithQr } from '../../../types/api'
import styles from './EventsPanel.module.css'

const STATUS_LABEL: Record<number, string> = { 0: 'Programado', 1: 'En vivo', 2: 'Completado', 3: 'Cancelado' }

export function EventsPanel() {
  const queryClient = useQueryClient()
  const { data: events, isLoading } = useQuery({
    queryKey: ['admin-events'],
    queryFn: () => api.get<EventAdmin[]>('/admin/events', true),
  })

  const [clientName, setClientName] = useState('')
  const [location, setLocation] = useState('')
  const [eventDate, setEventDate] = useState('')
  const [lastCreated, setLastCreated] = useState<EventWithQr | null>(null)

  const createEvent = useMutation({
    mutationFn: () =>
      api.post<EventWithQr>(
        '/admin/events',
        { clientName, location: location || null, eventDateUtc: new Date(eventDate).toISOString() },
        true,
      ),
    onSuccess: (created) => {
      setLastCreated(created)
      setClientName('')
      setLocation('')
      setEventDate('')
      queryClient.invalidateQueries({ queryKey: ['admin-events'] })
    },
  })

  return (
    <div className={styles.layout}>
      <div className={`card ${styles.panel}`}>
        <h2>Próximos eventos</h2>
        {isLoading && <LoadingState label="Cargando eventos…" />}
        {!isLoading && (!events || events.length === 0) && <EmptyState title="Sin eventos programados" />}
        {events && events.length > 0 && (
          <div className={styles.list}>
            {events.map((e) => (
              <div key={e.id} className={`card ${styles.eventRow}`}>
                <div>
                  <div className={styles.eventName}>{e.clientName}</div>
                  <div className={styles.eventDate}>{new Date(e.eventDateUtc).toLocaleString('es-MX')}</div>
                </div>
                <Badge variant={e.status === 1 ? 'live' : e.status === 2 ? 'muted' : 'default'}>{STATUS_LABEL[e.status]}</Badge>
              </div>
            ))}
          </div>
        )}
      </div>

      <div className={`card ${styles.panel}`}>
        <h2>Nuevo evento</h2>
        <form
          className={styles.form}
          onSubmit={(e) => {
            e.preventDefault()
            createEvent.mutate()
          }}
        >
          <div className="field">
            <label htmlFor="clientName">Cliente / evento</label>
            <input id="clientName" value={clientName} onChange={(e) => setClientName(e.target.value)} required maxLength={120} />
          </div>
          <div className="field">
            <label htmlFor="location">Lugar (opcional)</label>
            <input id="location" value={location} onChange={(e) => setLocation(e.target.value)} maxLength={200} />
          </div>
          <div className="field">
            <label htmlFor="eventDate">Fecha y hora</label>
            <input id="eventDate" type="datetime-local" value={eventDate} onChange={(e) => setEventDate(e.target.value)} required />
          </div>
          <Button type="submit" disabled={createEvent.isPending}>
            {createEvent.isPending ? 'Creando…' : 'Crear evento y generar QR'}
          </Button>
          {createEvent.isError && <p className="field-error">No se pudo crear el evento.</p>}
        </form>

        {lastCreated && (
          <div className={styles.qrResult}>
            <p style={{ color: 'var(--text-secondary)', fontSize: 13.5, marginBottom: 12 }}>
              QR de <strong style={{ color: 'var(--text-primary)' }}>{lastCreated.event.clientName}</strong> — imprímelo o muéstralo en cabina.
            </p>
            <img src={lastCreated.qrCodeDataUrl} alt={`Código QR para ${lastCreated.event.clientName}`} />
            <div className={styles.qrLink}>/evento/{lastCreated.event.qrToken}</div>
          </div>
        )}
      </div>
    </div>
  )
}

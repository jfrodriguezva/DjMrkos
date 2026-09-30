import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { Badge } from '../../../components/ui/Badge'
import { Button } from '../../../components/ui/Button'
import { EmptyState, LoadingState } from '../../../components/ui/PageState'
import { api } from '../../../lib/apiClient'
import type { Lead } from '../../../types/api'
import styles from './AdminList.module.css'

/**
 * Leads don't carry a "source" column — every entry point (Contratar, Cotizador, Agendar,
 * the plain contact form) writes a distinct opening line into `message`, so the admin can
 * still tell a firm booking from a browse at a glance without a schema change.
 */
function leadKind(message: string): { label: string; variant: 'live' | 'success' | 'default' } {
  if (message.startsWith('SOLICITUD DE CONTRATACIÓN')) return { label: 'Contratación', variant: 'live' }
  if (message.startsWith('Solicitud de apartado de fecha')) return { label: 'Agendar', variant: 'success' }
  if (message.startsWith('Presupuesto armado desde el cotizador') || message.startsWith('Sin artículos seleccionados'))
    return { label: 'Cotización', variant: 'default' }
  return { label: 'Contacto', variant: 'default' }
}

export function LeadsPanel() {
  const queryClient = useQueryClient()
  const { data: leads, isLoading } = useQuery({ queryKey: ['admin-leads'], queryFn: () => api.get<Lead[]>('/admin/leads', true) })
  const [confirmingId, setConfirmingId] = useState<string | null>(null)

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ['admin-leads'] })
    queryClient.invalidateQueries({ queryKey: ['admin-events'] })
  }

  if (isLoading) return <LoadingState label="Cargando cotizaciones…" />
  if (!leads || leads.length === 0) return <EmptyState title="Sin solicitudes todavía" />

  return (
    <div className={styles.list}>
      {leads.map((lead) => {
        const kind = leadKind(lead.message)
        const isWon = lead.status === 2
        return (
          <div key={lead.id} className={`card ${styles.row}`}>
            <div className={styles.rowMain}>
              <div className={styles.rowTitle}>
                {lead.name} · {lead.email}
              </div>
              <div className={styles.rowMeta}>
                {lead.phone && `${lead.phone} · `}
                {lead.eventDate ? `Evento: ${lead.eventDate}` : 'Sin fecha definida'} ·{' '}
                {new Date(lead.createdAtUtc).toLocaleDateString('es-MX')}
              </div>
              <p className={styles.rowBody}>{lead.message}</p>

              {confirmingId === lead.id && (
                <ConfirmLeadForm lead={lead} onDone={() => setConfirmingId(null)} onConfirmed={invalidate} />
              )}
            </div>
            <div style={{ display: 'flex', flexDirection: 'column', gap: 8, alignItems: 'flex-end' }}>
              <Badge variant={kind.variant}>{kind.label}</Badge>
              {isWon ? (
                <Badge variant="success">Cita confirmada</Badge>
              ) : (
                <Button size="sm" variant="ghost" onClick={() => setConfirmingId(confirmingId === lead.id ? null : lead.id)}>
                  Confirmar cita
                </Button>
              )}
            </div>
          </div>
        )
      })}
    </div>
  )
}

/**
 * Confirming a lead is what actually books it: it creates the Event that shows up in the
 * "Eventos" tab (the DJ's internal calendar), reusing the same entity the QR flow relies on.
 */
function ConfirmLeadForm({ lead, onDone, onConfirmed }: { lead: Lead; onDone: () => void; onConfirmed: () => void }) {
  const [date, setDate] = useState(lead.eventDate ?? '')
  const [time, setTime] = useState('18:00')
  const [location, setLocation] = useState('')

  const confirm = useMutation({
    mutationFn: () =>
      api.post(
        `/admin/leads/${lead.id}/confirm`,
        { eventDateUtc: new Date(`${date}T${time}`).toISOString(), location: location || null },
        true,
      ),
    onSuccess: () => {
      onConfirmed()
      onDone()
    },
  })

  return (
    <form
      style={{ display: 'flex', gap: 8, flexWrap: 'wrap', alignItems: 'flex-end', marginTop: 12, paddingTop: 12, borderTop: '1px solid var(--border)' }}
      onSubmit={(e) => {
        e.preventDefault()
        confirm.mutate()
      }}
    >
      <div className="field">
        <label htmlFor={`confirmDate-${lead.id}`}>Fecha</label>
        <input id={`confirmDate-${lead.id}`} type="date" value={date} onChange={(e) => setDate(e.target.value)} required />
      </div>
      <div className="field">
        <label htmlFor={`confirmTime-${lead.id}`}>Hora</label>
        <input id={`confirmTime-${lead.id}`} type="time" value={time} onChange={(e) => setTime(e.target.value)} required />
      </div>
      <div className="field">
        <label htmlFor={`confirmLocation-${lead.id}`}>Lugar (opcional)</label>
        <input id={`confirmLocation-${lead.id}`} value={location} onChange={(e) => setLocation(e.target.value)} maxLength={200} />
      </div>
      <Button type="submit" size="sm" disabled={confirm.isPending || !date}>
        {confirm.isPending ? 'Agendando…' : 'Agendar en el calendario'}
      </Button>
      <Button type="button" size="sm" variant="ghost" onClick={onDone}>
        Cancelar
      </Button>
      {confirm.isError && <p className="field-error">No se pudo confirmar la cita.</p>}
    </form>
  )
}

import { useQuery } from '@tanstack/react-query'
import { Badge } from '../../../components/ui/Badge'
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
  const { data: leads, isLoading } = useQuery({ queryKey: ['admin-leads'], queryFn: () => api.get<Lead[]>('/admin/leads', true) })

  if (isLoading) return <LoadingState label="Cargando cotizaciones…" />
  if (!leads || leads.length === 0) return <EmptyState title="Sin solicitudes todavía" />

  return (
    <div className={styles.list}>
      {leads.map((lead) => {
        const kind = leadKind(lead.message)
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
            </div>
            <Badge variant={kind.variant}>{kind.label}</Badge>
          </div>
        )
      })}
    </div>
  )
}

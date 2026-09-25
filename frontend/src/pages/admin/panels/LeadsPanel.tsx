import { useQuery } from '@tanstack/react-query'
import { EmptyState, LoadingState } from '../../../components/ui/PageState'
import { api } from '../../../lib/apiClient'
import type { Lead } from '../../../types/api'
import styles from './AdminList.module.css'

export function LeadsPanel() {
  const { data: leads, isLoading } = useQuery({ queryKey: ['admin-leads'], queryFn: () => api.get<Lead[]>('/admin/leads', true) })

  if (isLoading) return <LoadingState label="Cargando cotizaciones…" />
  if (!leads || leads.length === 0) return <EmptyState title="Sin solicitudes de cotización todavía" />

  return (
    <div className={styles.list}>
      {leads.map((lead) => (
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
        </div>
      ))}
    </div>
  )
}

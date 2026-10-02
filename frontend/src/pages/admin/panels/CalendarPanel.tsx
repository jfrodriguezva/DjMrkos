import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { Button } from '../../../components/ui/Button'
import { EmptyState, LoadingState } from '../../../components/ui/PageState'
import { ApiError, api } from '../../../lib/apiClient'
import { formatEventDate } from '../../../lib/gallery'
import type { BlockedDate } from '../../../types/api'
import styles from './CatalogPanel.module.css'

/**
 * Cierra días en el calendario público (/agendar) además de los que ya ocupa un evento:
 * vacaciones, compromisos personales. El motivo solo lo ve el DJ — el visitante solo ve "Ocupado".
 */
export function CalendarPanel() {
  const queryClient = useQueryClient()
  const { data: blocked, isLoading } = useQuery({ queryKey: ['admin-blocked-dates'], queryFn: () => api.get<BlockedDate[]>('/admin/blocked-dates', true) })

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ['admin-blocked-dates'] })
    queryClient.invalidateQueries({ queryKey: ['availability'] })
  }

  const unblock = useMutation({
    mutationFn: (id: string) => api.del(`/admin/blocked-dates/${id}`, true),
    onSuccess: invalidate,
  })

  return (
    <div className={styles.layout}>
      <div className={`card ${styles.panel}`}>
        <h2>Bloquear fechas</h2>
        <p style={{ color: 'var(--text-secondary)', fontSize: 13.5 }}>
          Los días bloqueados aparecen como <strong>Ocupado</strong> en Agendar y avisan en Contratar. Los días con un evento ya se bloquean solos.
        </p>
        <BlockForm onBlocked={invalidate} />
      </div>

      <div className={`card ${styles.panel}`}>
        <h2>Fechas bloqueadas</h2>
        {isLoading && <LoadingState label="Cargando fechas…" />}
        {!isLoading && (!blocked || blocked.length === 0) && <EmptyState title="No hay fechas bloqueadas" />}

        <div className={styles.categoryList}>
          {blocked?.map((b) => (
            <div key={b.id} className={`card ${styles.categoryRow}`}>
              <div className={styles.categoryFooter}>
                <div>
                  <div className={styles.moduleRowName}>{formatEventDate(b.date)}</div>
                  {b.reason && <div className={styles.moduleRowMeta}>{b.reason}</div>}
                </div>
                <Button size="sm" variant="ghost" onClick={() => unblock.mutate(b.id)} disabled={unblock.isPending}>
                  Desbloquear
                </Button>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  )
}

function BlockForm({ onBlocked }: { onBlocked: () => void }) {
  const [from, setFrom] = useState('')
  const [to, setTo] = useState('')
  const [reason, setReason] = useState('')

  const block = useMutation({
    mutationFn: () => api.post<BlockedDate[]>('/admin/blocked-dates', { from, to: to || null, reason: reason || null }, true),
    onSuccess: () => {
      setFrom('')
      setTo('')
      setReason('')
      onBlocked()
    },
  })

  const error = block.error instanceof ApiError ? (Object.values(block.error.problem.errors ?? {})[0]?.[0] ?? block.error.problem.title) : null

  return (
    <form
      className={styles.inlineForm}
      onSubmit={(e) => {
        e.preventDefault()
        if (from) block.mutate()
      }}
    >
      <div className={styles.inlineFormRow}>
        <div className="field">
          <label htmlFor="blockFrom">Desde</label>
          <input id="blockFrom" type="date" value={from} onChange={(e) => setFrom(e.target.value)} required />
        </div>
        <div className="field">
          <label htmlFor="blockTo">Hasta (opcional)</label>
          <input id="blockTo" type="date" value={to} min={from || undefined} onChange={(e) => setTo(e.target.value)} />
        </div>
      </div>
      <div className="field">
        <label htmlFor="blockReason">Motivo (solo lo ves tú)</label>
        <input id="blockReason" value={reason} onChange={(e) => setReason(e.target.value)} maxLength={200} placeholder="Ej. Vacaciones" />
      </div>
      <Button type="submit" size="sm" disabled={block.isPending || !from}>
        {block.isPending ? 'Bloqueando…' : to ? 'Bloquear rango' : 'Bloquear día'}
      </Button>
      {block.isSuccess && (
        <p style={{ color: 'var(--success)', fontSize: 13 }}>
          {block.data.length === 0 ? 'Esas fechas ya estaban bloqueadas.' : `Listo: ${block.data.length} ${block.data.length === 1 ? 'día bloqueado' : 'días bloqueados'}.`}
        </p>
      )}
      {error && <p className="field-error">{error}</p>}
    </form>
  )
}

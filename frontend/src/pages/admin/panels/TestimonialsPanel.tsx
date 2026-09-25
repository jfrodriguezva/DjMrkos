import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Badge } from '../../../components/ui/Badge'
import { Button } from '../../../components/ui/Button'
import { EmptyState, LoadingState } from '../../../components/ui/PageState'
import { api } from '../../../lib/apiClient'
import type { Testimonial } from '../../../types/api'
import styles from './AdminList.module.css'

export function TestimonialsPanel() {
  const queryClient = useQueryClient()
  const { data: testimonials, isLoading } = useQuery({
    queryKey: ['admin-testimonials'],
    queryFn: () => api.get<Testimonial[]>('/admin/testimonials', true),
  })

  const approve = useMutation({
    mutationFn: (id: string) => api.post(`/admin/testimonials/${id}/approve`, undefined, true),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['admin-testimonials'] }),
  })

  if (isLoading) return <LoadingState label="Cargando testimonios…" />
  if (!testimonials || testimonials.length === 0) return <EmptyState title="Sin testimonios todavía" />

  return (
    <div className={styles.list}>
      {testimonials.map((t) => (
        <div key={t.id} className={`card ${styles.row}`}>
          <div className={styles.rowMain}>
            <div className={styles.rowTitle}>{t.clientName}</div>
            <div className={styles.rowMeta}>
              <span className={styles.stars}>{'★'.repeat(t.rating)}{'☆'.repeat(5 - t.rating)}</span> ·{' '}
              {new Date(t.createdAtUtc).toLocaleDateString('es-MX')}
            </div>
            <p className={styles.rowBody}>{t.comment}</p>
          </div>
          {t.isApproved ? (
            <Badge variant="success">Publicado</Badge>
          ) : (
            <Button size="sm" onClick={() => approve.mutate(t.id)} disabled={approve.isPending}>
              Aprobar
            </Button>
          )}
        </div>
      ))}
    </div>
  )
}

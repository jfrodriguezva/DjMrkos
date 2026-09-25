import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { Button } from '../components/ui/Button'
import { EmptyState, LoadingState } from '../components/ui/PageState'
import { api } from '../lib/apiClient'
import type { Testimonial } from '../types/api'
import styles from './TestimonialsPage.module.css'

export function TestimonialsPage() {
  const queryClient = useQueryClient()
  const { data: testimonials, isLoading } = useQuery({ queryKey: ['testimonials'], queryFn: () => api.get<Testimonial[]>('/testimonials') })

  const [clientName, setClientName] = useState('')
  const [rating, setRating] = useState(5)
  const [comment, setComment] = useState('')
  const [submitted, setSubmitted] = useState(false)

  const submit = useMutation({
    mutationFn: () => api.post<Testimonial>('/testimonials', { clientName, eventId: null, rating, comment }),
    onSuccess: () => {
      setSubmitted(true)
      setClientName('')
      setComment('')
      setRating(5)
      queryClient.invalidateQueries({ queryKey: ['testimonials'] })
    },
  })

  return (
    <>
      <div className={styles.header}>
        <div className="container">
          <span className="eyebrow">Lo que dicen</span>
          <h1>Testimonios</h1>
          <p style={{ color: 'var(--text-secondary)', marginTop: 12 }}>
            Cada reseña pasa por revisión antes de publicarse — cuéntanos cómo fue tu evento.
          </p>
        </div>
      </div>

      <div className="container section">
        <div className={styles.layout}>
          <div>
            {isLoading && <LoadingState label="Cargando testimonios…" />}
            {!isLoading && (!testimonials || testimonials.length === 0) && (
              <EmptyState title="Todavía no hay testimonios publicados" description="Sé el primero en compartir tu experiencia." />
            )}
            {testimonials && testimonials.length > 0 && (
              <div className={styles.grid}>
                {testimonials.map((t) => (
                  <div key={t.id} className={`card ${styles.testimonialCard}`}>
                    <div className={styles.stars}>{'★'.repeat(t.rating)}{'☆'.repeat(5 - t.rating)}</div>
                    <p>“{t.comment}”</p>
                    <div className={styles.name}>{t.clientName}</div>
                  </div>
                ))}
              </div>
            )}
          </div>

          <div className={`card ${styles.formCard}`}>
            <h2>Comparte tu experiencia</h2>
            <form
              className={styles.form}
              onSubmit={(e) => {
                e.preventDefault()
                submit.mutate()
              }}
            >
              <div className="field">
                <label htmlFor="clientName">Tu nombre</label>
                <input id="clientName" value={clientName} onChange={(e) => setClientName(e.target.value)} required maxLength={80} />
              </div>

              <div className="field">
                <label>Calificación</label>
                <div className={styles.ratingRow}>
                  {[1, 2, 3, 4, 5].map((n) => (
                    <button
                      key={n}
                      type="button"
                      className={`${styles.starButton} ${n <= rating ? styles.starButtonActive : ''}`}
                      onClick={() => setRating(n)}
                      aria-label={`${n} estrellas`}
                    >
                      ★
                    </button>
                  ))}
                </div>
              </div>

              <div className="field">
                <label htmlFor="comment">Comentario</label>
                <textarea id="comment" value={comment} onChange={(e) => setComment(e.target.value)} required maxLength={600} />
              </div>

              <Button type="submit" disabled={submit.isPending}>
                {submit.isPending ? 'Enviando…' : 'Enviar reseña'}
              </Button>

              {submitted && <p className={styles.success}>¡Gracias! Tu reseña quedó pendiente de aprobación.</p>}
              {submit.isError && <p className="field-error">No se pudo enviar. Intenta de nuevo.</p>}
            </form>
          </div>
        </div>
      </div>
    </>
  )
}

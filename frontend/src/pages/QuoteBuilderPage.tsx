import { useMutation, useQuery } from '@tanstack/react-query'
import { useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { CartPanel } from '../components/quote/CartPanel'
import { CatalogGrid } from '../components/quote/CatalogGrid'
import { Button } from '../components/ui/Button'
import { LoadingState } from '../components/ui/PageState'
import { ApiError, api } from '../lib/apiClient'
import { audienceTierFor, formatMxn } from '../lib/currency'
import { usePlanner } from '../lib/usePlanner'
import type { ApiProblem, Lead, MenuModule } from '../types/api'
import styles from './QuoteBuilderPage.module.css'

export function QuoteBuilderPage() {
  const navigate = useNavigate()
  const { data: modules, isLoading } = useQuery({ queryKey: ['menu'], queryFn: () => api.get<MenuModule[]>('/menu') })
  const planner = usePlanner()

  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [phone, setPhone] = useState('')
  const [notes, setNotes] = useState('')

  const tier = audienceTierFor(planner.guestCount)
  const total = useMemo(() => planner.cart.reduce((sum, l) => sum + (l.price ?? 0) * l.quantity, 0), [planner.cart])

  const summaryMessage = useMemo(() => {
    const lines = planner.cart.map(
      (l) => `- ${l.quantity}× ${l.name} (${l.moduleName})${l.price ? ` — ${formatMxn(l.price * l.quantity)}` : ' — incluido/a cotizar'}`,
    )
    const parts = [
      planner.cart.length > 0 ? `Presupuesto armado desde el cotizador:\n${lines.join('\n')}` : 'Sin artículos seleccionados en el cotizador.',
      `Invitados estimados: ${planner.guestCount}`,
      `Total estimado: ${formatMxn(total)}`,
    ]
    if (notes.trim()) parts.push(`Notas: ${notes.trim()}`)
    return parts.join('\n\n')
  }, [planner.cart, planner.guestCount, total, notes])

  const submit = useMutation({
    mutationFn: () =>
      api.post<Lead>('/leads', {
        name,
        email,
        phone: phone || null,
        eventDate: planner.preferredDate,
        message: summaryMessage,
      }),
  })

  const problem = submit.error instanceof ApiError ? (submit.error.problem as ApiProblem) : null

  return (
    <>
      <div className={styles.header}>
        <div className="container">
          <span className="eyebrow">Cotizador interactivo</span>
          <h1>Arma tu presupuesto</h1>
          <p className={styles.headerLead}>
            Arrastra los servicios que quieres a tu presupuesto — o toca el botón <strong style={{ color: 'var(--text-primary)' }}>+</strong> de
            cada tarjeta. El total se actualiza al instante.
          </p>

          <div className={styles.guestBar}>
            <div className={styles.guestField}>
              <label htmlFor="guestCount">Invitados esperados</label>
              <div className={styles.guestInputRow}>
                <button
                  type="button"
                  className={styles.stepBtn}
                  onClick={() => planner.setGuestCount(Math.max(10, planner.guestCount - 10))}
                  aria-label="Menos invitados"
                >
                  −
                </button>
                <input
                  id="guestCount"
                  type="number"
                  min={10}
                  max={2000}
                  value={planner.guestCount}
                  onChange={(e) => planner.setGuestCount(Math.max(10, Number(e.target.value) || 10))}
                />
                <button
                  type="button"
                  className={styles.stepBtn}
                  onClick={() => planner.setGuestCount(Math.min(2000, planner.guestCount + 10))}
                  aria-label="Más invitados"
                >
                  +
                </button>
              </div>
            </div>

            <div className={`card ${styles.tierCard}`}>
              <span className={styles.tierLabel}>{tier.label}</span>
              <p className={styles.tierLine}>
                <b>Audio:</b> {tier.audio}
              </p>
              <p className={styles.tierLine}>
                <b>Iluminación:</b> {tier.lighting}
              </p>
            </div>
          </div>
        </div>
      </div>

      <div className="container section">
        {isLoading && <LoadingState label="Cargando catálogo…" />}

        {modules && (
          <div className={styles.layout}>
            <CatalogGrid modules={modules} onAdd={planner.addItem} />

            <CartPanel cart={planner.cart} total={total} onChangeQty={planner.changeQty} onRemove={planner.removeItem} onDropPayload={planner.addItem}>
              {planner.cart.length > 0 && (
                <Button variant="ghost" className={styles.hireLink} onClick={() => navigate('/contratar')}>
                  Continuar a contratar →
                </Button>
              )}

              {submit.isSuccess ? (
                <p className={styles.success}>¡Gracias, {name.split(' ')[0]}! Recibimos tu presupuesto y te contactaremos muy pronto.</p>
              ) : (
                <form
                  className={styles.contactForm}
                  onSubmit={(e) => {
                    e.preventDefault()
                    submit.mutate()
                  }}
                >
                  <div className="field">
                    <label htmlFor="qbName">Nombre</label>
                    <input id="qbName" value={name} onChange={(e) => setName(e.target.value)} required maxLength={120} />
                  </div>
                  <div className="field">
                    <label htmlFor="qbEmail">Correo</label>
                    <input id="qbEmail" type="email" value={email} onChange={(e) => setEmail(e.target.value)} required maxLength={200} />
                  </div>
                  <div className="field">
                    <label htmlFor="qbPhone">Teléfono (opcional)</label>
                    <input id="qbPhone" value={phone} onChange={(e) => setPhone(e.target.value)} maxLength={30} />
                  </div>
                  <div className="field">
                    <label htmlFor="qbDate">Fecha del evento (opcional)</label>
                    <input
                      id="qbDate"
                      type="date"
                      value={planner.preferredDate ?? ''}
                      onChange={(e) => planner.setPreferredDate(e.target.value || null)}
                    />
                  </div>
                  <div className="field">
                    <label htmlFor="qbNotes">Notas adicionales (opcional)</label>
                    <textarea id="qbNotes" value={notes} onChange={(e) => setNotes(e.target.value)} maxLength={400} rows={2} />
                  </div>
                  <Button type="submit" disabled={submit.isPending}>
                    {submit.isPending ? 'Enviando…' : 'Enviar presupuesto'}
                  </Button>
                  {problem && <p className="field-error">{problem.title}</p>}
                </form>
              )}
            </CartPanel>
          </div>
        )}
      </div>
    </>
  )
}

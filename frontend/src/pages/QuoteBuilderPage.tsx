import { useMutation, useQuery } from '@tanstack/react-query'
import { useMemo, useState } from 'react'
import { Button } from '../components/ui/Button'
import { LoadingState } from '../components/ui/PageState'
import { ApiError, api } from '../lib/apiClient'
import { audienceTierFor, formatMxn } from '../lib/currency'
import type { ApiProblem, Lead, MenuModule } from '../types/api'
import styles from './QuoteBuilderPage.module.css'

interface CartLine {
  categoryId: string
  name: string
  moduleName: string
  price: number | null
  quantity: number
}

interface DragPayload {
  categoryId: string
  name: string
  moduleName: string
  price: number | null
}

const DRAG_MIME = 'application/json'

export function QuoteBuilderPage() {
  const { data: modules, isLoading } = useQuery({ queryKey: ['menu'], queryFn: () => api.get<MenuModule[]>('/menu') })

  const [guestCount, setGuestCount] = useState(100)
  const [cart, setCart] = useState<CartLine[]>([])
  const [dropActive, setDropActive] = useState(false)
  const [draggingId, setDraggingId] = useState<string | null>(null)

  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [phone, setPhone] = useState('')
  const [eventDate, setEventDate] = useState('')
  const [notes, setNotes] = useState('')

  const tier = audienceTierFor(guestCount)

  function addItem(payload: DragPayload) {
    setCart((prev) => {
      const existing = prev.find((l) => l.categoryId === payload.categoryId)
      if (existing) {
        return prev.map((l) => (l.categoryId === payload.categoryId ? { ...l, quantity: l.quantity + 1 } : l))
      }
      return [...prev, { ...payload, quantity: 1 }]
    })
  }

  function changeQty(categoryId: string, delta: number) {
    setCart((prev) =>
      prev
        .map((l) => (l.categoryId === categoryId ? { ...l, quantity: l.quantity + delta } : l))
        .filter((l) => l.quantity > 0),
    )
  }

  function removeItem(categoryId: string) {
    setCart((prev) => prev.filter((l) => l.categoryId !== categoryId))
  }

  const total = useMemo(() => cart.reduce((sum, l) => sum + (l.price ?? 0) * l.quantity, 0), [cart])

  const summaryMessage = useMemo(() => {
    const lines = cart.map((l) => `- ${l.quantity}× ${l.name} (${l.moduleName})${l.price ? ` — ${formatMxn(l.price * l.quantity)}` : ' — incluido/a cotizar'}`)
    const parts = [
      cart.length > 0 ? `Presupuesto armado desde el cotizador:\n${lines.join('\n')}` : 'Sin artículos seleccionados en el cotizador.',
      `Invitados estimados: ${guestCount}`,
      `Total estimado: ${formatMxn(total)}`,
    ]
    if (notes.trim()) parts.push(`Notas: ${notes.trim()}`)
    return parts.join('\n\n')
  }, [cart, guestCount, total, notes])

  const submit = useMutation({
    mutationFn: () =>
      api.post<Lead>('/leads', {
        name,
        email,
        phone: phone || null,
        eventDate: eventDate || null,
        message: summaryMessage,
      }),
  })

  const problem = submit.error instanceof ApiError ? (submit.error.problem as ApiProblem) : null

  function handleDragStart(e: React.DragEvent, payload: DragPayload) {
    e.dataTransfer.setData(DRAG_MIME, JSON.stringify(payload))
    e.dataTransfer.effectAllowed = 'copy'
    setDraggingId(payload.categoryId)
  }

  function handleDrop(e: React.DragEvent) {
    e.preventDefault()
    setDropActive(false)
    const raw = e.dataTransfer.getData(DRAG_MIME)
    if (!raw) return
    addItem(JSON.parse(raw) as DragPayload)
  }

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
                <button type="button" className={styles.stepBtn} onClick={() => setGuestCount((n) => Math.max(10, n - 10))} aria-label="Menos invitados">
                  −
                </button>
                <input
                  id="guestCount"
                  type="number"
                  min={10}
                  max={2000}
                  value={guestCount}
                  onChange={(e) => setGuestCount(Math.max(10, Number(e.target.value) || 10))}
                />
                <button type="button" className={styles.stepBtn} onClick={() => setGuestCount((n) => Math.min(2000, n + 10))} aria-label="Más invitados">
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
            <div>
              {modules.map((m) => (
                <div key={m.id} className={styles.moduleGroup}>
                  <h2>{m.name}</h2>
                  {m.categories.length === 0 ? (
                    <p style={{ color: 'var(--text-muted)', fontSize: 13.5 }}>Sin artículos publicados todavía.</p>
                  ) : (
                    <div className={styles.itemGrid}>
                      {m.categories.map((c) => {
                        const payload: DragPayload = { categoryId: c.id, name: c.name, moduleName: m.name, price: c.price }
                        return (
                          <div
                            key={c.id}
                            className={`card ${styles.itemCard} ${draggingId === c.id ? styles.itemCardDragging : ''}`}
                            draggable
                            onDragStart={(e) => handleDragStart(e, payload)}
                            onDragEnd={() => setDraggingId(null)}
                          >
                            <span className={styles.itemName}>{c.name}</span>
                            {c.description && <span className={styles.itemDesc}>{c.description}</span>}
                            <div className={styles.itemFooter}>
                              <span className={`${styles.itemPrice} ${c.price === null ? styles.itemPriceIncluded : ''}`}>
                                {c.price !== null ? `Desde ${formatMxn(c.price)}` : 'Incluido / a cotizar'}
                              </span>
                              <button
                                type="button"
                                className={styles.addBtn}
                                onClick={() => addItem(payload)}
                                aria-label={`Agregar ${c.name} al presupuesto`}
                              >
                                +
                              </button>
                            </div>
                          </div>
                        )
                      })}
                    </div>
                  )}
                </div>
              ))}
            </div>

            <div className={`card ${styles.cart}`}>
              <h2>Tu presupuesto</h2>

              <div
                className={`${styles.dropZone} ${dropActive ? styles.dropZoneActive : ''}`}
                onDragOver={(e) => {
                  e.preventDefault()
                  setDropActive(true)
                }}
                onDragLeave={() => setDropActive(false)}
                onDrop={handleDrop}
              >
                {cart.length === 0 ? (
                  <div className={styles.dropZoneEmpty}>Arrastra aquí un servicio, o usa el botón + de cualquier tarjeta.</div>
                ) : (
                  cart.map((l) => (
                    <div key={l.categoryId} className={styles.cartLine}>
                      <div className={styles.cartLineInfo}>
                        <div className={styles.cartLineName}>{l.name}</div>
                        <div className={styles.cartLineUnit}>{l.price !== null ? formatMxn(l.price) : 'Incluido / a cotizar'} c/u</div>
                      </div>
                      <div className={styles.qtyControl}>
                        <button type="button" className={styles.qtyBtn} onClick={() => changeQty(l.categoryId, -1)} aria-label="Quitar uno">
                          −
                        </button>
                        <span className={styles.qtyValue}>{l.quantity}</span>
                        <button type="button" className={styles.qtyBtn} onClick={() => changeQty(l.categoryId, 1)} aria-label="Agregar uno">
                          +
                        </button>
                      </div>
                      <button type="button" className={styles.removeBtn} onClick={() => removeItem(l.categoryId)} aria-label={`Quitar ${l.name}`}>
                        ✕
                      </button>
                    </div>
                  ))
                )}
              </div>

              <div className={styles.totalsRow}>
                <span className={styles.totalLabel}>Total estimado</span>
                <span className={styles.totalValue}>{formatMxn(total)}</span>
              </div>
              <p className={styles.totalNote}>Precios de referencia — la cotización final depende de fecha, lugar y disponibilidad.</p>

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
                    <input id="qbDate" type="date" value={eventDate} onChange={(e) => setEventDate(e.target.value)} />
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
            </div>
          </div>
        )}
      </div>
    </>
  )
}

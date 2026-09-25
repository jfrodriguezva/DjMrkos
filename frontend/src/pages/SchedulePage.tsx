import { useMutation, useQuery } from '@tanstack/react-query'
import { useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Button } from '../components/ui/Button'
import { LoadingState } from '../components/ui/PageState'
import { ApiError, api } from '../lib/apiClient'
import { usePlanner } from '../lib/usePlanner'
import type { ApiProblem, Lead } from '../types/api'
import styles from './SchedulePage.module.css'

const WEEKDAY_LABELS = ['Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb', 'Dom']

function toIsoDate(year: number, monthIndex: number, day: number): string {
  return `${year}-${String(monthIndex + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`
}

function startOfMonth(date: Date): Date {
  return new Date(date.getFullYear(), date.getMonth(), 1)
}

function buildMonthCells(viewDate: Date): (number | null)[] {
  const year = viewDate.getFullYear()
  const month = viewDate.getMonth()
  const firstWeekday = (new Date(year, month, 1).getDay() + 6) % 7 // Monday = 0
  const daysInMonth = new Date(year, month + 1, 0).getDate()
  return [...Array(firstWeekday).fill(null), ...Array.from({ length: daysInMonth }, (_, i) => i + 1)]
}

/**
 * A real availability calendar, not a decoy — every "ocupado" day comes from an actual
 * scheduled event (GET /api/availability), so a couple double-booking a date here would be
 * lying to a system that the admin dashboard also trusts.
 */
export function SchedulePage() {
  const navigate = useNavigate()
  const planner = usePlanner()
  const today = useMemo(() => new Date(), [])
  const [viewDate, setViewDate] = useState(() => startOfMonth(today))

  const { data: busyDates, isLoading } = useQuery({ queryKey: ['availability'], queryFn: () => api.get<string[]>('/availability') })
  const busySet = useMemo(() => new Set(busyDates ?? []), [busyDates])

  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [phone, setPhone] = useState('')

  const todayIso = toIsoDate(today.getFullYear(), today.getMonth(), today.getDate())
  const cells = buildMonthCells(viewDate)
  const canGoBack = startOfMonth(viewDate).getTime() > startOfMonth(today).getTime()

  const submit = useMutation({
    mutationFn: () =>
      api.post<Lead>('/leads', {
        name,
        email,
        phone: phone || null,
        eventDate: planner.preferredDate,
        message: `Solicitud de apartado de fecha desde el calendario de disponibilidad.\n\nFecha solicitada: ${planner.preferredDate}`,
      }),
  })

  const problem = submit.error instanceof ApiError ? (submit.error.problem as ApiProblem) : null

  const selectedLabel = planner.preferredDate
    ? new Date(`${planner.preferredDate}T00:00:00`).toLocaleDateString('es-MX', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' })
    : null

  return (
    <>
      <div className={styles.header}>
        <div className="container">
          <span className="eyebrow">Disponibilidad en vivo</span>
          <h1>Agendar</h1>
          <p className={styles.headerLead}>
            El calendario refleja los eventos ya confirmados — elige una fecha disponible para apartarla o continuar directo a contratar.
          </p>
        </div>
      </div>

      <div className="container section">
        {isLoading && <LoadingState label="Consultando disponibilidad…" />}

        {!isLoading && (
          <div className={styles.layout}>
            <div className={`card ${styles.calendarCard}`}>
              <div className={styles.calendarHead}>
                <button
                  type="button"
                  className={styles.navBtn}
                  disabled={!canGoBack}
                  onClick={() => setViewDate((d) => new Date(d.getFullYear(), d.getMonth() - 1, 1))}
                  aria-label="Mes anterior"
                >
                  ←
                </button>
                <span className={styles.monthLabel}>{viewDate.toLocaleDateString('es-MX', { month: 'long', year: 'numeric' })}</span>
                <button
                  type="button"
                  className={styles.navBtn}
                  onClick={() => setViewDate((d) => new Date(d.getFullYear(), d.getMonth() + 1, 1))}
                  aria-label="Mes siguiente"
                >
                  →
                </button>
              </div>

              <div className={styles.weekRow}>
                {WEEKDAY_LABELS.map((w) => (
                  <span key={w}>{w}</span>
                ))}
              </div>

              <div className={styles.dayGrid}>
                {cells.map((day, i) => {
                  if (day === null) return <span key={`empty-${i}`} className={styles.dayEmpty} />
                  const iso = toIsoDate(viewDate.getFullYear(), viewDate.getMonth(), day)
                  const isPast = iso < todayIso
                  const isBusy = busySet.has(iso)
                  const isSelected = iso === planner.preferredDate
                  return (
                    <button
                      key={iso}
                      type="button"
                      className={`${styles.day} ${isBusy ? styles.dayBusy : ''} ${isSelected ? styles.daySelected : ''}`}
                      disabled={isPast || isBusy}
                      onClick={() => planner.setPreferredDate(iso)}
                      aria-label={`${iso}${isBusy ? ' — ocupado' : ''}`}
                    >
                      {day}
                    </button>
                  )
                })}
              </div>

              <div className={styles.legend}>
                <span className={styles.legendItem}>
                  <span className={styles.legendSwatch} style={{ background: 'var(--bg-elevated)', border: '1px solid var(--border-strong)' }} />
                  Disponible
                </span>
                <span className={styles.legendItem}>
                  <span className={styles.legendSwatch} style={{ background: 'var(--red-dim)' }} />
                  Ocupado
                </span>
                <span className={styles.legendItem}>
                  <span className={styles.legendSwatch} style={{ background: 'var(--red)' }} />
                  Seleccionada
                </span>
              </div>
            </div>

            <div className={`card ${styles.sidePanel}`}>
              <h2>Tu fecha</h2>

              {!selectedLabel ? (
                <p className={styles.emptySelection}>Elige una fecha disponible en el calendario.</p>
              ) : (
                <>
                  <div className={styles.selectedDate}>{selectedLabel}</div>
                  <p className={styles.selectedHint}>Apártala dejándonos tus datos, o continúa directo a armar el contrato.</p>

                  <Button style={{ width: '100%', marginTop: 16 }} onClick={() => navigate('/contratar')}>
                    Continuar a contratar →
                  </Button>

                  {submit.isSuccess ? (
                    <p className={styles.success}>¡Gracias, {name.split(' ')[0]}! Apartamos tu fecha de forma preliminar y te contactaremos para confirmar.</p>
                  ) : (
                    <form
                      className={styles.form}
                      onSubmit={(e) => {
                        e.preventDefault()
                        submit.mutate()
                      }}
                    >
                      <div className="field">
                        <label htmlFor="schName">Nombre</label>
                        <input id="schName" value={name} onChange={(e) => setName(e.target.value)} required maxLength={120} />
                      </div>
                      <div className="field">
                        <label htmlFor="schEmail">Correo</label>
                        <input id="schEmail" type="email" value={email} onChange={(e) => setEmail(e.target.value)} required maxLength={200} />
                      </div>
                      <div className="field">
                        <label htmlFor="schPhone">Teléfono (opcional)</label>
                        <input id="schPhone" value={phone} onChange={(e) => setPhone(e.target.value)} maxLength={30} />
                      </div>
                      <Button type="submit" variant="ghost" disabled={submit.isPending}>
                        {submit.isPending ? 'Enviando…' : 'Solo apartar la fecha'}
                      </Button>
                      {problem && <p className="field-error">{problem.title}</p>}
                    </form>
                  )}
                </>
              )}
            </div>
          </div>
        )}
      </div>
    </>
  )
}

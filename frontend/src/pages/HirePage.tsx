import { useMutation, useQuery } from '@tanstack/react-query'
import { useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { CartPanel } from '../components/quote/CartPanel'
import { CatalogGrid } from '../components/quote/CatalogGrid'
import { Button } from '../components/ui/Button'
import { LoadingState } from '../components/ui/PageState'
import { ApiError, api } from '../lib/apiClient'
import { formatMxn } from '../lib/currency'
import { usePlanner } from '../lib/usePlanner'
import type { ApiProblem, Lead, MenuModule } from '../types/api'
import styles from './HirePage.module.css'

/**
 * The commitment step of the funnel: Cotizar explores prices, Agendar checks a date,
 * Contratar reviews both together and submits a firm booking request — tagged distinctly in
 * the lead message so the admin's Cotizaciones tab can tell a browse from a real ask.
 */
export function HirePage() {
  const { data: modules, isLoading } = useQuery({ queryKey: ['menu'], queryFn: () => api.get<MenuModule[]>('/menu') })
  const planner = usePlanner()

  // Starts hidden behind the empty notice below when the cart is empty — revealed by "agrégalos
  // aquí mismo". (If the cart already has items this state is never consulted: the grid always
  // shows in that case, see the render check below.)
  const [showCatalog, setShowCatalog] = useState(false)
  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [phone, setPhone] = useState('')
  const [acceptedTerms, setAcceptedTerms] = useState(false)

  const total = useMemo(() => planner.cart.reduce((sum, l) => sum + (l.price ?? 0) * l.quantity, 0), [planner.cart])

  const summaryMessage = useMemo(() => {
    const lines = planner.cart.map(
      (l) => `- ${l.quantity}× ${l.name} (${l.moduleName})${l.price ? ` — ${formatMxn(l.price * l.quantity)}` : ' — incluido/a cotizar'}`,
    )
    const parts = [
      'SOLICITUD DE CONTRATACIÓN',
      planner.cart.length > 0 ? `Servicios:\n${lines.join('\n')}` : 'Sin servicios seleccionados — contratación a definir en la llamada.',
      `Invitados estimados: ${planner.guestCount}`,
      `Total estimado: ${formatMxn(total)}`,
      planner.preferredDate ? `Fecha solicitada: ${planner.preferredDate}` : 'Fecha por confirmar.',
    ]
    return parts.join('\n\n')
  }, [planner.cart, planner.guestCount, planner.preferredDate, total])

  const submit = useMutation({
    mutationFn: () =>
      api.post<Lead>('/leads', {
        name,
        email,
        phone: phone || null,
        eventDate: planner.preferredDate,
        message: summaryMessage,
      }),
    onSuccess: () => planner.clearCart(),
  })

  const problem = submit.error instanceof ApiError ? (submit.error.problem as ApiProblem) : null

  return (
    <>
      <div className={styles.header}>
        <div className="container">
          <span className="eyebrow">Último paso</span>
          <h1>Contratar</h1>
          <p className={styles.headerLead}>
            Revisa tu presupuesto, confirma la fecha y déjanos tus datos — un asesor de DJ MrKos confirma disponibilidad final y formaliza el
            contrato contigo.
          </p>
        </div>
      </div>

      <div className="container section">
        <div className={`card ${styles.summaryCard}`}>
          <h2>Resumen</h2>
          <div className={styles.summaryRow}>
            <span className={styles.summaryLabel}>Fecha del evento</span>
            <span className={styles.summaryValue}>{planner.preferredDate ?? 'Por confirmar'}</span>
          </div>
          <div className={styles.summaryRow}>
            <span className={styles.summaryLabel}>Invitados estimados</span>
            <span className={styles.summaryValue}>{planner.guestCount}</span>
          </div>
          <div className={styles.summaryRow}>
            <span className={styles.summaryLabel}>Servicios seleccionados</span>
            <span className={styles.summaryValue}>{planner.cart.length}</span>
          </div>
          <div className={styles.summaryRow}>
            <span className={styles.summaryLabel}>Total estimado</span>
            <span className={styles.summaryValue}>{formatMxn(total)}</span>
          </div>
        </div>

        {isLoading && <LoadingState label="Cargando catálogo…" />}

        {modules && (
          <div className={styles.layout}>
            <div>
              {planner.cart.length === 0 && !showCatalog ? (
                <div className={`card ${styles.emptyNotice}`}>
                  Aún no tienes servicios en tu presupuesto. <Link to="/cotizador">Ve al cotizador</Link> o{' '}
                  <button type="button" className={styles.inlineLink} onClick={() => setShowCatalog(true)}>
                    agrégalos aquí mismo
                  </button>
                  .
                </div>
              ) : (
                <CatalogGrid modules={modules} onAdd={planner.addItem} />
              )}
            </div>

            <CartPanel title="Tu contrato" cart={planner.cart} total={total} onChangeQty={planner.changeQty} onRemove={planner.removeItem} onDropPayload={planner.addItem}>
              {submit.isSuccess ? (
                <p className={styles.success}>
                  ¡Gracias, {name.split(' ')[0]}! Recibimos tu solicitud de contratación — un asesor te contacta en menos de 24 horas para confirmar.
                </p>
              ) : (
                <form
                  className={styles.contractForm}
                  onSubmit={(e) => {
                    e.preventDefault()
                    submit.mutate()
                  }}
                >
                  <div className="field">
                    <label htmlFor="hireDate">Fecha del evento</label>
                    <input
                      id="hireDate"
                      type="date"
                      value={planner.preferredDate ?? ''}
                      onChange={(e) => planner.setPreferredDate(e.target.value || null)}
                    />
                  </div>
                  <div className="field">
                    <label htmlFor="hireName">Nombre</label>
                    <input id="hireName" value={name} onChange={(e) => setName(e.target.value)} required maxLength={120} />
                  </div>
                  <div className="field">
                    <label htmlFor="hireEmail">Correo</label>
                    <input id="hireEmail" type="email" value={email} onChange={(e) => setEmail(e.target.value)} required maxLength={200} />
                  </div>
                  <div className="field">
                    <label htmlFor="hirePhone">Teléfono</label>
                    <input id="hirePhone" value={phone} onChange={(e) => setPhone(e.target.value)} required maxLength={30} />
                  </div>

                  <label className={styles.termsRow}>
                    <input type="checkbox" checked={acceptedTerms} onChange={(e) => setAcceptedTerms(e.target.checked)} required />
                    Entiendo que esta es una solicitud de contratación preliminar — DJ MrKos confirmará disponibilidad final, condiciones de pago
                    y firmará el contrato formal por separado.
                  </label>

                  <Button type="submit" disabled={submit.isPending || !acceptedTerms}>
                    {submit.isPending ? 'Enviando…' : 'Confirmar contratación'}
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

import type { ReactNode } from 'react'
import { useState } from 'react'
import type { CartLine } from '../../lib/usePlanner'
import { formatMxn } from '../../lib/currency'
import { DRAG_MIME, type DragPayload } from './CatalogGrid'
import styles from './CartPanel.module.css'

interface CartPanelProps {
  title?: string
  cart: CartLine[]
  total: number
  onChangeQty: (categoryId: string, delta: number) => void
  onRemove: (categoryId: string) => void
  onDropPayload: (payload: DragPayload) => void
  children?: ReactNode
}

/** The drop target + running total, shared by the quote builder and the hire page. */
export function CartPanel({ title = 'Tu presupuesto', cart, total, onChangeQty, onRemove, onDropPayload, children }: CartPanelProps) {
  const [dropActive, setDropActive] = useState(false)

  function handleDrop(e: React.DragEvent) {
    e.preventDefault()
    setDropActive(false)
    const raw = e.dataTransfer.getData(DRAG_MIME)
    if (!raw) return
    onDropPayload(JSON.parse(raw) as DragPayload)
  }

  return (
    <div className={`card ${styles.cart}`}>
      <h2>{title}</h2>

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
                <button type="button" className={styles.qtyBtn} onClick={() => onChangeQty(l.categoryId, -1)} aria-label="Quitar uno">
                  −
                </button>
                <span className={styles.qtyValue}>{l.quantity}</span>
                <button type="button" className={styles.qtyBtn} onClick={() => onChangeQty(l.categoryId, 1)} aria-label="Agregar uno">
                  +
                </button>
              </div>
              <button type="button" className={styles.removeBtn} onClick={() => onRemove(l.categoryId)} aria-label={`Quitar ${l.name}`}>
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

      {children}
    </div>
  )
}

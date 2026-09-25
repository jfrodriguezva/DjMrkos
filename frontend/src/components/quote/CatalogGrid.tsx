import { useState } from 'react'
import type { CartLine } from '../../lib/usePlanner'
import { formatMxn } from '../../lib/currency'
import type { MenuModule } from '../../types/api'
import { ModuleIcon } from '../ui/ModuleIcon'
import styles from './CatalogGrid.module.css'

export type DragPayload = Omit<CartLine, 'quantity'>

export const DRAG_MIME = 'application/json'

interface CatalogGridProps {
  modules: MenuModule[]
  onAdd: (payload: DragPayload) => void
}

/**
 * The catalog side of the quote builder — every module and category straight from
 * `GET /api/menu`, each card draggable onto a `CartPanel` drop zone elsewhere on the page,
 * or added with the `+` button (the only path that reliably works on a phone: native HTML5
 * drag-and-drop isn't touch-friendly).
 */
export function CatalogGrid({ modules, onAdd }: CatalogGridProps) {
  const [draggingId, setDraggingId] = useState<string | null>(null)

  function handleDragStart(e: React.DragEvent, payload: DragPayload) {
    e.dataTransfer.setData(DRAG_MIME, JSON.stringify(payload))
    e.dataTransfer.effectAllowed = 'copy'
    setDraggingId(payload.categoryId)
  }

  return (
    <div>
      {modules.map((m) => (
        <div key={m.id} className={styles.moduleGroup}>
          <h2 className={styles.moduleHeading}>
            <ModuleIcon name={m.icon} />
            {m.name}
          </h2>
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
                      <button type="button" className={styles.addBtn} onClick={() => onAdd(payload)} aria-label={`Agregar ${c.name} al presupuesto`}>
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
  )
}

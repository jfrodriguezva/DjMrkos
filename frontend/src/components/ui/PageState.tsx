import type { ReactNode } from 'react'
import { Equalizer } from './Equalizer'
import styles from './PageState.module.css'

export function LoadingState({ label = 'Cargando…' }: { label?: string }) {
  return (
    <div className={styles.state}>
      <Equalizer />
      <p className={styles.title}>{label}</p>
    </div>
  )
}

export function ErrorState({ title = 'Algo salió mal', description, action }: { title?: string; description?: string; action?: ReactNode }) {
  return (
    <div className={styles.state}>
      <p className={styles.title}>{title}</p>
      {description && <p>{description}</p>}
      {action}
    </div>
  )
}

export function EmptyState({ title, description }: { title: string; description?: string }) {
  return (
    <div className={styles.state}>
      <p className={styles.title}>{title}</p>
      {description && <p>{description}</p>}
    </div>
  )
}

import { useQuery } from '@tanstack/react-query'
import { Link, useParams } from 'react-router-dom'
import { EmptyState, LoadingState } from '../components/ui/PageState'
import { api } from '../lib/apiClient'
import type { MenuModule } from '../types/api'
import styles from './ServiceDetailPage.module.css'

export function ServiceDetailPage() {
  const { moduleSlug } = useParams<{ moduleSlug: string }>()
  const { data: modules, isLoading } = useQuery({ queryKey: ['menu'], queryFn: () => api.get<MenuModule[]>('/menu') })

  const module_ = modules?.find((m) => m.slug === moduleSlug)

  if (isLoading) return <LoadingState label="Cargando módulo…" />

  if (!module_) {
    return (
      <div className="container section">
        <EmptyState title="No encontramos este servicio" description="Puede que el módulo haya cambiado de nombre o ya no esté activo." />
        <div style={{ textAlign: 'center' }}>
          <Link to="/servicios" className="btn btn--ghost">
            Volver a servicios
          </Link>
        </div>
      </div>
    )
  }

  return (
    <>
      <div className={styles.header}>
        <div className="container">
          <div className={styles.breadcrumb}>
            <Link to="/servicios">Servicios</Link> / {module_.name}
          </div>
          <h1>{module_.name}</h1>
        </div>
      </div>

      <div className="container section">
        {module_.categories.length === 0 ? (
          <EmptyState title="Aún no hay categorías publicadas" description="Este módulo está preparándose." />
        ) : (
          <div className={styles.grid}>
            {module_.categories.map((c) => (
              <div key={c.id} className={`card ${styles.categoryCard}`}>
                <h3>{c.name}</h3>
                {c.description && <p>{c.description}</p>}
              </div>
            ))}
          </div>
        )}
      </div>
    </>
  )
}

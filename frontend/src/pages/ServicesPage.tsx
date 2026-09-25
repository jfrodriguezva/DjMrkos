import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { EmptyState, LoadingState } from '../components/ui/PageState'
import { api } from '../lib/apiClient'
import type { MenuModule } from '../types/api'
import styles from './ServicesPage.module.css'

export function ServicesPage() {
  const { data: modules, isLoading } = useQuery({ queryKey: ['menu'], queryFn: () => api.get<MenuModule[]>('/menu') })

  return (
    <>
      <div className={styles.header}>
        <div className="container">
          <span className="eyebrow">Catálogo configurable</span>
          <h1>Servicios</h1>
          <p style={{ color: 'var(--text-secondary)', marginTop: 12 }}>
            Cada módulo y categoría se administra desde el panel del DJ — este catálogo crece con el equipo real disponible.
          </p>
        </div>
      </div>

      <div className="container section">
        {isLoading && <LoadingState label="Cargando catálogo…" />}

        {!isLoading && (!modules || modules.length === 0) && (
          <EmptyState title="El catálogo aún no tiene módulos" description="Vuelve pronto — estamos preparando el equipo." />
        )}

        {modules && modules.length > 0 && (
          <div className={styles.list}>
            {modules.map((m) => (
              <div key={m.id} className={`card ${styles.moduleRow}`}>
                <div className={styles.moduleInfo}>
                  <h3>{m.name}</h3>
                  {m.categories.length > 0 ? (
                    <div className={styles.categoryChips}>
                      {m.categories.map((c) => (
                        <span key={c.id} className={styles.chip}>
                          {c.name}
                        </span>
                      ))}
                    </div>
                  ) : (
                    <p style={{ color: 'var(--text-muted)', fontSize: 13.5, marginTop: 8 }}>Categorías por publicar.</p>
                  )}
                </div>
                <Link to={`/servicios/${m.slug}`} className="btn btn--ghost btn--sm">
                  Ver detalle
                </Link>
              </div>
            ))}
          </div>
        )}
      </div>
    </>
  )
}

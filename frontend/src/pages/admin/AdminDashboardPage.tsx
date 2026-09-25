import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { getAdminApiKey, setAdminApiKey } from '../../lib/apiClient'
import { CatalogPanel } from './panels/CatalogPanel'
import { EventsPanel } from './panels/EventsPanel'
import { LeadsPanel } from './panels/LeadsPanel'
import { LiveQueuePanel } from './panels/LiveQueuePanel'
import { TestimonialsPanel } from './panels/TestimonialsPanel'
import styles from './AdminDashboardPage.module.css'

type Tab = 'queue' | 'events' | 'catalog' | 'testimonials' | 'leads'

const TABS: { id: Tab; label: string }[] = [
  { id: 'queue', label: 'Cola en vivo' },
  { id: 'events', label: 'Eventos' },
  { id: 'catalog', label: 'Catálogo' },
  { id: 'testimonials', label: 'Testimonios' },
  { id: 'leads', label: 'Cotizaciones' },
]

export function AdminDashboardPage() {
  const navigate = useNavigate()
  const [tab, setTab] = useState<Tab>('queue')

  useEffect(() => {
    if (!getAdminApiKey()) navigate('/admin/login', { replace: true })
  }, [navigate])

  return (
    <div className={styles.shell}>
      <div className="container">
        <div className={styles.topbar}>
          <div className={styles.brand}>
            DJ <span>MrKos</span> · Panel
          </div>
          <button
            className="btn btn--ghost btn--sm"
            onClick={() => {
              setAdminApiKey(null)
              navigate('/admin/login')
            }}
          >
            Salir
          </button>
        </div>

        <div className={styles.tabs}>
          {TABS.map((t) => (
            <button
              key={t.id}
              className={`${styles.tab} ${tab === t.id ? styles.tabActive : ''}`}
              onClick={() => setTab(t.id)}
            >
              {t.label}
            </button>
          ))}
        </div>

        <div className={styles.content}>
          {tab === 'queue' && <LiveQueuePanel />}
          {tab === 'events' && <EventsPanel />}
          {tab === 'catalog' && <CatalogPanel />}
          {tab === 'testimonials' && <TestimonialsPanel />}
          {tab === 'leads' && <LeadsPanel />}
        </div>
      </div>
    </div>
  )
}

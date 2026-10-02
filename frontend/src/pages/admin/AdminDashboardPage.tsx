import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { getAdminApiKey, setAdminApiKey } from '../../lib/apiClient'
import { CalendarPanel } from './panels/CalendarPanel'
import { CatalogPanel } from './panels/CatalogPanel'
import { EventsPanel } from './panels/EventsPanel'
import { GalleryPanel } from './panels/GalleryPanel'
import { LeadsPanel } from './panels/LeadsPanel'
import { LiveQueuePanel } from './panels/LiveQueuePanel'
import { PromotionsPanel } from './panels/PromotionsPanel'
import { TestimonialsPanel } from './panels/TestimonialsPanel'
import styles from './AdminDashboardPage.module.css'

type Tab = 'queue' | 'events' | 'calendar' | 'gallery' | 'catalog' | 'promotions' | 'testimonials' | 'leads'

const TABS: { id: Tab; label: string }[] = [
  { id: 'queue', label: 'Cola en vivo' },
  { id: 'events', label: 'Eventos' },
  { id: 'calendar', label: 'Calendario' },
  { id: 'gallery', label: 'Galería' },
  { id: 'catalog', label: 'Catálogo' },
  { id: 'promotions', label: 'Promociones' },
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
          {tab === 'calendar' && <CalendarPanel />}
          {tab === 'gallery' && <GalleryPanel />}
          {tab === 'catalog' && <CatalogPanel />}
          {tab === 'promotions' && <PromotionsPanel />}
          {tab === 'testimonials' && <TestimonialsPanel />}
          {tab === 'leads' && <LeadsPanel />}
        </div>
      </div>
    </div>
  )
}

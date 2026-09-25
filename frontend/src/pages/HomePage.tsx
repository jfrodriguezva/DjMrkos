import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { Equalizer } from '../components/ui/Equalizer'
import { ModuleIcon } from '../components/ui/ModuleIcon'
import { api } from '../lib/apiClient'
import type { MenuModule, Testimonial } from '../types/api'
import styles from './HomePage.module.css'

// A fixed, non-scannable pattern — just enough to suggest "this is a QR code" in the hero.
const QR_MOCK_PATTERN = [
  1, 1, 1, 0, 1, 1, 0, 1, 0, 1, 1, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 1, 1, 0, 1,
]

export function HomePage() {
  const { data: modules } = useQuery({ queryKey: ['menu'], queryFn: () => api.get<MenuModule[]>('/menu') })
  const { data: testimonials } = useQuery({ queryKey: ['testimonials'], queryFn: () => api.get<Testimonial[]>('/testimonials') })

  return (
    <>
      <section className={styles.hero}>
        <div className="container">
          <Equalizer />
          <div className={styles.heroContent} style={{ marginTop: 24 }}>
            <span className="eyebrow">DJ · Iluminación · Sonido para eventos</span>
            <h1>El pulso de tu evento</h1>
            <p className={styles.heroTagline}>Música, luces, cabina y sonido — en un portal tan configurable como tu set.</p>
            <p className={`${styles.lead} lead`}>
              Bodas, XV años, corporativos y fiestas privadas. Armamos el equipo exacto que tu evento necesita, y tus invitados
              pueden pedir canciones en vivo escaneando un código QR — la petición te llega directo a la cabina.
            </p>
          </div>
          <div className={styles.heroActions}>
            <Link to="/cotizador" className="btn btn--primary">
              Arma tu presupuesto
            </Link>
            <Link to="/servicios" className="btn btn--ghost">
              Ver servicios
            </Link>
          </div>
        </div>
      </section>

      <section className="section">
        <div className="container">
          <div className={styles.sectionHead}>
            <div>
              <span className="eyebrow">Catálogo</span>
              <h2>Servicios</h2>
            </div>
            <Link to="/servicios" className="btn btn--ghost btn--sm">
              Ver todo
            </Link>
          </div>

          {modules && modules.length > 0 ? (
            <div className={styles.moduleGrid}>
              {modules.map((m) => (
                <Link key={m.id} to={`/servicios/${m.slug}`} className={`card ${styles.moduleCard}`}>
                  <div className={styles.moduleIcon}>
                    <ModuleIcon name={m.icon} size={20} />
                  </div>
                  <h3>{m.name}</h3>
                  <p>{m.categories.length} categoría{m.categories.length === 1 ? '' : 's'} disponible{m.categories.length === 1 ? '' : 's'}</p>
                </Link>
              ))}
            </div>
          ) : (
            <p style={{ color: 'var(--text-secondary)' }}>El catálogo se está preparando — vuelve pronto.</p>
          )}
        </div>
      </section>

      <section className={`section ${styles.qrBand}`}>
        <div className="container">
          <div className={styles.qrBandGrid}>
            <div>
              <span className="eyebrow">La pieza estrella</span>
              <h2>Pide tu canción escaneando un QR</h2>
              <ol className={styles.qrSteps}>
                <li>
                  <span className={styles.qrStepNumber}>01</span>
                  <p>Al contratar, tu evento recibe un código QR único, válido solo durante la fiesta.</p>
                </li>
                <li>
                  <span className={styles.qrStepNumber}>02</span>
                  <p>Tus invitados lo escanean con la cámara del celular — sin apps, sin registrarse.</p>
                </li>
                <li>
                  <span className={styles.qrStepNumber}>03</span>
                  <p>La canción llega en tiempo real al dashboard del DJ, directo en la cabina.</p>
                </li>
              </ol>
            </div>
            <div className={styles.qrVisual}>
              <div className={styles.qrGrid}>
                {QR_MOCK_PATTERN.map((on, i) => (
                  <span key={i} style={{ background: on ? 'var(--silver-bright)' : 'transparent' }} />
                ))}
              </div>
            </div>
          </div>
        </div>
      </section>

      {testimonials && testimonials.length > 0 && (
        <section className="section">
          <div className="container">
            <div className={styles.sectionHead}>
              <div>
                <span className="eyebrow">Lo que dicen</span>
                <h2>Testimonios</h2>
              </div>
              <Link to="/testimonios" className="btn btn--ghost btn--sm">
                Ver todos
              </Link>
            </div>
            <div className={styles.testimonialGrid}>
              {testimonials.slice(0, 3).map((t) => (
                <div key={t.id} className={`card ${styles.testimonialCard}`}>
                  <div className={styles.stars}>{'★'.repeat(t.rating)}{'☆'.repeat(5 - t.rating)}</div>
                  <p>“{t.comment}”</p>
                  <div className="name">{t.clientName}</div>
                </div>
              ))}
            </div>
          </div>
        </section>
      )}
    </>
  )
}

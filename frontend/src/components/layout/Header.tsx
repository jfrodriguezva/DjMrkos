import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { NavLink, useNavigate } from 'react-router-dom'
import { api } from '../../lib/apiClient'
import type { MenuModule } from '../../types/api'
import { Button } from '../ui/Button'
import { ModuleIcon } from '../ui/ModuleIcon'
import styles from './Header.module.css'

/**
 * The entire "Servicios" dropdown comes from `GET /api/menu` — add a module in the admin
 * panel and it appears here on the next load. Nothing about the menu is hardcoded.
 */
export function Header() {
  const [mobileOpen, setMobileOpen] = useState(false)
  const navigate = useNavigate()
  const { data: modules } = useQuery({
    queryKey: ['menu'],
    queryFn: () => api.get<MenuModule[]>('/menu'),
  })

  const link = ({ isActive }: { isActive: boolean }) => (isActive ? `${styles.navLink} ${styles.navLinkActive}` : styles.navLink)

  return (
    <header className={styles.header}>
      <div className={`container ${styles.bar}`}>
        <NavLink to="/" className={styles.brand}>
          DJ <span>MrKos</span>
        </NavLink>

        <nav className={styles.nav}>
          <NavLink to="/" className={link} end>
            Inicio
          </NavLink>

          <div className={styles.dropdownWrap}>
            <NavLink to="/servicios" className={link}>
              Servicios
            </NavLink>
            {modules && modules.length > 0 && (
              <div className={`${styles.dropdown} card`}>
                {modules.map((m) => (
                  <NavLink key={m.id} to={`/servicios/${m.slug}`} className={styles.dropdownItem}>
                    <ModuleIcon name={m.icon} size={16} />
                    {m.name}
                  </NavLink>
                ))}
              </div>
            )}
          </div>

          <NavLink to="/quienes-somos" className={link}>
            Quiénes somos
          </NavLink>
          <NavLink to="/galeria" className={link}>
            Galería
          </NavLink>
          <NavLink to="/agendar" className={link}>
            Agendar
          </NavLink>
          <NavLink to="/cotizador" className={link}>
            Cotizador
          </NavLink>
          <NavLink to="/testimonios" className={link}>
            Testimonios
          </NavLink>
        </nav>

        <div className={styles.actions}>
          <Button size="sm" onClick={() => navigate('/contratar')}>
            Contratar
          </Button>
          <button
            className={styles.menuToggle}
            aria-label={mobileOpen ? 'Cerrar menú' : 'Abrir menú'}
            aria-expanded={mobileOpen}
            onClick={() => setMobileOpen((v) => !v)}
          >
            {mobileOpen ? '✕' : '☰'}
          </button>
        </div>
      </div>

      {mobileOpen && (
        <div className={styles.mobilePanel}>
          <NavLink to="/" className={styles.mobileLink} onClick={() => setMobileOpen(false)} end>
            Inicio
          </NavLink>
          <NavLink to="/servicios" className={styles.mobileLink} onClick={() => setMobileOpen(false)}>
            Servicios
          </NavLink>
          <NavLink to="/quienes-somos" className={styles.mobileLink} onClick={() => setMobileOpen(false)}>
            Quiénes somos
          </NavLink>
          <NavLink to="/galeria" className={styles.mobileLink} onClick={() => setMobileOpen(false)}>
            Galería
          </NavLink>
          <NavLink to="/agendar" className={styles.mobileLink} onClick={() => setMobileOpen(false)}>
            Agendar
          </NavLink>
          <NavLink to="/cotizador" className={styles.mobileLink} onClick={() => setMobileOpen(false)}>
            Cotizador
          </NavLink>
          <NavLink to="/contratar" className={styles.mobileLink} onClick={() => setMobileOpen(false)}>
            Contratar
          </NavLink>
          <NavLink to="/testimonios" className={styles.mobileLink} onClick={() => setMobileOpen(false)}>
            Testimonios
          </NavLink>
          <NavLink to="/contacto" className={styles.mobileLink} onClick={() => setMobileOpen(false)}>
            Contacto
          </NavLink>
        </div>
      )}
    </header>
  )
}

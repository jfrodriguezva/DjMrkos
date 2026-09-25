import { Link } from 'react-router-dom'
import styles from './Footer.module.css'

export function Footer() {
  return (
    <footer className={styles.footer}>
      <div className="container">
        <div className={styles.grid}>
          <div>
            <div className={styles.brand}>
              DJ <span>MrKos</span>
            </div>
            <p className={styles.tagline}>Música, luces, cabina y sonido — el pulso de tu evento, de principio a fin.</p>
          </div>

          <div>
            <div className={styles.heading}>Portal</div>
            <div className={styles.links}>
              <Link to="/servicios">Servicios</Link>
              <Link to="/quienes-somos">Quiénes somos</Link>
              <Link to="/testimonios">Testimonios</Link>
              <Link to="/contacto">Contacto</Link>
            </div>
          </div>

          <div>
            <div className={styles.heading}>Para tu evento</div>
            <div className={styles.links}>
              <span>¿Ya contrataste? Pide tu canción escaneando el QR de tu evento.</span>
            </div>
          </div>
        </div>

        <div className={styles.bottom}>
          <span>© {new Date().getFullYear()} DJ MrKos</span>
          <Link to="/admin/login">Panel DJ</Link>
        </div>
      </div>
    </footer>
  )
}

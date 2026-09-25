import { Link } from 'react-router-dom'

export function NotFoundPage() {
  return (
    <div className="container section" style={{ textAlign: 'center' }}>
      <span className="eyebrow">404</span>
      <h1 style={{ marginTop: 12 }}>Esta página se perdió en la pista</h1>
      <p style={{ color: 'var(--text-secondary)', margin: '16px auto', maxWidth: '42ch' }}>
        Puede que el enlace esté roto o que la página haya cambiado de lugar.
      </p>
      <Link to="/" className="btn btn--primary">
        Volver al inicio
      </Link>
    </div>
  )
}

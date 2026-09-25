import { useMutation } from '@tanstack/react-query'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import { Button } from '../components/ui/Button'
import { ApiError, api } from '../lib/apiClient'
import type { ApiProblem, Lead } from '../types/api'
import styles from './ContactPage.module.css'

export function ContactPage() {
  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [phone, setPhone] = useState('')
  const [eventDate, setEventDate] = useState('')
  const [message, setMessage] = useState('')

  const submit = useMutation({
    mutationFn: () =>
      api.post<Lead>('/leads', {
        name,
        email,
        phone: phone || null,
        eventDate: eventDate || null,
        message,
      }),
  })

  const problem = submit.error instanceof ApiError ? (submit.error.problem as ApiProblem) : null

  return (
    <>
      <div className={styles.header}>
        <div className="container">
          <span className="eyebrow">Contacto directo</span>
          <h1>Escríbenos</h1>
        </div>
      </div>

      <div className="container section">
        <div className={styles.layout}>
          <div>
            <p style={{ color: 'var(--text-secondary)' }}>
              ¿Ya sabes qué servicios quieres? Arma tu presupuesto en el{' '}
              <Link to="/cotizador" style={{ color: 'var(--steel-bright)' }}>
                cotizador interactivo
              </Link>
              . Si prefieres platicarlo directo — dudas, fechas ajustadas o algo fuera del catálogo — escríbenos aquí y te
              respondemos en persona.
            </p>
            <div className={styles.infoList}>
              <div className={styles.infoItem}>
                <div className="label">Respuesta</div>
                <div className="value">En menos de 24 horas</div>
              </div>
              <div className={styles.infoItem}>
                <div className="label">Cobertura</div>
                <div className="value">Bodas · XV años · Corporativos · Fiestas privadas</div>
              </div>
            </div>
          </div>

          <div className={`card ${styles.formCard}`}>
            {submit.isSuccess ? (
              <p className={styles.success}>
                ¡Gracias, {name.split(' ')[0]}! Recibimos tu mensaje y te contactaremos muy pronto.
              </p>
            ) : (
              <form
                className={styles.form}
                onSubmit={(e) => {
                  e.preventDefault()
                  submit.mutate()
                }}
              >
                <div className={styles.row2}>
                  <div className="field">
                    <label htmlFor="name">Nombre</label>
                    <input id="name" value={name} onChange={(e) => setName(e.target.value)} required maxLength={120} />
                  </div>
                  <div className="field">
                    <label htmlFor="email">Correo</label>
                    <input id="email" type="email" value={email} onChange={(e) => setEmail(e.target.value)} required maxLength={200} />
                  </div>
                </div>

                <div className={styles.row2}>
                  <div className="field">
                    <label htmlFor="phone">Teléfono (opcional)</label>
                    <input id="phone" value={phone} onChange={(e) => setPhone(e.target.value)} maxLength={30} />
                  </div>
                  <div className="field">
                    <label htmlFor="eventDate">Fecha del evento (opcional)</label>
                    <input id="eventDate" type="date" value={eventDate} onChange={(e) => setEventDate(e.target.value)} />
                  </div>
                </div>

                <div className="field">
                  <label htmlFor="message">¿En qué te ayudamos?</label>
                  <textarea
                    id="message"
                    value={message}
                    onChange={(e) => setMessage(e.target.value)}
                    required
                    maxLength={1000}
                    placeholder="Tu duda, tu evento, o algo que no encontraste en el catálogo…"
                  />
                </div>

                <Button type="submit" disabled={submit.isPending}>
                  {submit.isPending ? 'Enviando…' : 'Enviar mensaje'}
                </Button>

                {problem && <p className="field-error">{problem.title}</p>}
              </form>
            )}
          </div>
        </div>
      </div>
    </>
  )
}

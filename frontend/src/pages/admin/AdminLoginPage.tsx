import { useMutation } from '@tanstack/react-query'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Button } from '../../components/ui/Button'
import { Equalizer } from '../../components/ui/Equalizer'
import { setAdminApiKey } from '../../lib/apiClient'
import type { Lead } from '../../types/api'
import { api } from '../../lib/apiClient'
import styles from './AdminLoginPage.module.css'

/**
 * v1 auth is a single shared API key (see docs/ARCHITECTURE.md for the upgrade path once
 * there's more than one admin). This page just verifies the key actually works before
 * committing to it, by calling a real protected endpoint.
 */
export function AdminLoginPage() {
  const navigate = useNavigate()
  const [apiKey, setApiKey] = useState('')

  const verify = useMutation({
    mutationFn: async () => {
      setAdminApiKey(apiKey)
      try {
        await api.get<Lead[]>('/admin/leads', true)
      } catch (err) {
        setAdminApiKey(null)
        throw err
      }
    },
    onSuccess: () => navigate('/admin'),
  })

  return (
    <div className={styles.screen}>
      <Equalizer />
      <div className={styles.brand}>
        DJ <span>MrKos</span> · Panel
      </div>

      <div className={`card ${styles.card}`}>
        <h1 style={{ fontSize: 20 }}>Acceso del DJ</h1>
        <form
          className={styles.form}
          onSubmit={(e) => {
            e.preventDefault()
            verify.mutate()
          }}
        >
          <div className="field">
            <label htmlFor="apiKey">API key</label>
            <input
              id="apiKey"
              type="password"
              value={apiKey}
              onChange={(e) => setApiKey(e.target.value)}
              required
              autoFocus
              placeholder="••••••••••••"
            />
          </div>
          <Button type="submit" disabled={verify.isPending}>
            {verify.isPending ? 'Verificando…' : 'Entrar'}
          </Button>
          {verify.isError && <p className="field-error">API key inválida.</p>}
        </form>
      </div>
    </div>
  )
}

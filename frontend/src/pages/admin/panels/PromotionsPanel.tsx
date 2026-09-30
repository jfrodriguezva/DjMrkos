import { useMutation, useQueries, useQuery, useQueryClient } from '@tanstack/react-query'
import { useMemo, useState } from 'react'
import { Button } from '../../../components/ui/Button'
import { EmptyState, LoadingState } from '../../../components/ui/PageState'
import { api } from '../../../lib/apiClient'
import type { CategoryAdmin, ModuleAdmin, PromotionAdmin } from '../../../types/api'
import styles from './CatalogPanel.module.css'

type Target = 'module' | 'category'

/**
 * Configura descuentos por porcentaje sobre un módulo completo o una sola categoría — el
 * catálogo público (`/menu`) y el cotizador los recogen automáticamente en el siguiente request.
 */
export function PromotionsPanel() {
  const queryClient = useQueryClient()
  const { data: modules, isLoading: loadingModules } = useQuery({ queryKey: ['admin-modules'], queryFn: () => api.get<ModuleAdmin[]>('/admin/modules', true) })
  const { data: promotions, isLoading: loadingPromotions } = useQuery({
    queryKey: ['admin-promotions'],
    queryFn: () => api.get<PromotionAdmin[]>('/admin/promotions', true),
  })

  const categoryQueries = useQueries({
    queries: (modules ?? []).map((m) => ({
      queryKey: ['categories', m.id],
      queryFn: () => api.get<CategoryAdmin[]>(`/modules/${m.id}/categories`),
      enabled: !!modules,
    })),
  })
  const allCategories = useMemo(() => categoryQueries.flatMap((q) => q.data ?? []), [categoryQueries])

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ['admin-promotions'] })
    queryClient.invalidateQueries({ queryKey: ['menu'] })
  }

  const moduleName = (id: string | null) => (id ? modules?.find((m) => m.id === id)?.name : undefined)
  const categoryLabel = (id: string | null) => {
    const category = id ? allCategories.find((c) => c.id === id) : undefined
    if (!category) return undefined
    const parentModule = moduleName(category.moduleId)
    return parentModule ? `${category.name} (${parentModule})` : category.name
  }

  const toggleActive = useMutation({
    mutationFn: (p: PromotionAdmin) => api.put<PromotionAdmin>(`/admin/promotions/${p.id}`, { label: p.label, discountPercentage: p.discountPercentage, isActive: !p.isActive }, true),
    onSuccess: invalidate,
  })

  const remove = useMutation({
    mutationFn: (id: string) => api.del(`/admin/promotions/${id}`, true),
    onSuccess: invalidate,
  })

  const isLoading = loadingModules || loadingPromotions

  return (
    <div className={styles.layout}>
      <div className={`card ${styles.panel}`}>
        <h2>Promociones activas</h2>
        {isLoading && <LoadingState label="Cargando promociones…" />}
        {!isLoading && (!promotions || promotions.length === 0) && <EmptyState title="Sin promociones todavía" />}

        <div className={styles.categoryList}>
          {promotions?.map((p) => (
            <div key={p.id} className={`card ${styles.categoryRow}`}>
              <div className={styles.moduleRowName}>{p.label}</div>
              <div className={styles.moduleRowMeta}>
                -{p.discountPercentage}% · {p.moduleId ? `Módulo: ${moduleName(p.moduleId) ?? '…'}` : `Categoría: ${categoryLabel(p.categoryId) ?? '…'}`}
              </div>
              <div className={styles.categoryFooter}>
                <span className="badge">{p.isActive ? 'Activa' : 'Inactiva'}</span>
                <div className={styles.rowActions}>
                  <Button size="sm" variant="ghost" onClick={() => toggleActive.mutate(p)} disabled={toggleActive.isPending}>
                    {p.isActive ? 'Desactivar' : 'Activar'}
                  </Button>
                  <Button size="sm" variant="ghost" onClick={() => remove.mutate(p.id)} disabled={remove.isPending}>
                    Eliminar
                  </Button>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>

      <div className={`card ${styles.panel}`}>
        <h2>Nueva promoción</h2>
        <NewPromotionForm modules={modules ?? []} categories={allCategories} onCreated={invalidate} />
      </div>
    </div>
  )
}

function NewPromotionForm({ modules, categories, onCreated }: { modules: ModuleAdmin[]; categories: CategoryAdmin[]; onCreated: () => void }) {
  const [target, setTarget] = useState<Target>('module')
  const [moduleId, setModuleId] = useState('')
  const [categoryId, setCategoryId] = useState('')
  const [label, setLabel] = useState('')
  const [discountPercentage, setDiscountPercentage] = useState('10')

  const create = useMutation({
    mutationFn: () =>
      api.post<PromotionAdmin>(
        '/admin/promotions',
        {
          moduleId: target === 'module' ? moduleId || null : null,
          categoryId: target === 'category' ? categoryId || null : null,
          label,
          discountPercentage: Number(discountPercentage),
        },
        true,
      ),
    onSuccess: () => {
      setLabel('')
      setDiscountPercentage('10')
      setModuleId('')
      setCategoryId('')
      onCreated()
    },
  })

  const canSubmit = label.trim() !== '' && Number(discountPercentage) > 0 && (target === 'module' ? moduleId !== '' : categoryId !== '')

  return (
    <form
      className={styles.inlineForm}
      onSubmit={(e) => {
        e.preventDefault()
        if (canSubmit) create.mutate()
      }}
    >
      <div className="field">
        <label>Aplica a</label>
        <div className={styles.rowActions}>
          <label className={styles.toggle}>
            <input type="radio" name="target" checked={target === 'module'} onChange={() => setTarget('module')} />
            Módulo completo
          </label>
          <label className={styles.toggle}>
            <input type="radio" name="target" checked={target === 'category'} onChange={() => setTarget('category')} />
            Una categoría
          </label>
        </div>
      </div>

      {target === 'module' ? (
        <div className="field">
          <label htmlFor="promoModule">Módulo</label>
          <select id="promoModule" value={moduleId} onChange={(e) => setModuleId(e.target.value)} required>
            <option value="">Selecciona un módulo…</option>
            {modules.map((m) => (
              <option key={m.id} value={m.id}>
                {m.name}
              </option>
            ))}
          </select>
        </div>
      ) : (
        <div className="field">
          <label htmlFor="promoCategory">Categoría</label>
          <select id="promoCategory" value={categoryId} onChange={(e) => setCategoryId(e.target.value)} required>
            <option value="">Selecciona una categoría…</option>
            {categories.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name} ({modules.find((m) => m.id === c.moduleId)?.name ?? '…'})
              </option>
            ))}
          </select>
        </div>
      )}

      <div className={styles.inlineFormRow}>
        <div className="field">
          <label htmlFor="promoLabel">Nombre de la promoción</label>
          <input id="promoLabel" value={label} onChange={(e) => setLabel(e.target.value)} required maxLength={200} placeholder="Ej. Promo de temporada" />
        </div>
        <div className="field">
          <label htmlFor="promoDiscount">Descuento (%)</label>
          <input
            id="promoDiscount"
            type="number"
            min={1}
            max={100}
            step="1"
            value={discountPercentage}
            onChange={(e) => setDiscountPercentage(e.target.value)}
            required
          />
        </div>
      </div>

      <Button type="submit" size="sm" disabled={create.isPending || !canSubmit}>
        {create.isPending ? 'Creando…' : 'Crear promoción'}
      </Button>
    </form>
  )
}

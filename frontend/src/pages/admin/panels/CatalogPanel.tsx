import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { Button } from '../../../components/ui/Button'
import { EmptyState, LoadingState } from '../../../components/ui/PageState'
import { api } from '../../../lib/apiClient'
import type { CategoryAdmin, ModuleAdmin } from '../../../types/api'
import styles from './CatalogPanel.module.css'

/**
 * The only UI for what the price field this session added actually configures — without it,
 * setting a category's price would mean going through Swagger by hand.
 */
export function CatalogPanel() {
  const queryClient = useQueryClient()
  const { data: modules, isLoading } = useQuery({ queryKey: ['admin-modules'], queryFn: () => api.get<ModuleAdmin[]>('/admin/modules', true) })
  const [selectedModuleId, setSelectedModuleId] = useState<string | null>(null)

  const invalidateModules = () => {
    queryClient.invalidateQueries({ queryKey: ['admin-modules'] })
    queryClient.invalidateQueries({ queryKey: ['menu'] })
  }

  const [newModuleName, setNewModuleName] = useState('')
  const createModule = useMutation({
    mutationFn: () => api.post<ModuleAdmin>('/admin/modules', { name: newModuleName, icon: null, displayOrder: modules?.length ?? 0 }, true),
    onSuccess: () => {
      setNewModuleName('')
      invalidateModules()
    },
  })

  const toggleModuleActive = useMutation({
    mutationFn: (m: ModuleAdmin) =>
      api.put<ModuleAdmin>(`/admin/modules/${m.id}`, { name: m.name, icon: m.icon, displayOrder: m.displayOrder, isActive: !m.isActive }, true),
    onSuccess: invalidateModules,
  })

  const selectedModule = modules?.find((m) => m.id === selectedModuleId) ?? modules?.[0] ?? null

  return (
    <div className={styles.layout}>
      <div className={`card ${styles.panel}`}>
        <h2>Módulos</h2>
        {isLoading && <LoadingState label="Cargando módulos…" />}
        {!isLoading && (!modules || modules.length === 0) && <EmptyState title="Sin módulos todavía" />}

        <div className={styles.moduleList}>
          {modules?.map((m) => (
            <button
              key={m.id}
              type="button"
              className={`${styles.moduleRow} ${(selectedModule?.id ?? '') === m.id ? styles.moduleRowActive : ''}`}
              onClick={() => setSelectedModuleId(m.id)}
            >
              <div>
                <div className={styles.moduleRowName}>{m.name}</div>
                <div className={styles.moduleRowMeta}>{m.isActive ? 'Activo' : 'Inactivo'} · orden {m.displayOrder}</div>
              </div>
              <span
                role="button"
                tabIndex={0}
                className="badge"
                onClick={(e) => {
                  e.stopPropagation()
                  toggleModuleActive.mutate(m)
                }}
              >
                {m.isActive ? 'Desactivar' : 'Activar'}
              </span>
            </button>
          ))}
        </div>

        <form
          className={styles.inlineForm}
          onSubmit={(e) => {
            e.preventDefault()
            createModule.mutate()
          }}
        >
          <div className="field">
            <label htmlFor="newModuleName">Nuevo módulo</label>
            <input id="newModuleName" value={newModuleName} onChange={(e) => setNewModuleName(e.target.value)} required maxLength={80} placeholder="Ej. Fotografía" />
          </div>
          <Button type="submit" size="sm" disabled={createModule.isPending}>
            {createModule.isPending ? 'Creando…' : 'Agregar módulo'}
          </Button>
        </form>
      </div>

      <div className={`card ${styles.panel}`}>
        {selectedModule ? (
          <CategoriesEditor module={selectedModule} />
        ) : (
          <EmptyState title="Selecciona un módulo" description="Elige un módulo a la izquierda para ver y editar sus categorías." />
        )}
      </div>
    </div>
  )
}

function CategoriesEditor({ module: mod }: { module: ModuleAdmin }) {
  const queryClient = useQueryClient()
  const { data: categories, isLoading } = useQuery({
    queryKey: ['categories', mod.id],
    queryFn: () => api.get<CategoryAdmin[]>(`/modules/${mod.id}/categories`),
  })

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ['categories', mod.id] })
    queryClient.invalidateQueries({ queryKey: ['menu'] })
  }

  return (
    <>
      <h2>Categorías de {mod.name}</h2>
      {isLoading && <LoadingState label="Cargando categorías…" />}
      {!isLoading && (!categories || categories.length === 0) && <EmptyState title="Sin categorías en este módulo" />}

      <div className={styles.categoryList}>
        {categories?.map((c) => (
          <CategoryRow key={c.id} category={c} onSaved={invalidate} />
        ))}
      </div>

      <NewCategoryForm moduleId={mod.id} nextOrder={categories?.length ?? 0} onCreated={invalidate} />
    </>
  )
}

function CategoryRow({ category, onSaved }: { category: CategoryAdmin; onSaved: () => void }) {
  const [name, setName] = useState(category.name)
  const [description, setDescription] = useState(category.description ?? '')
  const [price, setPrice] = useState(category.price !== null ? String(category.price) : '')
  const [isActive, setIsActive] = useState(category.isActive)

  const save = useMutation({
    mutationFn: () =>
      api.put<CategoryAdmin>(
        `/admin/categories/${category.id}`,
        {
          name,
          description: description || null,
          imageUrl: null,
          price: price.trim() === '' ? null : Number(price),
          displayOrder: category.displayOrder,
          isActive,
        },
        true,
      ),
    onSuccess: onSaved,
  })

  const remove = useMutation({
    mutationFn: () => api.del(`/admin/categories/${category.id}`, true),
    onSuccess: onSaved,
  })

  return (
    <div className={`card ${styles.categoryRow}`}>
      <div className={styles.categoryFields}>
        <div className="field">
          <label>Nombre</label>
          <input value={name} onChange={(e) => setName(e.target.value)} maxLength={80} />
        </div>
        <div className="field">
          <label>Descripción</label>
          <input value={description} onChange={(e) => setDescription(e.target.value)} maxLength={500} />
        </div>
        <div className="field">
          <label>Precio (MXN)</label>
          <input type="number" min={0} step="50" value={price} onChange={(e) => setPrice(e.target.value)} placeholder="Incluido" />
        </div>
      </div>
      <div className={styles.categoryFooter}>
        <label className={styles.toggle}>
          <input type="checkbox" checked={isActive} onChange={(e) => setIsActive(e.target.checked)} />
          Visible en el portal
        </label>
        <div className={styles.rowActions}>
          <Button size="sm" variant="ghost" onClick={() => remove.mutate()} disabled={remove.isPending}>
            Eliminar
          </Button>
          <Button size="sm" onClick={() => save.mutate()} disabled={save.isPending}>
            {save.isPending ? 'Guardando…' : 'Guardar'}
          </Button>
        </div>
      </div>
    </div>
  )
}

function NewCategoryForm({ moduleId, nextOrder, onCreated }: { moduleId: string; nextOrder: number; onCreated: () => void }) {
  const [name, setName] = useState('')
  const [description, setDescription] = useState('')
  const [price, setPrice] = useState('')

  const create = useMutation({
    mutationFn: () =>
      api.post<CategoryAdmin>(
        '/admin/categories',
        { moduleId, name, description: description || null, imageUrl: null, price: price.trim() === '' ? null : Number(price), displayOrder: nextOrder },
        true,
      ),
    onSuccess: () => {
      setName('')
      setDescription('')
      setPrice('')
      onCreated()
    },
  })

  return (
    <form
      className={styles.inlineForm}
      onSubmit={(e) => {
        e.preventDefault()
        create.mutate()
      }}
    >
      <div className={styles.inlineFormRow}>
        <div className="field">
          <label htmlFor="newCatName">Nueva categoría</label>
          <input id="newCatName" value={name} onChange={(e) => setName(e.target.value)} required maxLength={80} placeholder="Ej. Pajara Peggy" />
        </div>
        <div className="field">
          <label htmlFor="newCatPrice">Precio (MXN, opcional)</label>
          <input id="newCatPrice" type="number" min={0} step="50" value={price} onChange={(e) => setPrice(e.target.value)} placeholder="Incluido" />
        </div>
      </div>
      <div className="field">
        <label htmlFor="newCatDesc">Descripción (opcional)</label>
        <input id="newCatDesc" value={description} onChange={(e) => setDescription(e.target.value)} maxLength={500} />
      </div>
      <Button type="submit" size="sm" disabled={create.isPending}>
        {create.isPending ? 'Creando…' : 'Agregar categoría'}
      </Button>
    </form>
  )
}
